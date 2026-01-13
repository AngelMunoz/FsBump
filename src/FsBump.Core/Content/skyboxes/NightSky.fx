#if OPENGL
	#define SV_Position POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

// Trivial shader to test cross-platform compilation
matrix World;
matrix View;
matrix Projection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float3 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.TexCoord = input.Position.xyz;
    return output;
}

float hash(float3 p)
{
    p = frac(p * 0.3183099 + 0.1);
    p *= 17.0;
    return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
}

// 3D Value Noise for smooth clouds/craters
float noise(float3 x)
{
    float3 i = floor(x);
    float3 f = frac(x);
    f = f * f * (3.0 - 2.0 * f);

    float a = hash(i + float3(0,0,0));
    float b = hash(i + float3(1,0,0));
    float c = hash(i + float3(0,1,0));
    float d = hash(i + float3(1,1,0));
    float e = hash(i + float3(0,0,1));
    float f_val = hash(i + float3(1,0,1));
    float g = hash(i + float3(0,1,1));
    float h = hash(i + float3(1,1,1));

    return lerp(lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y),
                lerp(lerp(e, f_val, f.x), lerp(g, h, f.x), f.y), f.z);
}

// Fractal Brownian Motion for detailed nebulas
float fbm(float3 x)
{
    float v = 0.0;
    float a = 0.5;
    float3 shift = float3(100, 100, 100);
    for (int i = 0; i < 4; ++i)
    {
        v += a * noise(x);
        x = x * 2.0 + shift;
        a *= 0.5;
    }
    return v;
}

float GetStarLayer(float3 dir, float scale, float threshold)
{
    float3 grid = dir * scale;
    float3 cell = floor(grid);
    float h = hash(cell);
    return smoothstep(threshold, 1.0, h);
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float3 dir = normalize(input.TexCoord);

    // 1. Deep Space Background
    float3 deepSpace = float3(0.0, 0.02, 0.05); // Very dark blue/black
    float3 horizon = float3(0.05, 0.1, 0.25);   // Slightly lighter near horizon
    float horizonFactor = pow(max(0.0, dir.y + 0.2), 3.0);
    float3 sky = lerp(deepSpace, horizon, horizonFactor);

    // 2. Nebulae / Galaxy Clouds
    float cloudNoise = fbm(dir * 2.0);
    float3 nebulaColor1 = float3(0.3, 0.0, 0.4); // Purple
    float3 nebulaColor2 = float3(0.0, 0.3, 0.4); // Teal

    float nebulaMask = smoothstep(0.4, 0.8, cloudNoise);
    // Mix two colors based on higher frequency noise
    float3 nebula = lerp(nebulaColor1, nebulaColor2, noise(dir * 5.0)) * nebulaMask * 0.6;

    sky += nebula;

    // 3. Multi-Layer Stars
    float stars = 0.0;
    stars += GetStarLayer(dir, 100.0, 0.995) * 0.9; // Big, bright, rare
    stars += GetStarLayer(dir, 250.0, 0.985) * 0.6; // Medium
    stars += GetStarLayer(dir, 500.0, 0.96) * 0.3;  // Small, dense dust

    sky += stars;

    // 4. Moon (Analytical Ray-Sphere Intersection)
    // Place moon in the sky
    float3 moonDir = normalize(float3(0.6, 0.5, -0.6));
    float moonRadius = 15.0;
    float moonDist = 200.0;
    float3 sphereCenter = moonDir * moonDist;

    // Ray: P = 0 + t * dir
    // Intersection: |t*dir - C|^2 = R^2
    // t^2 - 2t(dir.C) + |C|^2 - R^2 = 0
    float b = -2.0 * dot(dir, sphereCenter);
    float c = dot(sphereCenter, sphereCenter) - (moonRadius * moonRadius);
    float disc = b * b - 4.0 * c;

    if (disc > 0.0)
    {
        float t = (-b - sqrt(disc)) / 2.0;
        if (t > 0.0)
        {
            float3 hitPos = dir * t;
            float3 normal = normalize(hitPos - sphereCenter);

            // Lighting (Sun from opposite side roughly)
            float3 sunDir = normalize(float3(-0.8, 0.3, 0.5));
            float NdotL = max(0.0, dot(normal, sunDir));

            // Texture/Crater Noise on the moon surface
            float moonSurfaceNoise = fbm(normal * 12.0);
            float3 moonColor = float3(0.8, 0.8, 0.8) * (0.6 + 0.4 * moonSurfaceNoise);

            // Ambient shadow
            float3 ambient = float3(0.02, 0.02, 0.05);

            sky = moonColor * (NdotL + ambient);
        }
    }

    return float4(sky, 1.0);
}

technique ProceduralNightSky
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};