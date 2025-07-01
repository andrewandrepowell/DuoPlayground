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

Texture2D ParallaxTexture;
sampler2D ParallaxTextureSampler = sampler_state
{
    Texture = <ParallaxTexture>;
    minfilter = point; // Use point filtering for minification
    magfilter = point; // Use point filtering for magnification
    mipfilter = point; // Use point filtering for mipmaps
    AddressU = Clamp;
    AddressV = Clamp;
};

float2 SpriteTextureDimensions;
float2 ParallaxTextureDimensions;
float4 ParallaxRegion;
float2 Position = float2(0, 0);

float wrap(float x, float m)
{
    return (m + x % m) % m;
}

float2 wrap(float2 x, float2 m)
{
    return float2(wrap(x.x, m.x), wrap(x.y, m.y));
}

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 parallaxPosition = wrap(input.TextureCoordinates * SpriteTextureDimensions + Position, ParallaxRegion.zw) + ParallaxRegion.xy;
    return tex2D(SpriteTextureSampler, input.TextureCoordinates) * tex2D(ParallaxTextureSampler, parallaxPosition / ParallaxTextureDimensions) * input.Color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};