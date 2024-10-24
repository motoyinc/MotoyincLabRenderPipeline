Shader "MLabRP/Lab/Unlit"
{
    Properties
    {
        _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "Unlit"}
            HLSLPROGRAM
            
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            
            #include "UnlitPass.hlsl"
            
            ENDHLSL
        }
    }
    
}