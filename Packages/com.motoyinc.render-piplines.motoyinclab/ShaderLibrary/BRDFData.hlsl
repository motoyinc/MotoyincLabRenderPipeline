#ifndef MLABRP_BRDF_INCLUDED
#define MLABRP_BRDF_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "UnityInput.hlsl"
#include "SurfaceData.hlsl"
#include "Input.hlsl"

struct BRDFData {
    float3 diffuse;
    float3 specular;
    float roughness;
};

// 该函数的作用是把 metallic原本是0~1 映射到0~0.96
// 并取反（金属度越大值越小）
#define MIN_REFLECTIVITY 0.04
float OneMinusReflectivity (float metallic) {
    float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

BRDFData GetBRDF (SurfaceData surface) {
    BRDFData brdf;
    // float oneMinusReflectivity = 1.0 - surface.metallic;
    float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
    brdf.diffuse = surface.color * oneMinusReflectivity;
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    brdf.roughness = surface.roughness;
    return brdf;
}

float DirectBRDFSpecular (InputData inputData, BRDFData brdf, Light light) {
    float3 h = SafeNormalize(light.direction + inputData.viewDirectionWS);
    float nh2 = Square(saturate(dot(inputData.normalWS, h)));
    float lh2 = Square(saturate(dot(light.direction, h)));
    float r2 = Square(brdf.roughness);
    float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
    float normalization = brdf.roughness * 4.0 + 2.0;
    return r2 / (d2 * max(0.1, lh2) * normalization);
}

float DirectBRDFPhongSpecular (InputData inputData, BRDFData brdf, Light light) {
    float3 r = SafeNormalize(reflect(-light.direction, inputData.normalWS));
    float specular = (1-brdf.roughness) * pow(max(0.0, dot(r, inputData.viewDirectionWS)), (1-brdf.roughness+0.01)*100);
    return specular;
}

float3 DirectBRDF (InputData inputData, BRDFData brdf, Light light) {
    return DirectBRDFSpecular(inputData, brdf, light) * brdf.specular  + brdf.diffuse;
}

#endif