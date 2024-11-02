#ifndef MLABRP_LIGHT_INCLUDED
#define MLABRP_LIGHT_INCLUDED

#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Common.hlsl"
#define MAX_ADDITIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
    float4 _MainLightColor;
    float4 _MainLightPosition;
    float4 _AdditionalLightsCount;
    float4 _AdditionalLightsPosition[MAX_ADDITIONAL_LIGHT_COUNT];
    float4 _AdditionalLightsColor[MAX_ADDITIONAL_LIGHT_COUNT];
    float4 _AdditionalLightsAttenuation[MAX_ADDITIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light {
    float3 color;
    float3 direction;
};

int GetAdditionalLightCount () {
    return _AdditionalLightsCount.x;
}

Light GetAdditionalLight (int index, float3 positionWS) {
    Light light;
    
    // 计算光的照射方向
    float4 lightPositionWS = _AdditionalLightsPosition[index];
    float lightType = lightPositionWS.w;
    light.direction = lightPositionWS.xyz - positionWS * lightType;
    float3 dirColor = _AdditionalLightsColor[index].rgb;

    // 计算光线衰减
    float distance = length(light.direction);
    float distanceSqr = distance * distance;    // Distance^2
    float3 unDirColor = _AdditionalLightsColor[index].rgb*(1/(distanceSqr)); 

    // 光线范围计算
    float lightRangeSqr = _AdditionalLightsAttenuation[index].x; // Range^2
    float attenuation = max(0,1-(distanceSqr/lightRangeSqr)) * max(0,1-(distanceSqr/lightRangeSqr));
    unDirColor =unDirColor * attenuation;

    // 区分直射光和非直射光Color
    light.color = dirColor * (1-lightType) + unDirColor * lightType;
    return light;
}

Light GetDirectionalLight () {
    Light light;
    light.color = _MainLightColor.rgb;
    light.direction = _MainLightPosition.xyz;
    return light;
}

Light _DEUBG_GetDirectionalLight() {
    Light light;
    light.color = 1.0;
    light.direction = normalize(float3(1.0, 1.0, 0.0));
    return light;
}

#endif
