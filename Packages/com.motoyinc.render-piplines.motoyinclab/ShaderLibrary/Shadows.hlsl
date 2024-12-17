#ifndef MLABRP_SHADOWS_INCLUDED
#define MLABRP_SHADOWS_INCLUDED

#include "Common.hlsl"
#include "SurfaceData.hlsl"
#include "Input.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

#define MAX_SHADOW_CASCADES 4

#if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
    #define MAIN_LIGHT_CALCULATE_SHADOWS
#endif

#define SOFT_SHADOW_QUALITY_OFF    half(0.0)
#define SOFT_SHADOW_QUALITY_LOW    half(1.0)
#define SOFT_SHADOW_QUALITY_MEDIUM half(2.0)
#define SOFT_SHADOW_QUALITY_HIGH   half(3.0)

#define BEYOND_SHADOW_FAR(shadowCoord) shadowCoord.z <= 0.0 || shadowCoord.z >= 1.0

/////////////////////////////////////////////////////////////////////////////
///                         主光源数据计算                                  ///
/////////////////////////////////////////////////////////////////////////////
float4 _ShadowBias;
half IsDirectionalLight()
{
    return round(_ShadowBias.z) == 1.0 ? 1 : 0;
}

TEXTURE2D_SHADOW(_MainLightShadowmapTexture);
SAMPLER_CMP(sampler_LinearClampCompare);

CBUFFER_START(LightShadows)
    float4x4    _MainLightWorldToShadow[MAX_SHADOW_CASCADES + 1];
    float4      _MainLightShadowParams;
    float4      _MainLightShadowmapSize;    // (xy: 1/width and 1/height, zw: width and height)
    float4      _CascadeShadowSplitSpheres0;
    float4      _CascadeShadowSplitSpheres1;
    float4      _CascadeShadowSplitSpheres2;
    float4      _CascadeShadowSplitSpheres3;
    float4      _CascadeShadowSplitSphereRadii;
CBUFFER_END


struct ShadowSamplingData
{
    float4 shadowmapSize;   // RT 大小
    half softShadowQuality; // 软阴影质量
};

// 获取主光源阴影 采样数据
ShadowSamplingData GetMainLightShadowSamplingData()
{
    ShadowSamplingData shadowSamplingData;
    shadowSamplingData.shadowmapSize = _MainLightShadowmapSize;
    shadowSamplingData.softShadowQuality = half(_MainLightShadowParams.y);
    return shadowSamplingData;
}

// 获取主光源 灯光数据
// x: 阴影强度
// y: 软阴影（0 关，1、2、3 软阴影质量）
half4 GetMainLightShadowParams()
{
    return half4(_MainLightShadowParams);
}


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
///                         主光源软阴影计算                                ///
/////////////////////////////////////////////////////////////////////////////

real SampleShadowmapFilteredLowQuality(TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), float4 shadowCoord, ShadowSamplingData samplingData)
{
    
    // 计算单个纹素单位
    real2 invAtlas;
    invAtlas.x = samplingData.shadowmapSize.x;      // 1/WidthTextureSize
    invAtlas.y = samplingData.shadowmapSize.y;      // 1/HeightTextureSize
    
    // 以0.5个纹素单位进行偏移
    real invHalfShadowAtlasWidth = 0.5f * invAtlas.x;
    real invHalfShadowAtlasHeight = 0.5f * invAtlas.x;
    
    // 合并偏移量
    real2 shadowOffset0 = real2(- invHalfShadowAtlasWidth, - invHalfShadowAtlasHeight);
    real2 shadowOffset1 = real2(  invHalfShadowAtlasWidth, - invHalfShadowAtlasHeight);
    real2 shadowOffset2 = real2(- invHalfShadowAtlasWidth,   invHalfShadowAtlasHeight);
    real2 shadowOffset3 = real2(  invHalfShadowAtlasWidth,   invHalfShadowAtlasHeight);
    
    // 采样
    real4 attenuation4;
    attenuation4.x = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + float3(shadowOffset0, 0)));
    attenuation4.y = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + float3(shadowOffset1, 0)));
    attenuation4.z = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + float3(shadowOffset2, 0)));
    attenuation4.w = real(SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + float3(shadowOffset3, 0)));

    // 均值PCF
    return dot(attenuation4, real(0.25));
}

real SampleShadowmapFilteredMediumQuality(TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), float4 shadowCoord, ShadowSamplingData samplingData)
{
    real4 textureSize = samplingData.shadowmapSize;
    real fetchesWeights[9];
    real2 fetchesUV[9];
    SampleShadow_ComputeSamples_Tent_5x5(textureSize, shadowCoord.xy, fetchesWeights, fetchesUV);

    float attenuation = fetchesWeights[0] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[0].xy, shadowCoord.z))
                + fetchesWeights[1] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[1].xy, shadowCoord.z))
                + fetchesWeights[2] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[2].xy, shadowCoord.z))
                + fetchesWeights[3] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[3].xy, shadowCoord.z))
                + fetchesWeights[4] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[4].xy, shadowCoord.z))
                + fetchesWeights[5] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[5].xy, shadowCoord.z))
                + fetchesWeights[6] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[6].xy, shadowCoord.z))
                + fetchesWeights[7] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[7].xy, shadowCoord.z))
                + fetchesWeights[8] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[8].xy, shadowCoord.z));
    return  attenuation;
}

