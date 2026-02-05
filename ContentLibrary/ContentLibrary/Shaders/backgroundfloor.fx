Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
    AddressU = Clamp;
    AddressV = Clamp;
};

Texture2D HashTexture;

sampler2D HashSampler = sampler_state
{
    Texture = <HashTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
    AddressU = Wrap;
    AddressV = Wrap;
};

float HashSize = 256;

float Saturation = 0.4;
float Gain = 1;

float2 WorldOrigin;
float2 WorldSize;
float TileSizePx = 128;

float SpeckSizePx = 4;
float SpeckDensity = 0.06;
float SpeckStrength = 0.35;
float3 SpeckColor = float3(0.10, 0.12, 0.08);

float DashChance = 0.70;
float DashMinCells = 2;
float DashMaxCells = 5;

float2 Seed;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float Hash21(float2 p)
{
    p = frac(p * float2(0.1031, 0.1030));
    p += dot(p, p.yx + 33.33);
    return frac((p.x + p.y) * p.x);
}

float3 ApplySaturation(float3 rgb, float sat)
{
    float l = dot(rgb, float3(0.3, 0.59, 0.11));
    return lerp(l.xxx, rgb, sat);
}

// Wrap to keep numbers small (stable far from origin)
float2 WrapCell(float2 c, float span)
{
    return c - span * floor(c / span);
}

float4 Hash4(float2 id)
{
    // id should be integer-ish already (cell/segId). Seed is integer texel offset.
    id = WrapCell(id + Seed, HashSize);
    float2 uv = (id + 0.5) / HashSize; // center of texel
    return tex2D(HashSampler, uv);     // returns 0..1 in rgba
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 world = WorldOrigin + input.TextureCoordinates * WorldSize;

    // If your tile is solid, this is effectively just a solid base anyway.
    float2 tileUv = frac(world / TileSizePx);
    float4 col = tex2D(SpriteTextureSampler, tileUv) * input.Color;

    float density = saturate(SpeckDensity);

    // 4x4 cell coords in world space
    float2 cell = floor(world / SpeckSizePx);
    cell = WrapCell(cell, 16384.0);

    // ---- Dash segments (horizontal bias) ----
    // Choose a "segment bucket" along X so a whole range shares one dash definition.
    // Period scales with DashMax so you can set DashMaxCells=20 without weirdness.
    float period = max(16.0, DashMaxCells * 4.0);        // cells
    float groupX = floor(cell.x / period);

    // One segment per (groupX,row)
    float2 segId = float2(groupX, cell.y);
    segId = WrapCell(segId, 4096.0);

    // Segment decides if it exists and if it's a dash vs "none"
    float4 segR = Hash4(segId);
    
    float isDash = step(segR.r, DashChance);
    float segSpawn = step(1.0 - saturate(density * 0.50), segR.g);
    
    float room = max(1.0, period - DashMaxCells);
    float startOffset = floor(segR.b * room);
    
    float dashLen = floor(lerp(DashMinCells, DashMaxCells + 1.0, segR.a));

    // Cell inside dash?
    float localX = cell.x - groupX * period;           // 0..period
    float dashStart = startOffset;                     // already 0..room
    float dashEnd = dashStart + dashLen;
    float inDash = step(dashStart, localX) * (1.0 - step(dashEnd, localX));

    float dashMask = isDash * segSpawn * inDash;

    // Make dashes “speckly” not solid stripes (comment out if you want solid dashes)
    float4 cellR = Hash4(cell);
    float dashFill  = step(0.20, cellR.g);
    
    // ---- Block specks (non-dash) ----
    float blockDensity = density * (1.0 - DashChance);
    float blockSpawn = step(1.0 - saturate(blockDensity), cellR.r);
    float blockMask = blockSpawn;

    float mask = max(blockMask, dashMask);

    col.rgb = lerp(col.rgb, SpeckColor, mask * SpeckStrength);

    col.rgb = ApplySaturation(col.rgb, Saturation);
    col.rgb = saturate(col.rgb * Gain);
    return col;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_3_0 MainPS();
    }
}
