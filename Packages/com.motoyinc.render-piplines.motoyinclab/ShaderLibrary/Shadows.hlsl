#ifndef MLABRP_SHADOWS_INCLUDED
#define MLABRP_SHADOWS_INCLUDED

#include "Common.hlsl"
#include "SurfaceData.hlsl"

#define MAX_SHADOW_CASCADES 4


TEXTURE2D_SHADOW(_MainLightShadowmapTexture);
SAMPLER_CMP(sampler_LinearClampCompare);

CBUFFER_START(LightShadows)
    float4x4    _MainLightWorldToShadow[MAX_SHADOW_CASCADES + 1];
CBUFFER_END


float4 TransformWorldToShadowCoord(float3 positionWS)
{
    half cascadeIndex = half(0.0);
    float4 shadowCoord = mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));
    
    return shadowCoord;
}

half MainLightRealtimeShadow(float4 shadowCoord)
{
    return SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture, sampler_LinearClampCompare, shadowCoord);
}


#endif