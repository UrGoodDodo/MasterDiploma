#ifndef FRACTURE_CONCRETE_INCLUDED
#define FRACTURE_CONCRETE_INCLUDED

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


void ConcretePattern(
    float3 p,
    out float Cement,
    out float Aggregate,
    out float Pores,
    out float HeightSoft,
    out float HeightRough)
{
    Cement = fbm(p * 1.15);

    float aggregateCells = fbm(floor(p * 4.0) * 0.37 + 8.3);
    float aggregateShape = fbm(p * 4.0);
    Aggregate = smoothstep(0.58, 0.82, aggregateShape + aggregateCells * 0.35);

    float poreNoise = fbm(p * 22.0);
    Pores = smoothstep(0.73, 0.92, poreNoise);

    HeightSoft = saturate(Cement * 0.65 + Aggregate * 0.20 - Pores * 0.12);
    HeightRough = saturate(Cement * 0.35 + Aggregate * 0.55 - Pores * 0.35);
}

float ConcreteHeight(float3 p, float rough)
{
    float c, a, pores, hs, hr;
    ConcretePattern(p, c, a, pores, hs, hr);
    return lerp(hs, hr, rough);
}

void ConcreteExterior_float(
    float3 Position,
    float Scale,
    float NormalStrength,
    out float3 BaseColor,
    out float Roughness,
    out float Height,
    out float3 NormalTS)
{
    float3 p = Position * Scale;

    float cement, aggregate, pores, hs, hr;
    ConcretePattern(p, cement, aggregate, pores, hs, hr);

    float3 cementA = float3(0.32, 0.32, 0.31);
    float3 cementB = float3(0.47, 0.46, 0.43);
    float3 subtleStone = float3(0.24, 0.24, 0.23);

    BaseColor = lerp(cementA, cementB, cement);
    BaseColor = lerp(BaseColor, subtleStone, aggregate * 0.25);
    BaseColor = lerp(BaseColor, BaseColor * 0.70, pores * 0.25);
    BaseColor = saturate(BaseColor);

    Roughness = 0.72;
    Height = hs;

    float e = 0.10;
    float h  = ConcreteHeight(p, 0.0);
    float hx = ConcreteHeight(p + float3(e, 0, 0), 0.0);
    float hy = ConcreteHeight(p + float3(0, e, 0), 0.0);
    NormalTS = makeNormalTS(h, hx, hy, NormalStrength);
}

void ConcreteInterior_float(
    float3 Position,
    float Scale,
    float NormalStrength,
    out float3 BaseColor,
    out float Roughness,
    out float Height,
    out float3 NormalTS)
{
    float3 p = Position * Scale;

    float cement, aggregate, pores, hs, hr;
    ConcretePattern(p, cement, aggregate, pores, hs, hr);

    float3 cementA = float3(0.30, 0.30, 0.29);
    float3 cementB = float3(0.55, 0.54, 0.50);
    float3 stoneA = float3(0.18, 0.18, 0.17);
    float3 poreColor = float3(0.045, 0.045, 0.04);

    BaseColor = lerp(cementA, cementB, cement);
    BaseColor = lerp(BaseColor, stoneA, aggregate * 0.85);
    BaseColor = lerp(BaseColor, poreColor, pores * 0.75);
    BaseColor = saturate(BaseColor);

    Roughness = 0.92;
    Height = hr;

    float e = 0.06;
    float h  = ConcreteHeight(p, 1.0);
    float hx = ConcreteHeight(p + float3(e, 0, 0), 1.0);
    float hy = ConcreteHeight(p + float3(0, e, 0), 1.0);
    NormalTS = makeNormalTS(h, hx, hy, NormalStrength);
}

#endif
