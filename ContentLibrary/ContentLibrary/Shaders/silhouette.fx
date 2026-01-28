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

// SpriteBatch will supply the vertex shader if you omit one.
float4 PSMain(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float a = tex2D(TextureSampler, texCoord).a;
    return float4(color.rgb * a, 1);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PSMain();
    }
}