#ifndef MLABRP_LAB_LIT_PASS_INCLUDE
#define MLABRP_LAB_LIT_PASS_INCLUDE

#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Common.hlsl"
#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/SurfaceData.hlsl"
#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Input.hlsl"
#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/RealtimeLight.hlsl"
#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Lighting.hlsl"

struct Attributes {
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 baseUV : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct Varyings {
    float4 positionCS : SV_POSITION;
    float4 shadowCoord : VAR_SHADOW_UV;
    float3 positionWS : VAR_POSITION_WS;
    float3 normalWS : VAR_NORMAL;
    float2 baseUV : VAR_BASE_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
    UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
    UNITY_DEFINE_INSTANCED_PROP(float, _Roughness)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

Varyings LitPassVertex (Attributes input){
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output); 
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    output.baseUV = input.baseUV * baseST.xy + baseST.zw;
    output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
    return output;
}

float4 LitPassFragment(Varyings input) : SV_TARGET
{
    SurfaceData surface;
    UNITY_SETUP_INSTANCE_ID(input);
    float baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
    
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    
    // 采集几何信息
    surface.color = baseMap * baseColor;
    surface.alpha = baseColor.a;
    surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Metallic);
    surface.roughness =UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Roughness);

    //环境数据信息收集
    InputData inputData;
    inputData.positionWS = input.positionWS;
    inputData.positionCS = input.positionCS;
    inputData.normalWS = normalize(input.normalWS);
    inputData.viewDirectionWS = normalize(_WorldSpaceCameraPos - input.positionWS);
    inputData.shadowCoord = input.shadowCoord;
    inputData.shadowMask = 0.0f;
    
    //计算光照颜色
    BRDFData brdf = GetBRDF(surface);
    float3 color = GetLighting(inputData, brdf);
    
    #if defined(_CLIPPING)
    clip(baseMap - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
    #endif

    #if defined(_GLOBAL_DEBUG)
    return DebugSurface(surface);
    #endif

    // 输出颜色
    return float4(color,surface.alpha);
}

#endif
