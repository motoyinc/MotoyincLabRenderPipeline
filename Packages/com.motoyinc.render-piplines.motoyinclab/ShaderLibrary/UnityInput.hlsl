#ifndef MLABRP_UNITY_INPUT_INCLUDED
#define MLABRP_UNITY_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
    float4x4 unity_WorldToObject;
    real4 unity_WorldTransformParams;

    float4x4 unity_MatrixVP;
    float4x4 unity_MatrixV;
    float4x4 unity_MatrixInvV;
    float4x4 unity_prev_MatrixM;
    float4x4 unity_prev_MatrixIM;
    float4x4 glstate_matrix_projection;

    float3 _WorldSpaceCameraPos;

CBUFFER_END


#endif