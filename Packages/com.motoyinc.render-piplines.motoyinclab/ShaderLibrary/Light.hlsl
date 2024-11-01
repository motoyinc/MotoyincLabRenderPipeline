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
    light.color = _AdditionalLightsColor[index].rgb;
    float4 lightPositionWS = _AdditionalLightsPosition[index];
    light.direction = lightPositionWS.xyz - positionWS * lightPositionWS.w;
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
