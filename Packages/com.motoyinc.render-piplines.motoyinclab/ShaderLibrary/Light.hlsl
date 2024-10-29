#ifndef MLABRP_LIGHT_INCLUDED
#define MLABRP_LIGHT_INCLUDED

#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Common.hlsl"

CBUFFER_START(_CustomLight)
    float3 _MainLightColor;
    float3 _MainLightPosition;
CBUFFER_END

struct Light {
    float3 color;
    float3 direction;
};

Light GetDirectionalLight () {
    Light light;
    light.color = _MainLightColor;
    light.direction = _MainLightPosition;
    return light;
}

Light _DEUBG_GetDirectionalLight() {
    Light light;
    light.color = 1.0;
    light.direction = normalize(float3(1.0, 1.0, 0.0));
    return light;
}

#endif
