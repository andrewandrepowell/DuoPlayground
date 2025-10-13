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

static const float GlowPeriod = 1.0f;
// static const float4 GlowColor = float4(243.0f, 233.0f, 220.0f, 255.0f) / 255.0f;
// static const float4 GlowColor = float4(26.0f, 58.0f, 58.0f, 255.0f) / 255.0f;
// static const float4 GlowColor = float4(0.0f, 0.0f, 0.0f, 255.0f) / 255.0f;
static const int GlowRadius = 2;
static const int GlowWidth = GlowRadius * 2 + 1;
static const int GlowArea = GlowWidth * GlowWidth;
static const float GlowGausSpread = 7.0f;
static const float GlowOffsetSpread = 3.0f;
static const float GlowBrighten = 1.0f;

float Time;
float2 SpriteTextureSize;
float GlowIntensitiy;
float4 GlowColor;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

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

float pulse()
{
    float wholeCoef = frac(Time / GlowPeriod);
    float halfCoef = frac(2.0 * wholeCoef);
    if (wholeCoef >= 0.5f)
        return lerp(1.0f, 0.0f, halfCoef);
    return lerp(0.0f, 1.0f, halfCoef);
}

float glow(float2 uv)
{
    float glowCoef = 0.0f;
    float4 pixelColor = tex2D(SpriteTextureSampler, uv);
    if (pixelColor.a != 0)
        return glowCoef;
    [unroll(GlowArea)]
    for (int bluri = 0; bluri < GlowArea; bluri++)
    {
        int blurOffsetX = (bluri % GlowWidth) - GlowRadius;
        int blurOffsetY = (bluri / GlowWidth) - GlowRadius;
        
        float2 blurOffset = float2(blurOffsetX, blurOffsetY) * GlowOffsetSpread;
        float2 blurOffsetUV = blurOffset / SpriteTextureSize;
        float4 blurColor = tex2D(SpriteTextureSampler, uv + blurOffsetUV);
        glowCoef += blurColor.a * gaus(length(blurOffset), GlowGausSpread);
    }
    return glowCoef;
}


float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float4 pixelColor = tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color;
    float4 glowPixel = glow(input.TextureCoordinates) * pulse() * GlowColor * GlowIntensitiy;
    float4 output = (pixelColor + glowPixel) * input.Color;
    return output;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}