#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 view_projection;
sampler TextureSampler : register(s0);

struct Vertex {
    float4 Position : SV_Position0;
    float4 Color : COLOR0;
    float4 TexCoord : TEXCOORD0;
};

Vertex SpriteVertexShader(Vertex v) {
    Vertex Output;

    Output.Position = mul(v.Position, view_projection);
    Output.Color = v.Color;
    Output.TexCoord = v.TexCoord;
    return Output;
}
float4 SpritePixelShader(Vertex v): COLOR0 {
    float4 diffuse = tex2D(TextureSampler, v.TexCoord.xy);
    return diffuse * v.Color;
}

technique SpriteBatch {
    pass {
        VertexShader = compile VS_SHADERMODEL SpriteVertexShader();
        PixelShader = compile PS_SHADERMODEL SpritePixelShader();
    }
}
