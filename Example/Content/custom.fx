#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

sampler TextureSampler : register(s0);

struct PixelInput {
    float4 Position : SV_Position0;
    float4 Color : COLOR0;
    float4 TexCoord : TEXCOORD0;
};

float4 SpritePixelShader(PixelInput p) : COLOR0 {
    float4 diffuse = tex2D(TextureSampler, p.TexCoord.xy);
    return diffuse * p.Color;
}

technique SpriteBatch {
    pass {
        PixelShader = compile PS_SHADERMODEL SpritePixelShader();
    }
}
