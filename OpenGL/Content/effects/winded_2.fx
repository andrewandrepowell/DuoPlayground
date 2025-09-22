#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

static const float Pi = 3.14159265359f;
static const float SwayPeriod = 4.0f;
static const float SwayXAmplitude = 18.0f;

float Time;
Matrix ViewProjection;
float2 SpriteTextureSize;
float2 SpriteTextureRegionOffset;
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

float2 sway(float2 uv, float seed)
{
    float2 regionPosition = uv * SpriteTextureSize - SpriteTextureRegionOffset;
    float timeCoef = Time / SwayPeriod;
    float diminishAmplitudeCoef = (1.0f - regionPosition.y / SpriteTextureRegionSize.y);
    float xOffset = SwayXAmplitude * cos(2 * Pi * (timeCoef + seed)) * diminishAmplitudeCoef;
    float2 offset = float2(xOffset, 0);
    return offset;
}

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    float2 swayedPosition = input.Position.xy + sway(input.TextureCoordinates, input.Position.x + input.Position.y);
    output.Position = mul(float4(swayedPosition, input.Position.zw), ViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color;
}

technique SpriteDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};