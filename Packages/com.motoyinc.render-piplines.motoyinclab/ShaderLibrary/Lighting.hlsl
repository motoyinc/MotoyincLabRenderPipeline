#ifndef MLABRP_LIGHTING_INCLUDED
#define MLABRP_LIGHTING_INCLUDED

// 使用时请直接注释掉：
//      仅用于编辑时不报错，防止嵌套式依赖关系，使代码变得难以维护
//      但也意味着，使用该库，必须包含以下依赖库
#include "SurfaceData.hlsl"
#include  "Input.hlsl"
#include "Shadows.hlsl"
#include "RealtimeLight.hlsl"
#include "BRDFData.hlsl"

// MainLightShadowData GetMainLightShadowData () {
//     MainLightShadowData data;
//     data.strength = _MainLightShadowData.x;
//     // data.tileIndex = _DirectionalLightShadowData.y;
//     return data;
// }

float3 LightingLambert(InputData inputData, Light light) {
    float3 diffuse = dot(inputData.normalWS, light.direction);
    diffuse = max(0.0, diffuse);  // 0 ~ ∞
    //diffuse = saturate(diffuse);  // 0 ~ 1
    return diffuse * light.color * light.shadowAttenuation;
}

float3 LightingPhysicallyBased(InputData inputData, BRDFData brdf, Light light)
{
    float3 diffuseIntensity = LightingLambert(inputData, light); 
    return diffuseIntensity * DirectBRDF(inputData, brdf, light);
}


///////////////////////////////////////////////////////////////////////////////
//                      Lighting Functions                                   //
//                    照明数据、以及照明数据初始化                                //
///////////////////////////////////////////////////////////////////////////////

// 光照对象
struct LightingData
{
    half3 mainLightColor;
    half3 additionalLightsColor;

};

// 创建并初始化 光照对象
LightingData CreateLightingData(InputData inputData, SurfaceData surfaceData)
{
    LightingData lightingData;
    lightingData.mainLightColor = 0;
    lightingData.additionalLightsColor = 0;

    return lightingData;
}


// 合并计算 光照结果
float4 CalculateFinalColor(LightingData lightingData, half alpha)
{
    half lightingColor = 0;
    lightingColor += lightingData.mainLightColor;
    lightingColor += lightingData.additionalLightsColor;
    return lightingColor;
}


///////////////////////////////////////////////////////////////////////////////
//                      Fragment Functions                                   //
//                    合并渲染片元着色器渲染结果                                  //
///////////////////////////////////////////////////////////////////////////////

// PBR 光照
half4 MotoyincLabFragmentPBR(InputData inputData, SurfaceData surface)
{
    BRDFData brdf = InitializeBRDFData(surface);

    // 初始化灯光对象
    LightingData lightingData = CreateLightingData(inputData, surface);
    Light light;

    
    /// 主光源计算
    light = GetMainLight(inputData);
    lightingData.mainLightColor = LightingPhysicallyBased(inputData, brdf, light);

    
    // 附加光源计算
    for(int i = 0 ; i< GetAdditionalLightCount(); ++i)
    {
        light = GetAdditionalLight(i, inputData);
        lightingData.additionalLightsColor += LightingPhysicallyBased(inputData, brdf, light);
    }

    /// 合并光源结果
    return CalculateFinalColor(lightingData, surface.alpha);
}

#endif
