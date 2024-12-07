#ifndef MLABRP_SHADOWS_INCLUDED
#define MLABRP_SHADOWS_INCLUDED

#include "Common.hlsl"
#include "SurfaceData.hlsl"
#include "Input.hlsl"

#define MAX_SHADOW_CASCADES 4

#if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
    #define MAIN_LIGHT_CALCULATE_SHADOWS
#endif

struct ShadowSamplingData
{
    half4 shadowOffset0;    // 阴影偏移值0
    half4 shadowOffset1;    // 阴影偏移值1
    float4 shadowmapSize;   // RT 大小
    half softShadowQuality; // 软阴影质量
};

/////////////////////////////////////////////////////////////////////////////
///                         主光源数据计算                                  ///
/////////////////////////////////////////////////////////////////////////////


TEXTURE2D_SHADOW(_MainLightShadowmapTexture);
SAMPLER_CMP(sampler_LinearClampCompare);

CBUFFER_START(LightShadows)
    float4x4    _MainLightWorldToShadow[MAX_SHADOW_CASCADES + 1];
    float4      _MainLightShadowParams;
    float4      _CascadeShadowSplitSpheres0;
    float4      _CascadeShadowSplitSpheres1;
    float4      _CascadeShadowSplitSpheres2;
    float4      _CascadeShadowSplitSpheres3;
    float4      _CascadeShadowSplitSphereRadii;
CBUFFER_END


// 根据物体的坐标判断 到相机的距离 输出引索
float4 ComputeCascadeIndex(float3 positionWS)
{
    // 求取剪裁起始点到物体表面的向量
    float3 fromCenter0 = positionWS - _CascadeShadowSplitSpheres0.xyz;
    float3 fromCenter1 = positionWS - _CascadeShadowSplitSpheres1.xyz;
    float3 fromCenter2 = positionWS - _CascadeShadowSplitSpheres2.xyz;
    float3 fromCenter3 = positionWS - _CascadeShadowSplitSpheres3.xyz;

    // 转化为距离值，并把每个联级的距离储存在一个float4的每个分量中
    float4 distances2 = float4(dot(fromCenter0, fromCenter0), dot(fromCenter1, fromCenter1), dot(fromCenter2, fromCenter2), dot(fromCenter3, fromCenter3));

    // 将距离与最大半径进行比较（分量与分量进行比较）
    half4 weights = half4(distances2 < _CascadeShadowSplitSphereRadii);     // 代码等价于：
                                                                            // weights.x = distances2.x < _CascadeShadowSplitSphereRadii.x
                                                                            // weights.y = distances2.y < _CascadeShadowSplitSphereRadii.y
                                                                            // weights.z = distances2.z < _CascadeShadowSplitSphereRadii.z
                                                                            // weights.w = distances2.w < _CascadeShadowSplitSphereRadii.w
    
    // 消除重叠区域（拆开就是联级之间两两相减）
    weights.yzw = saturate(weights.yzw - weights.xyz);                      // 代码等价于：
                                                                            // weight.y = weight.y - weight.x
                                                                            // weight.z = weight.z - weight.y
                                                                            // weight.w = weight.w - weight.z
    // 将结果转化成 0~3之间的引索
    return half(4.0) - dot(weights, half4(4, 3, 2, 1));
}

/////////////////////////////////////////////////////////////////////////////
///                         主光源实时阴影                                  ///
/////////////////////////////////////////////////////////////////////////////


// 获取主光灯光 shadowCoord
float4 TransformWorldToShadowCoord(float3 positionWS)
{
    #ifdef _MAIN_LIGHT_SHADOWS_CASCADE
        half cascadeIndex=ComputeCascadeIndex(positionWS);
    #else
        half cascadeIndex = half(0.0);
    #endif
    
    float4 shadowCoord = mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));
    shadowCoord.xyz /= shadowCoord.w; // 齐次归一化
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

half MainLightShadow(float4 shadowCoord, float3 positionWS)
{
    #if !defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        half realtimeShadow =half(1.0);
    #else
        half realtimeShadow = MainLightRealtimeShadow(shadowCoord);
    #endif
    
    half shadowFade = GetMainLightShadowFade(positionWS);
    half shadowBake = 1.0f;
    return lerp(realtimeShadow, shadowBake, shadowFade);
}


/////////////////////////////////////////////////////////////////////////////
///                             修复阴影BUG                                ///
/////////////////////////////////////////////////////////////////////////////

// 调整近平面剪裁值为最小值
float4 ApplyShadowClamping(float4 positionCS)
{
    #if UNITY_REVERSED_Z
    float clamped = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #else
    float clamped = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #endif

    positionCS.z = clamped;
    return positionCS;
}


float4 _ShadowBias;
half IsDirectionalLight()
{
    return round(_ShadowBias.z) == 1.0 ? 1 : 0;
}
float3 ApplyShadowBias(float3 positionWS, float3 normalWS, float3 lightDirection)
{
    float invNdotL = 1.0 - saturate(dot(lightDirection, normalWS));
    float scale = invNdotL * _ShadowBias.y;

    // normal bias is negative since we want to apply an inset normal offset
    positionWS = lightDirection * _ShadowBias.xxx + positionWS;
    positionWS = normalWS * scale.xxx + positionWS;
    return positionWS;
}


#endif