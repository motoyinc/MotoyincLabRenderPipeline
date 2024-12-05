#ifndef MLABRP_SHADOWS_INCLUDED
#define MLABRP_SHADOWS_INCLUDED

#include "Common.hlsl"
#include "SurfaceData.hlsl"
#include "Input.hlsl"

#define MAX_SHADOW_CASCADES 4



/////////////////////////////////////////////////////////////////////////////
///                         主光源数据计算                                  ///
/////////////////////////////////////////////////////////////////////////////


TEXTURE2D_SHADOW(_MainLightShadowmapTexture);
SAMPLER_CMP(sampler_LinearClampCompare);

CBUFFER_START(LightShadows)
    float4x4    _MainLightWorldToShadow[MAX_SHADOW_CASCADES + 1];
    float4      _MainLightShadowParams;
CBUFFER_END



/////////////////////////////////////////////////////////////////////////////
///                         主光源实时阴影                                  ///
/////////////////////////////////////////////////////////////////////////////


// 获取主光灯光 shadowCoord
float4 TransformWorldToShadowCoord(float3 positionWS)
{
    half cascadeIndex = half(0.0);
    float4 shadowCoord = mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));
    
    return shadowCoord;
}

// 获取主光实时阴影
half MainLightRealtimeShadow(float4 shadowCoord)
{
    return SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture, sampler_LinearClampCompare, shadowCoord);
}


/////////////////////////////////////////////////////////////////////////////
///                         主光源阴影输出计算                               ///
/////////////////////////////////////////////////////////////////////////////
// 获取摄像机最远范围
half GetMainLightShadowFade(float3 positionWS)
{
    float3 camToPixel = positionWS - _WorldSpaceCameraPos;
    float distanceCamToPixel2 = dot(camToPixel, camToPixel);

    float fade = saturate(distanceCamToPixel2 * float(_MainLightShadowParams.z) + float(_MainLightShadowParams.w));
    return half(fade);
}

half4 MainLightShadow(float4 shadowCoord, float3 positionWS)
{
    half realtimeShadow = MainLightRealtimeShadow(shadowCoord);
    half shadowFade = GetMainLightShadowFade(positionWS);
    half shadowBake = 1.0f;
    return lerp(realtimeShadow, shadowBake, shadowFade);
}



#endif