real SampleShadowmapFilteredHighQuality(TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), float4 shadowCoord, ShadowSamplingData samplingData)
{
    real fetchesWeights[16];
    real2 fetchesUV[16];
    SampleShadow_ComputeSamples_Tent_7x7(samplingData.shadowmapSize, shadowCoord.xy, fetchesWeights, fetchesUV);

    return fetchesWeights[0] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[0].xy, shadowCoord.z))
                + fetchesWeights[1] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[1].xy, shadowCoord.z))
                + fetchesWeights[2] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[2].xy, shadowCoord.z))
                + fetchesWeights[3] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[3].xy, shadowCoord.z))
                + fetchesWeights[4] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[4].xy, shadowCoord.z))
                + fetchesWeights[5] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[5].xy, shadowCoord.z))
                + fetchesWeights[6] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[6].xy, shadowCoord.z))
                + fetchesWeights[7] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[7].xy, shadowCoord.z))
                + fetchesWeights[8] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[8].xy, shadowCoord.z))
                + fetchesWeights[9] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[9].xy, shadowCoord.z))
                + fetchesWeights[10] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[10].xy, shadowCoord.z))
                + fetchesWeights[11] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[11].xy, shadowCoord.z))
                + fetchesWeights[12] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[12].xy, shadowCoord.z))
                + fetchesWeights[13] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[13].xy, shadowCoord.z))
                + fetchesWeights[14] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[14].xy, shadowCoord.z))
                + fetchesWeights[15] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[15].xy, shadowCoord.z));
}
/////////////////////////////////////////////////////////////////////////////
///                         主光源实时阴影                                  ///
/////////////////////////////////////////////////////////////////////////////

// 采样 ShadowMap
real SampleMainLightShadowmap(float4 shadowCoord, ShadowSamplingData samplingData, half4 shadowParams)
{
    real shadowStrength = shadowParams.x;

    real attenuation = 1.0f;
    if(shadowParams.y == SOFT_SHADOW_QUALITY_OFF)
        attenuation =  SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture, sampler_LinearClampCompare, shadowCoord.xyz);
    if(shadowParams.y == SOFT_SHADOW_QUALITY_LOW)
        attenuation = SampleShadowmapFilteredLowQuality(TEXTURE2D_SHADOW_ARGS(_MainLightShadowmapTexture, sampler_LinearClampCompare), shadowCoord, samplingData);
    if(shadowParams.y == SOFT_SHADOW_QUALITY_MEDIUM)
        attenuation = SampleShadowmapFilteredMediumQuality(TEXTURE2D_SHADOW_ARGS(_MainLightShadowmapTexture, sampler_LinearClampCompare), shadowCoord, samplingData);
    if(shadowParams.y == SOFT_SHADOW_QUALITY_HIGH)
        attenuation =  SampleShadowmapFilteredHighQuality(TEXTURE2D_SHADOW_ARGS(_MainLightShadowmapTexture, sampler_LinearClampCompare), shadowCoord, samplingData);

    attenuation = LerpWhiteTo(attenuation, shadowStrength);
    return BEYOND_SHADOW_FAR(shadowCoord) ? 1.0 : attenuation;
}


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
    // 主光源阴影关闭时，直接返回常数1
    #if !defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        return half(1.0);

    // 采样ShadowMap
    #else
        ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
        half4 shadowParams = GetMainLightShadowParams();
        return SampleMainLightShadowmap(shadowCoord,shadowSamplingData,shadowParams);
    
    #endif
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


half MixRealtimeAndBakedShadows(half realtimeShadow, half bakedShadow, half shadowFade)
{
    float mixShadows = lerp(realtimeShadow, 1, shadowFade);
    return min(bakedShadow, mixShadows);
}


half MainLightShadow(float4 shadowCoord, float3 positionWS)
{
    // 获取实时阴影
    half realtimeShadow = MainLightRealtimeShadow(shadowCoord);

    
    // 计算最大阴影衰减距离
    #ifdef MAIN_LIGHT_CALCULATE_SHADOWS
        half shadowFade = GetMainLightShadowFade(positionWS);
    
    #else
        half shadowFade = half(1.0);
    
    #endif

    
    // 获取烘培阴影
    half bakedShadow = 1.0f;

    // 计算阴影混合
    return MixRealtimeAndBakedShadows(realtimeShadow, bakedShadow, shadowFade);
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

    positionCS.z = lerp(positionCS.z, clamped, IsDirectionalLight());
    return positionCS;
}



float3 ApplyShadowBias(float3 positionWS, float3 normalWS, float3 lightDirection)
{
    float invNdotL = 1.0 - saturate(dot(lightDirection, normalWS));
    float scale = invNdotL * _ShadowBias.y;

    positionWS = lightDirection * _ShadowBias.xxx + positionWS;
    positionWS = normalWS * scale.xxx + positionWS;
    return positionWS ;
}

#endif