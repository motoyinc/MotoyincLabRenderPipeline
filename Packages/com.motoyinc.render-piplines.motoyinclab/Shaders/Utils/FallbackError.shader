Shader "Hidden/MotoyincLabRP/FallbackError"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma target 4.5
            #pragma editor_sync_compilation
            
            #pragma vertex vert
            #pragma fragment frag

            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.positionCS = mul(unity_MatrixVP, worldPos);
                return OUT;
            }

            float4 frag (Varyings IN) : SV_TARGET
            {
                return float4(1,0,1,1);
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Core/FallbackError"
}