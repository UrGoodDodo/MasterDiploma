#ifndef FRACTURE_INTERIOR_INCLUDED
#define FRACTURE_INTERIOR_INCLUDED

float hash31(float3 p)
{
    p = frac(p * 0.1031);
    p += dot(p, p.yzx + 33.33);
    return frac((p.x + p.y) * p.z);
}

float noise3D(float3 p)
{
    float3 i = floor(p);
    float3 f = frac(p);

    f = f * f * (3.0 - 2.0 * f);

    float n000 = hash31(i + float3(0.0, 0.0, 0.0));
    float n100 = hash31(i + float3(1.0, 0.0, 0.0));
    float n010 = hash31(i + float3(0.0, 1.0, 0.0));
    float n110 = hash31(i + float3(1.0, 1.0, 0.0));
    float n001 = hash31(i + float3(0.0, 0.0, 1.0));
    float n101 = hash31(i + float3(1.0, 0.0, 1.0));
    float n011 = hash31(i + float3(0.0, 1.0, 1.0));
    float n111 = hash31(i + float3(1.0, 1.0, 1.0));

    float nx00 = lerp(n000, n100, f.x);
    float nx10 = lerp(n010, n110, f.x);
    float nx01 = lerp(n001, n101, f.x);
    float nx11 = lerp(n011, n111, f.x);

    float nxy0 = lerp(nx00, nx10, f.y);
    float nxy1 = lerp(nx01, nx11, f.y);

    return lerp(nxy0, nxy1, f.z);
}

float fbm(float3 p)
{
    float value = 0.0;
    float amplitude = 0.5;

    [unroll]
    for (int i = 0; i < 5; i++)
    {
        value += noise3D(p) * amplitude;
        p *= 2.0;
        amplitude *= 0.5;
    }

    return saturate(value);
}


void GranitePattern(float3 p, out float Macro, out float Grain, out float VeinsSoft, out float VeinsSharp, out float HeightSoft, out float HeightRough)
{
    Macro = fbm(p * 0.65);

    Grain = fbm(p * 9.0);

    float veinDistortionA = fbm(p * 1.15);
    float veinDistortionB = fbm((p + float3(13.1, 7.7, 2.9)) * 2.25);

    float veinField = p.x * 0.85 + p.y * 0.28 + p.z * 0.18 + veinDistortionA * 1.35 + veinDistortionB * 0.45;

    float veinWave = abs(sin(veinField * 4.2));

    VeinsSoft = smoothstep(0.70, 1.0, veinWave);

    VeinsSharp = smoothstep(0.88, 1.0, veinWave);

    HeightSoft = saturate(Macro * 0.60 + Grain * 0.20 + VeinsSoft * 0.20);

    HeightRough = saturate(Macro * 0.35 + Grain * 0.35 + VeinsSharp * 0.35);
}

float GraniteHeightSoft(float3 p)
{
    float macro;
    float grain;
    float veinsSoft;
    float veinsSharp;
    float heightSoft;
    float heightRough;

    GranitePattern(p, macro, grain, veinsSoft, veinsSharp, heightSoft, heightRough);
    return heightSoft;
}

float GraniteHeightRough(float3 p)
{
    float macro;
    float grain;
    float veinsSoft;
    float veinsSharp;
    float heightSoft;
    float heightRough;

    GranitePattern(p, macro, grain, veinsSoft, veinsSharp, heightSoft, heightRough);
    return heightRough;
}

float3 MakeTangentNormalFromHeight(float3 p, float normalStrength, float sampleDistance, float useRoughHeight)
{
    float h;
    float hx;
    float hy;

    float3 px = p + float3(sampleDistance, 0.0, 0.0);
    float3 py = p + float3(0.0, sampleDistance, 0.0);

    if (useRoughHeight > 0.5)
    {
        h  = GraniteHeightRough(p);
        hx = GraniteHeightRough(px);
        hy = GraniteHeightRough(py);
    }
    else
    {
        h  = GraniteHeightSoft(p);
        hx = GraniteHeightSoft(px);
        hy = GraniteHeightSoft(py);
    }

    float dx = clamp(h - hx, -0.30, 0.30) * normalStrength;
    float dy = clamp(h - hy, -0.30, 0.30) * normalStrength;

    return normalize(float3(dx, dy, 1.0));
}

void GraniteExteriorUnified_float(float3 Position, float Scale, float NormalStrength, out float3 BaseColor, out float Roughness, out float Height, out float3 NormalTS)
{
    float3 p = Position * Scale;

    float macro;
    float grain;
    float veinsSoft;
    float veinsSharp;
    float heightSoft;
    float heightRough;

    GranitePattern(p, macro, grain, veinsSoft, veinsSharp, heightSoft, heightRough);

    float3 darkStone  = float3(0.17, 0.165, 0.155);
    float3 midStone   = float3(0.36, 0.345, 0.325);
    float3 lightStone = float3(0.58, 0.55, 0.50);
    float3 veinColor  = float3(0.76, 0.73, 0.66);

    float3 stone = lerp(darkStone, midStone, macro);
    stone = lerp(stone, lightStone, grain * 0.22);

    BaseColor = lerp(stone, veinColor, veinsSoft * 0.32);
    BaseColor += (grain - 0.5) * 0.045;
    BaseColor = saturate(BaseColor);

    Roughness = 0.48;
    Height = heightSoft;

    NormalTS = MakeTangentNormalFromHeight(p, NormalStrength, 0.09, 0.0);
}

void GraniteInteriorUnified_float(float3 Position, float Scale, float NormalStrength, out float3 BaseColor, out float Roughness, out float Height, out float3 NormalTS)
{
    float3 p = Position * Scale;

    float macro;
    float grain;
    float veinsSoft;
    float veinsSharp;
    float heightSoft;
    float heightRough;

    GranitePattern(p, macro, grain, veinsSoft, veinsSharp, heightSoft, heightRough);

    float3 darkStone  = float3(0.15, 0.145, 0.135);
    float3 midStone   = float3(0.42, 0.40, 0.37);
    float3 lightStone = float3(0.64, 0.60, 0.54);
    float3 veinColor  = float3(0.88, 0.84, 0.74);

    float3 stone = lerp(darkStone, midStone, macro);
    stone = lerp(stone, lightStone, grain * 0.35);

    BaseColor = lerp(stone, veinColor, veinsSharp * 0.90);
    BaseColor += (grain - 0.5) * 0.12;
    BaseColor = saturate(BaseColor);

    Roughness = 0.86;
    Height = heightRough;

    NormalTS = MakeTangentNormalFromHeight(p, NormalStrength, 0.055, 1.0);
}

#endif
