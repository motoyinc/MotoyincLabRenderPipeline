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

    float4 unity_LightmapST;
    float4 unity_DynamicLightmapST;

    // SH block feature
    real4 unity_SHAr;
    real4 unity_SHAg;
    real4 unity_SHAb;
    real4 unity_SHBr;
    real4 unity_SHBg;
    real4 unity_SHBb;
    real4 unity_SHC;

    // LPPV
    float4 unity_ProbeVolumeParams;
    float4x4 unity_ProbeVolumeWorldToObject;
    float4 unity_ProbeVolumeSizeInv;
    float4 unity_ProbeVolumeMin;

CBUFFER_END


// Unity specific
TEXTURECUBE(unity_SpecCube0);
SAMPLER(samplerunity_SpecCube0);
TEXTURECUBE(unity_SpecCube1);
SAMPLER(samplerunity_SpecCube1);

// Main lightmap
TEXTURE2D(unity_Lightmap);
SAMPLER(samplerunity_Lightmap);
TEXTURE2D_ARRAY(unity_Lightmaps);
SAMPLER(samplerunity_Lightmaps);

// Dynamic lightmap
TEXTURE2D(unity_DynamicLightmap);
SAMPLER(samplerunity_DynamicLightmap);
// TODO ENLIGHTEN: Instanced GI

// Dual or directional lightmap (always used with unity_Lightmap, so can share sampler)
TEXTURE2D(unity_LightmapInd);
TEXTURE2D_ARRAY(unity_LightmapsInd);
TEXTURE2D(unity_DynamicDirectionality);
// TODO ENLIGHTEN: Instanced GI
// TEXTURE2D_ARRAY(unity_DynamicDirectionality);

// LPPV
TEXTURE3D_FLOAT(unity_ProbeVolumeSH);
SAMPLER(samplerunity_ProbeVolumeSH);

#endif