#ifndef MLABRP_GLOBAL_ILLUMINATION_INCLUDED
#define MLABRP_GLOBAL_ILLUMINATION_INCLUDED

////////////////////////////////////////////////////////////////////////////////
/// lightMapUV 顶点输出结构体
////////////////////////////////////////////////////////////////////////////////

// Static LightMap
#if defined(LIGHTMAP_ON)
    #define VERTEX_OUTPUT_LIGHTMAP_UV(lmName, index) float2 lmName : TEXCOORD##index;
#else
    #define VERTEX_OUTPUT_LIGHTMAP_UV(lmName, index)
#endif


// Dynamic LightMap
#if defined(DYNAMICLIGHTMAP_ON)
    #define VERTEX_OUTPUT_DYNAMIC_LIGHTMAP_UV(dynamicLightmapUV, index) float2 dynamicLightmapUV : TEXCOORD##index;
#else
    #define VERTEX_OUTPUT_DYNAMIC_LIGHTMAP_UV(dynamicLightmapUV, index)
#endif




////////////////////////////////////////////////////////////////////////////////
/// lightMapUV 计算宏函数
////////////////////////////////////////////////////////////////////////////////

// Static LightMap
#if defined(LIGHTMAP_ON)
    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT) OUT.xy = lightmapUV.xy * lightmapScaleOffset.xy + lightmapScaleOffset.zw;
#else
    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT)
#endif


// Dynamic LightMap
#if defined(DYNAMICLIGHTMAP_ON)
    #define OUTPUT_DYNAMIC_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT) OUT.xy = lightmapUV.xy * lightmapScaleOffset.xy + lightmapScaleOffset.zw;
#else
    #define OUTPUT_DYNAMIC_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT)
#endif




////////////////////////////////////////////////////////////////////////////////
/// lightMap  贴图采样
////////////////////////////////////////////////////////////////////////////////

// 光照贴图采样库
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"


// 光照贴图绑定方式
#if defined(UNITY_DOTS_INSTANCING_ENABLED) && !defined(USE_LEGACY_LIGHTMAPS)
    // 光照贴图存储方式：使用 Texture Array
    // 所有的光照贴图（Lightmaps）被存储在 纹理数组（Texture Array）中。
    // 优点：更好的批处理（Batching）由于光照贴图被存储在 纹理数组 中，不同光照贴图的对象可以共用同一个渲染批次，避免了 Batching Break（批次断裂）。
    //      - DOTS（Data-Oriented Tech Stack）通常使用 实例化（Instancing），纹理数组能让 多个对象共享相同的 Shader 代码，提高渲染效率。
    #define LIGHTMAP_NAME unity_Lightmaps
    #define LIGHTMAP_INDIRECTION_NAME unity_LightmapsInd
    #define LIGHTMAP_SAMPLER_NAME samplerunity_Lightmaps
    #define LIGHTMAP_SAMPLE_EXTRA_ARGS staticLightmapUV, unity_LightmapIndex.x
#else
    #define LIGHTMAP_NAME unity_Lightmap
    #define LIGHTMAP_INDIRECTION_NAME unity_LightmapInd
    #define LIGHTMAP_SAMPLER_NAME samplerunity_Lightmap
    #define LIGHTMAP_SAMPLE_EXTRA_ARGS staticLightmapUV
#endif


