﻿#if OPENGL
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


static const int BlurRadius = 2;
static const int BlurWidth = BlurRadius * 2 + 1;
static const int BlurArea = BlurWidth * BlurWidth;
static const float GausSpread = 5.0f;
static const float OffsetSpread = 3.0f;
static const float Brighten = 0.5f;

float2 SpriteTextureDimensions;
float4 Color;
float Period;
float Time;


float reduce(float x, int n)
{
    return ceil(x * n) / n;
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

float average(float3 color)
{
    return (color.r + color.g + color.b) / 3;
}

float pulse()
{
    float v0 = wrap(Time, Period);
    float halfPeriod = Period / 2.0f;
    if (v0 >= halfPeriod)
        return (Period - v0) / halfPeriod;
    return v0 / halfPeriod;
}

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 newSpriteColor = float4(0, 0, 0, 0);
    float4 spriteColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);
    if (spriteColor.a != 0)
        return newSpriteColor;
    [unroll(BlurArea)]
    for (int bluri = 0; bluri < BlurArea; bluri++)
    {
        int blurOffsetX = (bluri % BlurWidth) - BlurRadius;
        int blurOffsetY = (bluri / BlurWidth) - BlurRadius;
        
        float2 blurOffset = float2(blurOffsetX, blurOffsetY) * OffsetSpread;
        float2 blurOffsetUV = blurOffset / SpriteTextureDimensions;
        float4 blurColor = tex2D(SpriteTextureSampler, input.TextureCoordinates + blurOffsetUV);
        blurColor = float4(blurColor.a, blurColor.a, blurColor.a, blurColor.a);
        newSpriteColor += blurColor * gaus(length(blurOffset), GausSpread);
    }
    return newSpriteColor * input.Color * Color * Brighten * pulse();
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};