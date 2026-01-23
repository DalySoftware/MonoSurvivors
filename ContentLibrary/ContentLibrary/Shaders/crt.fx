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
    
    // Bloom samples (separate radius so it actually "bleeds")
    float3 b0 = ExtractHighlights(c0.rgb);
    float3 bR = ExtractHighlights(tex2D(TextureSampler, i.TexCoord + float2( bo.x, 0)).rgb);
    float3 bL = ExtractHighlights(tex2D(TextureSampler, i.TexCoord + float2(-bo.x, 0)).rgb);
    float3 bU = ExtractHighlights(tex2D(TextureSampler, i.TexCoord + float2(0,  bo.y)).rgb);
    float3 bD = ExtractHighlights(tex2D(TextureSampler, i.TexCoord + float2(0, -bo.y)).rgb);
    
    float3 bloom =
        b0 * 0.40 +
        bR * 0.15 +
        bL * 0.15 +
        bU * 0.15 +
        bD * 0.15;
    
    col += bloom * BloomStrength;

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
    
    // Border-style vignette (edge band + stronger corners, keeps center clean)
    float2 uv = i.TexCoord;
    
    // distance to nearest edge in each axis (0 at edge, 0.5 at center)
    float2 e = min(uv, 1.0 - uv);
    
    // map to 0..1 inside the border band (0 = inside, 1 = at edge)
    float bx = saturate((VignetteWidthX - e.x) / max(VignetteWidthX, 1e-5));
    float by = saturate((VignetteWidthY - e.y) / max(VignetteWidthY, 1e-5));
    
    // sharpen so it stays near 0 until close to edge (approx pow without pow)
    bx = pow(bx, VignetteCurve);
    by = pow(by, VignetteCurve);
    
    // combine: edges darken when either axis is near edge; corners darken most when both are
    float edgeT = 1.0 - (1.0 - bx) * (1.0 - by);
    
    // optional extra corner push (only really affects corners)
    edgeT = saturate(edgeT + CornerBoost * (bx * by));
    
    // apply
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