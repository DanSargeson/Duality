#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// 1. Inputs from MonoGame
Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

// 2. Custom Parameters we will pass from C#
float Time;
float Intensity;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

// 3. The Actual Pixel Shader
float4 MainPS(VertexShaderOutput input) : COLOR
{
    // UV is the coordinate of the current pixel (X and Y go from 0.0 to 1.0)
    float2 uv = input.TextureCoordinates;

    // --- CHROMATIC ABERRATION ---
    // Shift the red channel slightly left, and the blue channel slightly right.
    // We multiply by Intensity so we can make the glitch worse during Insight!
    float split = 0.005 * Intensity;
    float r = tex2D(SpriteTextureSampler, float2(uv.x - split, uv.y)).r;
    float g = tex2D(SpriteTextureSampler, uv).g;
    float b = tex2D(SpriteTextureSampler, float2(uv.x + split, uv.y)).b;
    float4 color = float4(r, g, b, 1.0);

    // --- SCANLINES ---
    // Create a sine wave across the Y axis. We add Time so the lines scroll downward.
    float scanline = sin((uv.y * 800.0) + (Time * 10.0));
    // Soften the black lines so we can still see the game underneath
    scanline = (scanline * 0.1) + 0.9;
    color.rgb *= scanline;

    // --- VIGNETTE ---
    // Measure distance from the exact center of the screen (0.5, 0.5)
    float dist = distance(uv, float2(0.5, 0.5));
    // Smoothly darken the pixel if its distance is greater than 0.3
    float vignette = smoothstep(0.8, 0.3, dist);
    color.rgb *= vignette;

    // Keep the original alpha
    color.a = tex2D(SpriteTextureSampler, uv).a;

    // Output the final color, multiplied by the SpriteBatch tint (usually White)
    return color * input.Color;
}

// 4. The Compilation Instruction
technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}