#ifndef FRACTURE_CERAMIC_INCLUDED
#define FRACTURE_CERAMIC_INCLUDED

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

    float n000 = hash31(i + float3(0, 0, 0));
    float n100 = hash31(i + float3(1, 0, 0));
    float n010 = hash31(i + float3(0, 1, 0));
    float n110 = hash31(i + float3(1, 1, 0));
    float n001 = hash31(i + float3(0, 0, 1));
    float n101 = hash31(i + float3(1, 0, 1));
    float n011 = hash31(i + float3(0, 1, 1));
    float n111 = hash31(i + float3(1, 1, 1));

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

float3 makeNormalTS(float h, float hx, float hy, float strength)
{
    float dx = clamp(h - hx, -0.35, 0.35) * strength;
    float dy = clamp(h - hy, -0.35, 0.35) * strength;
    return normalize(float3(dx, dy, 1.0));
}


void CeramicPattern(
    float3 p,
    out float BodyNoise,
    out float Speckles,
    out float Pores,
    out float HeightSoft,
    out float HeightRough)
{
    BodyNoise = fbm(p * 1.8);
    Speckles = fbm(p * 28.0);
    Pores = smoothstep(0.76, 0.93, Speckles);

    HeightSoft = saturate(BodyNoise * 0.75 + Speckles * 0.08);
    HeightRough = saturate(BodyNoise * 0.35 + Speckles * 0.25 - Pores * 0.28);
}

float CeramicHeight(float3 p, float rough)
{
    float b, s, pores, hs, hr;
    CeramicPattern(p, b, s, pores, hs, hr);
    return lerp(hs, hr, rough);
}

void CeramicExterior_float(
    float3 Position,
    float Scale,
    float NormalStrength,
    out float3 BaseColor,
    out float Roughness,
    out float Height,
    out float3 NormalTS)
{
    float3 p = Position * Scale;

    float body, speckles, pores, hs, hr;
    CeramicPattern(p, body, speckles, pores, hs, hr);

    float3 glazeA = float3(0.86, 0.82, 0.74);
    float3 glazeB = float3(0.98, 0.94, 0.86);

    BaseColor = lerp(glazeA, glazeB, body);
    BaseColor += (speckles - 0.5) * 0.025;
    BaseColor = saturate(BaseColor);

    Roughness = 0.34;
    Height = hs;

    float e = 0.12;
    float h  = CeramicHeight(p, 0.0);
    float hx = CeramicHeight(p + float3(e, 0, 0), 0.0);
    float hy = CeramicHeight(p + float3(0, e, 0), 0.0);
    NormalTS = makeNormalTS(h, hx, hy, NormalStrength);
}

void CeramicInterior_float(
    float3 Position,
    float Scale,
    float NormalStrength,
    out float3 BaseColor,
    out float Roughness,
    out float Height,
    out float3 NormalTS)
{
    float3 p = Position * Scale;

    float body, speckles, pores, hs, hr;
    CeramicPattern(p, body, speckles, pores, hs, hr);

    float3 clayA = float3(0.62, 0.54, 0.45);
    float3 clayB = float3(0.86, 0.80, 0.70);
    float3 poreColor = float3(0.28, 0.23, 0.19);

    BaseColor = lerp(clayA, clayB, body);
    BaseColor = lerp(BaseColor, poreColor, pores * 0.55);
    BaseColor += (speckles - 0.5) * 0.07;
    BaseColor = saturate(BaseColor);

    Roughness = 0.88;
    Height = hr;

    float e = 0.055;
    float h  = CeramicHeight(p, 1.0);
    float hx = CeramicHeight(p + float3(e, 0, 0), 1.0);
    float hy = CeramicHeight(p + float3(0, e, 0), 1.0);
    NormalTS = makeNormalTS(h, hx, hy, NormalStrength);
}

#endif
