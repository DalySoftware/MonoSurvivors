float4x4 MatrixTransform;

texture Texture;
sampler TextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
    AddressU = Clamp;
    AddressV = Clamp;
};

// Tunables (give defaults so you see *something* even if a param isn’t set)
float ScanlineStrength = 0.45;   // 0..1
float ScanlineStepPx   = 2.0;    // scanline height in SCREEN pixels (1,2,3...)

float GrilleStrength = 0.12; // 0..0.25 (keep subtle)
float GrilleStepPx   = 3.0;  // width in SCREEN pixels (2..6)

float VignetteStrength = 0.20;   // 0..1
float VignetteWidthX = 0.08; // fraction of screen width (0.03..0.12)
float VignetteWidthY = 0.10; // fraction of screen height
float VignetteCurve  = 4.0;  // 2..8 (higher = more “only at the edge”)
float CornerBoost    = 0.35; // 0..1 (extra corner emphasis)

float BloomStrength  = 0.35; // 0..1 (try 0.2..0.6)
float BloomThreshold = 0.70; // 0..1 (0.6..0.85)
float BloomRadiusPx  = 2.0;  // in SOURCE pixels (1..4)
float BlurRadiusPx   = 1.0;   // in SOURCE pixels (0.5..2.5)
float BlurStrength   = 0.35;  // 0..1 (0.2..0.6 usually)
float ChromaticBleedPx = 0.75; // in SOURCE pixels, try 0.25..1.5
float ChromaticBleedX  = 1.0;  // 0..1 (1 = horizontal, 0 = vertical)
float ChromaStrength = 0.35; // 0..1 (try 0.15..0.6)

float Gain             = 1.10;   // 1..1.3

float2 SourceTexel = float2(1.0 / 1920.0, 1.0 / 1080.0); // set from C#

