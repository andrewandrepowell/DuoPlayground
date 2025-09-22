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
static const float SwayPeriod = 5.0f;
static const float SwayRadAmplitude = Pi / 64.0f;

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

float2x2 rotate(float rads)
{
    return float2x2(cos(rads), -sin(rads), sin(rads), cos(rads));
}

float2 sway(float2 position, float2 uv, float seed)
{
    float2 regionPosition = uv * SpriteTextureSize - SpriteTextureRegionOffset;
    float2 originPosition = (position - regionPosition) + SpriteTextureRegionSize / 2.0f;
    float timeCoef = Time / SwayPeriod;
    float diminishAmplitudeCoef = (1.0f - regionPosition.y / SpriteTextureRegionSize.y);
    float val0Coef = (cos(2 * Pi * timeCoef) + 1.0f) / 2.0f;
    float radCoef = lerp(-SwayRadAmplitude, SwayRadAmplitude, val0Coef) * diminishAmplitudeCoef;
    float2x2 rotateMatrix = rotate(radCoef);
    float2 rotatedPosition = mul(position - originPosition, rotateMatrix) + originPosition;
    return rotatedPosition;
}

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    float2 swayedPosition = sway(input.Position.xy, input.TextureCoordinates, input.TextureCoordinates.x + input.TextureCoordinates.y);
    output.Position = mul(float4(swayedPosition, input.Position.zw), ViewProjection);
    //output.Position = mul(input.Position, ViewProjection);
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