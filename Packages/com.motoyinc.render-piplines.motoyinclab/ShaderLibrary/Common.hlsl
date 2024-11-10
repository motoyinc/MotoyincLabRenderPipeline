#ifndef MLABRP_COMMON_INCLUDED
#define MLABRP_COMMON_INCLUDED

#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_I_V unity_MatrixInvV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_PREV_MATRIX_M unity_prev_MatrixM
#define UNITY_PREV_MATRIX_I_M unity_prev_MatrixIM
#define UNITY_MATRIX_P glstate_matrix_projection

#include "UnityInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

// 坐标空间转换
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
// float3 TransformObjectToWorld (float3 positionOS) {
//     return mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
// }

// float4 TransformWorldToHClip (float3 positionWS) {
//     return mul(unity_MatrixVP, float4(positionWS, 1.0));
// }

float Square (float v) {
    return v * v;
}


#endif