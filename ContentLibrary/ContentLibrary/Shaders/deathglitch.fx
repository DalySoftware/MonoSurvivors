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

// --- Params you set from C# ---
float Time;        // seconds
float Progress;    // 0..1 (0=start, 1=end)
float Strength;    // 0..1
float Seed;        // any float (unique-ish per death)
float2 SourceTexel; // 1/textureWidth, 1/textureHeight (optional, but useful)

float TearPx = 10.0;        // max horizontal tear in pixels
float TearBandPx = 6.0;     // band height in pixels (in sprite pixel space-ish)
float ChromaPx = 1.25;      // rgb split in pixels
float NoiseScale = 90.0;    // noise frequency

struct VSIn
{
    float4 Position : POSITION0;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOut
{
    float4 Position : POSITION0;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VSOut VS(VSIn i)
{
    VSOut o;
    o.Position = mul(i.Position, MatrixTransform);
    o.Color = i.Color;
    o.TexCoord = i.TexCoord;
    return o;
}

float Hash11(float p)
{
    // cheap hash (ps_3_0 friendly)
    p = frac(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return frac(p);
}

float Hash21(float2 p)
{
    float n = dot(p, float2(12.9898, 78.233));
    return frac(sin(n) * 43758.5453);
}

float4 PS(VSOut i) : COLOR0
{
    // Ease: strong at start, fades quickly.
    float t = saturate(Progress);
    float fade = 1.0 - t;
    fade = fade * fade; // ease-out

    float2 uv = i.TexCoord;

    // --- Horizontal tearing in a few bands (based on UV.y) ---
    // "Band index" changes with time so it jitters.
    float band = floor((uv.y * NoiseScale) / max(1.0, TearBandPx));
    float ti = floor(Time * 60.0);
    float n = Hash11(band + ti * 13.37 + Seed * 31.7);

    // Gate: only some bands tear.
    float bandOn = step(1.0 - 0.35 * Strength, n);

    float tearDir = n * 2.0 - 1.0;
    float tear = tearDir * TearPx * Strength * fade * bandOn;

    uv.x += tear * SourceTexel.x;

    // --- RGB split (direction follows the tear) ---
    float2 co = SourceTexel * (ChromaPx * Strength * fade) * float2(sign(tearDir), 0.0);

    float3 cG = tex2D(TextureSampler, saturate(uv)).rgb;
    float r   = tex2D(TextureSampler, saturate(uv + co)).r;
    float b   = tex2D(TextureSampler, saturate(uv - co)).b;

    float3 col = float3(r, cG.g, b);

    // --- Noisy dissolve (kills the sprite into static) ---
    float nn = Hash21(uv * NoiseScale + float2(Seed, Time * 2.0));
    // Threshold rises with progress: more pixels drop out near the end.
    float cut = step(nn, 1.0 - t);

    // Add a bit of “static brighten” on surviving pixels early.
    col += (nn - 0.5) * 0.20 * Strength * fade;

    float alpha = tex2D(TextureSampler, saturate(uv)).a;
    alpha *= cut;

    // Quick “pop” at the start
    col *= 1.0 + 0.35 * Strength * fade;

    return float4(saturate(col), alpha) * i.Color;
}

technique T
{
    pass P
    {
        PixelShader  = compile ps_3_0 PS();
    }
}