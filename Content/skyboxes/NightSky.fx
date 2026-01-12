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

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float3 dir = normalize(input.TexCoord);
    
    // Brighter night sky gradient
    float3 topColor = float3(0.05, 0.15, 0.4); 
    float3 bottomColor = float3(0.01, 0.02, 0.05);
    float3 sky = lerp(bottomColor, topColor, smoothstep(-0.2, 0.8, dir.y));

    // Stars
    // Scale direction to grid
    float3 grid = dir * 300.0;
    float3 cell = floor(grid);
    
    // Hash the cell ID
    float h = hash(cell);
    
    // Threshold for star presence
    float star = smoothstep(0.998, 1.0, h);
    
    // Flicker or vary brightness (optional, static for now)
    
    // Fade stars near horizon
    star *= smoothstep(-0.1, 0.2, dir.y);

    return float4(sky + star, 1.0);
}

technique ProceduralNightSky
{
    pass P0
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
};
