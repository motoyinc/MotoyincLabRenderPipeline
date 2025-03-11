#ifndef MLABRP_LIGHT_INCLUDED
#define MLABRP_LIGHT_INCLUDED

#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Common.hlsl"
#include  "Input.hlsl"
#include "Shadows.hlsl"

#define MAX_ADDITIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
    float4 _MainLightColor;
    float4 _MainLightPosition;
    float4 _AdditionalLightsCount;
    float4 _AdditionalLightsPosition[MAX_ADDITIONAL_LIGHT_COUNT];
    float4 _AdditionalLightsColor[MAX_ADDITIONAL_LIGHT_COUNT];
    float4 _AdditionalLightsAttenuation[MAX_ADDITIONAL_LIGHT_COUNT];
    float4 _AdditionalLightsSpotDir[MAX_ADDITIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light {
    float3 color;
    float3 direction;
    float distance;
    float shadowAttenuation;
};

int GetAdditionalLightCount () {
    return _AdditionalLightsCount.x;
}

Light GetAdditionalLight (int index, InputData inputData) {
    Light light;
    float4 lightPositionWS = _AdditionalLightsPosition[index];
    float3 color = _AdditionalLightsColor[index].rgb;
    float lightType = lightPositionWS.w;
    
    // 计算光的照射方向
    light.direction = normalize(lightPositionWS.xyz - inputData.positionWS * lightType);
    light.distance = length(lightPositionWS.xyz - inputData.positionWS);

    // 计算光线衰减
    float distanceSqr = max(light.distance * light.distance, 0.000001);    // Distance^2
    float distanceAttenuation = saturate(1/(distanceSqr)); 

    // 光线范围计算
    float lightRangeSqr = _AdditionalLightsAttenuation[index].x; // Range^2
    float sqr = pow(distanceSqr/lightRangeSqr, 2);
    float RangeAttenuation = max(0,1-sqr) * max(0,1-sqr);
    float Attenuation = RangeAttenuation * distanceAttenuation;

    // 计算聚光灯衰减
    float3 spotDir = _AdditionalLightsSpotDir[index].xyz;
    float spotIntensity = dot(light.direction, spotDir);
    float spotAttenuation = saturate(spotIntensity * _AdditionalLightsAttenuation[index].z + _AdditionalLightsAttenuation[index].w);

    // 区分直射光和非直射光Color
    light.color = color*((1-lightType) +  Attenuation * spotAttenuation* lightType);
    light.shadowAttenuation = 1;
    return light;
}

Light GetMainLight (InputData inputData) {
    Light light;
    light.color = _MainLightColor.rgb;
    light.direction = normalize(_MainLightPosition.xyz);
    light.distance = 0.0;
    
    #if _RECEIVE_SHADOWS
        light.shadowAttenuation = MainLightShadow(inputData.shadowCoord, inputData.positionWS, inputData.positionSS);
    
    #else
        light.shadowAttenuation = 1;
    
    #endif
    
    return light;
}

Light _DEUBG_GetDirectionalLight() {
    Light light;
    light.color = 1.0;
    light.direction = normalize(float3(1.0, 1.0, 0.0));
    light.distance = 0.0;
    light.shadowAttenuation = 1;
    return light;
}

#endif
