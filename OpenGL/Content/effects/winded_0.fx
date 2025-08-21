// https://docs.monogame.net/articles/tutorials/building_2d_games/24_shaders/
// https://learn-monogame.github.io/tutorial/first-shader/
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif


Matrix ViewProjection;
Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

static const float Amplitude = 8.0f;
static const float Period = 2.0f;
static const float Pi = 3.14159265359f;
float Time;
float2 SpriteTextureSize;
float2 SpriteTextureRegionSize;


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float wrap(float x, float m)
{
    return (m + x % m) % m;
};

float2 wrap(float2 x, float2 m)
{
    return float2(wrap(x.x, m.x), wrap(x.y, m.y));
};

float2 sway(float2 uv)
{
    // float2 regionPosition = wrap(uv * SpriteTextureSize, SpriteTextureRegionSize);
    float2 regionPosition = uv * SpriteTextureSize;
    float yOffset = Amplitude * cos(2 * Pi * (Time / Period + regionPosition.x / (2.0f * SpriteTextureRegionSize.x))) * (1.0f - regionPosition.y / SpriteTextureRegionSize.y);
    float2 offset = float2(0, yOffset);
    return offset;
}

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position + float4(sway(input.TextureCoordinates), 0, 0), ViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float4 output = tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color;
    return output;
}

technique SpriteDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}