// 采样光照贴图
half3 SampleLightmap(float2 staticLightmapUV, float2 dynamicLightmapUV, half3 normalWS)
{
    half4 transformCoords = half4(1, 1, 0, 0);
    float3 diffuseLighting = 0;

    // 采样Static LightMap 和对应的 DirlightMap
    #if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
        diffuseLighting = SampleDirectionalLightmap(
            TEXTURE2D_LIGHTMAP_ARGS(LIGHTMAP_NAME, LIGHTMAP_SAMPLER_NAME),
            TEXTURE2D_LIGHTMAP_ARGS(LIGHTMAP_INDIRECTION_NAME, LIGHTMAP_SAMPLER_NAME),
            LIGHTMAP_SAMPLE_EXTRA_ARGS, 
            transformCoords, 
            normalWS, 
            true);
    #elif defined(LIGHTMAP_ON)
        diffuseLighting = SampleSingleLightmap(
            TEXTURE2D_LIGHTMAP_ARGS(LIGHTMAP_NAME, LIGHTMAP_SAMPLER_NAME), 
            LIGHTMAP_SAMPLE_EXTRA_ARGS, 
            transformCoords, 
            true);
    #endif

    // 采样Dynamic LightMap 和对应的 DirlightMap
    #if defined(DYNAMICLIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
        diffuseLighting += SampleDirectionalLightmap(
            TEXTURE2D_ARGS(unity_DynamicLightmap, samplerunity_DynamicLightmap),
            TEXTURE2D_ARGS(unity_DynamicDirectionality, samplerunity_DynamicLightmap),
             dynamicLightmapUV, 
             transformCoords, 
             normalWS, 
             false);
    #elif defined(DYNAMICLIGHTMAP_ON)
        diffuseLighting += SampleSingleLightmap(
            TEXTURE2D_ARGS(unity_DynamicLightmap, 
            samplerunity_DynamicLightmap),
            dynamicLightmapUV, 
            transformCoords, 
            false);
    #endif

    return diffuseLighting;
}

// 采样静态光照贴图
half3 SampleLightmap(float2 staticLightmapUV, half3 normalWS)
{
    float2 dummyDynamicLightmapUV = float2(0,0);
    half3 result = SampleLightmap(staticLightmapUV, dummyDynamicLightmapUV, normalWS);
    return result;
}



////////////////////////////////////////////////////////////////////////////////
/// GI 宏函数
////////////////////////////////////////////////////////////////////////////////

// 将 SampleLightMap 通过宏封装成宏函数 SAMPLE_GI() 实现方法多态
#if defined(LIGHTMAP_ON) && defined(DYNAMICLIGHTMAP_ON)
    #define SAMPLE_GI(staticLmName, dynamicLmName, shName, normalWSName) SampleLightmap(staticLmName, dynamicLmName, normalWSName)
#elif defined(DYNAMICLIGHTMAP_ON)
    #define SAMPLE_GI(staticLmName, dynamicLmName, shName, normalWSName) SampleLightmap(0, dynamicLmName, normalWSName)
#elif defined(LIGHTMAP_ON)
    #define SAMPLE_GI(staticLmName, dynamicLmName, shName, normalWSName) SampleLightmap(staticLmName, 0, normalWSName)
#else
    // 在没有开启LightMap时 输出一个环境光（一般是天空球的Color，是个二阶SH，可以用来简单表示天光和地光）
    #define SAMPLE_GI(staticLmName, dynamicLmName, shName, normalWSName) half3(0,0,0)
#endif



////////////////////////////////////////////////////////////////////////////////
/// SH 球谐函数
////////////////////////////////////////////////////////////////////////////////

// 顶点SH颜色输出
#if defined(LIGHTMAP_ON)
    #define VERTEX_OUTPUT_SH_COLOR(shName, index)
#else
    #define VERTEX_OUTPUT_SH_COLOR(shName, index) float3 shName : TEXCOORD##index;
#endif



// 球谐
#if defined(LIGHTMAP_ON)
    #define OUTPUT_SH4(normalWS, OUT)
#else
    #define OUTPUT_SH4(normalWS, OUT) OUT.xyz = SampleProbeSHVertex(normalWS)
#endif

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/AmbientProbe.hlsl"


half3 SampleProbeSHVertex(in float3 normalWS)
{
    return EvaluateAmbientProbeSRGB(normalWS);
}

// 获取光照探针
#if defined(LIGHTMAP_ON)
    #define GET_SH_GI(vertexShName) 0.0f
#else
    #define GET_SH_GI(vertexShName) vertexShName
#endif








#endif
