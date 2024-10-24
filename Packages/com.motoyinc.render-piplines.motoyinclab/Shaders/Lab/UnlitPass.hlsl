#ifndef MLABRP_LAB_UNLIT_PASS_INCLUDE
#define MLABRP_LAB_UNLIT_PASS_INCLUDE

#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Common.hlsl"

float4 UnlitPassVertex (float3 positionOS : POSITION) : SV_POSITION {
    float3 positionWS = TransformObjectToWorld(positionOS.xyz);
    return TransformWorldToHClip(positionWS);
}

float4 _BaseColor;
float4 UnlitPassFragment() : SV_TARGET
{
    return _BaseColor;
}

#endif