struct VSIn
{
    float4 Position : POSITION0; // SpriteBatch space: pixel coords
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOut
{
    float4 Position : POSITION0;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float2 ScreenPx : TEXCOORD1; // pass pixel coords to PS
};

VSOut VS(VSIn i)
{
    VSOut o;
    o.ScreenPx = i.Position.xy;                // <-- crucial: screen pixel coords
    o.Position = mul(i.Position, MatrixTransform);
    o.Color = i.Color;
    o.TexCoord = i.TexCoord;
    return o;
}

float3 ExtractHighlights(float3 c)
{
    float bright = max(c.r, max(c.g, c.b));
    float w = saturate((bright - BloomThreshold) / max(1e-5, 1.0 - BloomThreshold));
    // soften knee a bit
    w = w * w * (3.0 - 2.0 * w); // smoothstep
    return c * w;
}

float ExtractWeight(float3 c)
{
    float bright = max(c.r, max(c.g, c.b));
    float w = saturate((bright - BloomThreshold) / max(1e-5, 1.0 - BloomThreshold));
    w = w * w * (3.0 - 2.0 * w); // smoothstep knee
    return w;
}

float4 PS(VSOut i) : COLOR0
{
    // Base blur taps
    float2 o  = SourceTexel * BlurRadiusPx;
    
    float4 c0  = tex2D(TextureSampler, i.TexCoord);
    float4 cR  = tex2D(TextureSampler, i.TexCoord + float2( o.x, 0));
    float4 cL  = tex2D(TextureSampler, i.TexCoord + float2(-o.x, 0));
    float4 cU  = tex2D(TextureSampler, i.TexCoord + float2(0,  o.y));
    float4 cD  = tex2D(TextureSampler, i.TexCoord + float2(0, -o.y));
    
    float3 blur =
        c0.rgb * 0.40 +
        cR.rgb * 0.15 +
        cL.rgb * 0.15 +
        cU.rgb * 0.15 +
        cD.rgb * 0.15;
    
    // Base image (sharp + small blur)
    float3 col = lerp(c0.rgb, blur, BlurStrength);
    float a = c0.a;
    
    float2 bo = SourceTexel * BloomRadiusPx;
    float2 uv = i.TexCoord;
    
    // chromatic offset in texcoords
    float cx = saturate(ChromaticBleedX); // clamp 0..1
    float2 co = SourceTexel * ChromaticBleedPx;
    co *= float2(cx, 1.0 - cx);
    
    // Center tap (reuse c0 for G + weight, only sample R/B shifted)
    float3 s0 = c0.rgb;
    float  w0 = ExtractWeight(s0);
    float  r0 = tex2D(TextureSampler, i.TexCoord + co).r;
    float  b0c = tex2D(TextureSampler, i.TexCoord - co).b;
    float3 cb0 = float3(r0, s0.g, b0c) * w0;
    
    // Extra visible chroma fringe (directional: red on one side, cyan on the other)
    float3 center = s0; // c0.rgb
    
    // Neighbours along chroma axis (co points right when ChromaticBleedX=1)
    float3 cPlus  = tex2D(TextureSampler, uv + co).rgb; // right neighbour
    float3 cMinus = tex2D(TextureSampler, uv - co).rgb; // left neighbour
    
    const float3 LUMA = float3(0.299, 0.587, 0.114);
    float lum0    = dot(center, LUMA);
    float lumPlus = dot(cPlus,  LUMA);
    float lumMinus= dot(cMinus, LUMA);
    
    // Only apply onto darker pixels (keeps the white outline from shifting colour)
    float darkFactor = saturate(1.0 - lum0 * 1.25);
    
    // Neighbour highlight gating (so bleed comes from bright edges even on dark pixels)
    float wPlus  = ExtractWeight(cPlus);
    float wMinus = ExtractWeight(cMinus);
    
    // “Is neighbour clearly brighter than me?”
    float eps = 0.02;
    float rightIsBright = step(lum0 + eps, lumPlus);
    float leftIsBright  = step(lum0 + eps, lumMinus);
    
    // Add RED bleed when bright is on the LEFT (so red shows on the right-hand dark side)
    float redAdd = max(0.0, cMinus.r - center.r);
    col.r += redAdd * (ChromaStrength * 1.25) * wMinus * leftIsBright * darkFactor;
    
    // Add CYAN bleed when bright is on the RIGHT (so cyan shows on the left-hand dark side)
    float gAdd = max(0.0, cPlus.g - center.g);
    float bAdd = max(0.0, cPlus.b - center.b);
    col.g += gAdd * (ChromaStrength * 0.65) * wPlus * rightIsBright * darkFactor;
    col.b += bAdd * (ChromaStrength * 0.80) * wPlus * rightIsBright * darkFactor;

    // Right tap
    float2 uvR = i.TexCoord + float2( bo.x, 0);
    float3 sR = cR.rgb;
    float  wR = ExtractWeight(sR);
    float  rR = tex2D(TextureSampler, uv + float2( bo.x, 0) + co).r;
    float  bR = tex2D(TextureSampler, uv + float2( bo.x, 0) - co).b;
    float3 cbR = float3(rR, sR.g, bR) * wR;
    
    // Left tap
    float2 uvL = i.TexCoord + float2(-bo.x, 0);
    float3 sL = cL.rgb;
    float  wL = ExtractWeight(sL);
    float  rL = tex2D(TextureSampler, uv + float2(-bo.x, 0) + co).r;
    float  bL = tex2D(TextureSampler, uv + float2(-bo.x, 0) - co).b;
    float3 cbL = float3(rL, sL.g, bL) * wL;
    
    // Up tap
    float2 uvU = i.TexCoord + float2(0,  bo.y);
    float3 sU = cU.rgb;
    float  wU = ExtractWeight(sU);
    float  rU = tex2D(TextureSampler, uv + float2(0,  bo.y) + co).r;
    float  bU = tex2D(TextureSampler, uv + float2(0,  bo.y) - co).b;
    float3 cbU = float3(rU, sU.g, bU) * wU;
    
    // Down tap
    float2 uvD = i.TexCoord + float2(0, -bo.y);
    float3 sD = cD.rgb;
    float  wD = ExtractWeight(sD);
    float  rD = tex2D(TextureSampler, uv + float2(0, -bo.y) + co).r;
    float  bD = tex2D(TextureSampler, uv + float2(0, -bo.y) - co).b;
    float3 cbD = float3(rD, sD.g, bD) * wD;
    
    // Combine (same weights as your blur)
    float3 chromaBloom =
        cb0 * 0.40 +
        cbR * 0.15 +
        cbL * 0.15 +
        cbU * 0.15 +
        cbD * 0.15;
    
    col += chromaBloom * BloomStrength;

    float2 p = i.ScreenPx;

    // Horizontal scanlines (visible at any scale because it's in SCREEN pixels)
    float fy = frac(p.y / ScanlineStepPx);
    float tri = abs(fy - 0.5) * 2.0;   // 0..1
    tri = tri * tri;                   // sharpen
    // centered so it doesn't only dim
    float scanMul = 1.0 + ScanlineStrength * (0.5 - tri) * 2.0; // [1-s, 1+s]
    col *= scanMul;
    
    // Vertical grille lines (aperture-grille-ish). Keep strength subtle.
    float fx = frac(p.x / GrilleStepPx);
    float triX = abs(fx - 0.5) * 2.0;     // 0..1
    triX = triX * triX;                   // sharpen
    float grilleMul = 1.0 + GrilleStrength * (0.5 - triX) * 2.0; // [1-g, 1+g]
    col *= grilleMul;
    
    // Border-style vignette (edge band + smoother corner curve)
    float2 e = min(uv, 1.0 - uv);
    
    // Raw band coords (0 inside, 1 at edge)
    float bx0 = saturate((VignetteWidthX - e.x) / max(VignetteWidthX, 1e-5));
    float by0 = saturate((VignetteWidthY - e.y) / max(VignetteWidthY, 1e-5));
    
    // Shape the edge falloff
    float bx = pow(bx0, VignetteCurve);
    float by = pow(by0, VignetteCurve);
    
    // Union of edge bands (what you already had)
    float edgeT = 1.0 - (1.0 - bx) * (1.0 - by);
    
    // Corner term, but based on *unshaped* bx0/by0 so it starts curving earlier.
    // Use x^(1.5) = x*sqrt(x) (cheaper than pow)
    float corner = bx0 * by0;
    corner = corner * sqrt(max(corner, 1e-6)); // ^1.5
    
    edgeT = saturate(edgeT + CornerBoost * corner);
    
    // Apply
    col *= (1.0 - VignetteStrength * edgeT);

    col *= Gain;

    return float4(saturate(col), a) * i.Color;
}

technique T
{
    pass P
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PS();
    }
}