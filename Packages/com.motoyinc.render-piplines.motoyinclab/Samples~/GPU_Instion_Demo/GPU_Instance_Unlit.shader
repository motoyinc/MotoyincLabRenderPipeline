Shader "MLabRP/Demo/GPU_Instance_Unlit"
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
            #pragma vertex vert
            #pragma fragment frag

            // GPU实例化宏
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            
            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };
            
            // 不使用属性块
            // float4 _BaseColor;

            // 使用属性块
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                float3 positionWS = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vertex = mul(unity_MatrixVP, float4(positionWS, 1.0));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                // 不使用属性块 获取数据
                // float4 color = _BaseColor;
                
                // 使用属性块 获取数据
                float4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor);
                return color;
            }
            ENDHLSL
        }
    }
}
