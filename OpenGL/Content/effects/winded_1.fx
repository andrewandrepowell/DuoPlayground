// https://docs.monogame.net/articles/tutorials/building_2d_games/24_shaders/
// https://learn-monogame.github.io/tutorial/first-shader/
// https://thebookofshaders.com/10/
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

static const float YAmplitude = 4.0f;
static const float XAmplitude = 6.0f;
static const float Period = 4.0f;
static const float Pi = 3.14159265359f;

float Time;
float2 SpriteTextureSize;
float2 SpriteTextureRegionOffset;
float2 SpriteTextureRegionSize;
float Scale;
float Seed;

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


float rand(float x)
{
    return frac(sin(x) * 1000000.0f);
}

float gaus(float x, float sigma)
{
    return exp(-x * x / (2.0 * sigma * sigma)) / (sqrt(2.0 * 3.14159265359 * sigma * sigma));
};

float wrap(float x, float m)
{
    return (m + x % m) % m;
};

float2 wrap(float2 x, float2 m)
{
    return float2(wrap(x.x, m.x), wrap(x.y, m.y));
};


float2 scale(float2 position, float2 uv, float amount)
{
    float2 regionPosition = (uv * SpriteTextureSize) - SpriteTextureRegionOffset;
    float2 originPosition = (position - regionPosition) + (SpriteTextureRegionSize / 2);
    float2 translatedPosition = position - originPosition;
    float2 scaledPosition = mul(translatedPosition, float2x2(amount, 0.0f, 0.0f, amount));
    float2 untranslatedPosition = scaledPosition + originPosition;
    return untranslatedPosition;
}

float drift(float x)
{
    float i = floor(x) % 6;
    float i_next = (i + 1) % 6;
    float f = floor(x);
    return lerp(rand(i), rand(i_next), smoothstep(0.0f, 1.0f, f));
}

float2 sway(float2 uv, float seed)
{
    
    float2 regionPosition = uv * SpriteTextureSize - SpriteTextureRegionOffset;
    float timeCoef = Time / Period;
    float diminishAmplitudeCoef = (1.0f - regionPosition.y / SpriteTextureRegionSize.y);
    float driftCoef = drift(timeCoef / 2.0f + seed);
    float rightPhaseCoef = regionPosition.x / (2.0f * SpriteTextureRegionSize.x);
    float yOffset = YAmplitude * cos(2 * Pi * (timeCoef + rightPhaseCoef + seed)) * diminishAmplitudeCoef * driftCoef;
    float xOffset = XAmplitude * cos(2 * Pi * (timeCoef + seed)) * diminishAmplitudeCoef * driftCoef;
    float2 offset = float2(xOffset, yOffset);
    return offset;
}

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float2 swayedPosition = input.Position.xy + sway(input.TextureCoordinates, Seed);
    float2 scaledPosition = scale(swayedPosition, input.TextureCoordinates, Scale);
    output.Position = mul(float4(scaledPosition, input.Position.zw), ViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

technique SpriteDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
    }
}