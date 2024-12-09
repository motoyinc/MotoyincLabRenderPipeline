﻿#ifndef MLABRP_DEBUG_INCLUDED
#define MLABRP_DEBUG_INCLUDED

#include "Input.hlsl"
#include "SurfaceData.hlsl"
#include "BRDFData.hlsl"
#include "Shadows.hlsl"
#include "RealtimeLight.hlsl"

CBUFFER_START(_debug)
    int _DisplayShadowCascade;
    int _DisplayGBuffer;
CBUFFER_END

float4 DisplayShadowCascade(InputData inputData)
{
    float shadowDebug = ComputeCascadeIndex(inputData.positionWS);
    float4 shadowMask = half4(0.0f,0.0f,0.0f,1.0f);
    if (shadowDebug == 0)
        shadowMask += half4(1.0f,0.0f,0.0f,0.0f);
    if (shadowDebug == 1)
        shadowMask += half4(0.0f,1.0f,0.0f,0.0f);
    if (shadowDebug == 2)
        shadowMask += half4(0.0f,0.0f,8.0f,0.0f);
    if (shadowDebug == 3)
        shadowMask += half4(1.0f,0.0f,5.0f,0.0f);
    return  shadowMask;
}

float4 DisplayGBuffer(SurfaceData surface, InputData inputData, BRDFData brdf)
{
    if(_DisplayGBuffer == 1)
        return float4(surface.color,1);
    if(_DisplayGBuffer == 2)
        return surface.metallic;
    if(_DisplayGBuffer == 3)
        return surface.roughness;
    if(_DisplayGBuffer == 4)
        return  surface.alpha;
    
    if(_DisplayGBuffer == 10)
        return float4(inputData.positionWS,1);
    if(_DisplayGBuffer == 11)
        return float4(inputData.normalWS,1);
    if(_DisplayGBuffer == 12)
        return float4(inputData.viewDirectionWS,1);
    if(_DisplayGBuffer == 13)
        return inputData.shadowCoord;

    Light mainLight = GetMainLight(inputData);
    if(_DisplayGBuffer == 30)
        return float4(mainLight.color,1);
    if(_DisplayGBuffer == 31)
        return mainLight.shadowAttenuation;
    return 0;
}

float4 DebugOutput(float4 output_color, SurfaceData surface, InputData inputData, BRDFData brdf)
{
    if (_DisplayGBuffer != 0)
        output_color = DisplayGBuffer(surface, inputData, brdf);
    else if (_DisplayShadowCascade == 1)
        return DisplayShadowCascade(inputData) * 0.2 + output_color;
    
    return output_color;
}



#endif
