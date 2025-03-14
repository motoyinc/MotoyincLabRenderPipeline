#ifndef MLABRP_LAB_LIT_PASS_INCLUDE
#define MLABRP_LAB_LIT_PASS_INCLUDE

#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Common.hlsl"
#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/SurfaceData.hlsl"
#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Input.hlsl"
#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/RealtimeLight.hlsl"
#include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Lighting.hlsl"

#if _DEBUG_MODE
    #include "Packages/com.motoyinc.render-piplines.motoyinclab/ShaderLibrary/Debug.hlsl" 
#endif


struct Attributes {
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 baseUV : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct Varyings {
    float4 positionCS : SV_POSITION;
    #ifndef _MAIN_LIGHT_SHADOWS_CASCADE
        float4 shadowCoord : VAR_SHADOW_UV;
    #endif
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
    #ifndef _MAIN_LIGHT_SHADOWS_CASCADE
    output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
    #endif
    return output;
}

float4 LitPassFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    
    /// 初始化几何信息
    SurfaceData surface;
    float baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    surface.color = baseMap * baseColor;
    surface.alpha = baseColor.a;
    surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Metallic);
    surface.roughness =UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Roughness);

    
    /// 初始化空间数据
    InputData inputData;
    inputData.positionWS = input.positionWS;
    inputData.positionCS = input.positionCS;
    inputData.positionSS = input.positionCS.xy;
    inputData.normalWS = normalize(input.normalWS);
    inputData.viewDirectionWS = normalize(_WorldSpaceCameraPos - input.positionWS);
    #ifdef _MAIN_LIGHT_SHADOWS_CASCADE
        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
        inputData.shadowCoord = input.shadowCoord;
    #endif
    inputData.shadowMask = 0.0f;
    

    
    /// 计算光照颜色
    float4 color = MotoyincLabFragmentPBR(inputData, surface);
    

    #if defined(_CLIPPING)
    clip(baseMap - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
    #endif

    
    /// Debug设置
    #if defined(_DEBUG_MODE)
        BRDFData brdf = InitializeBRDFData(surface);
        float4 output_color = float4(color.xyz,surface.alpha);
        return DebugOutput(output_color, surface, inputData, brdf);
    #endif
    
    return color;
}

#endif
