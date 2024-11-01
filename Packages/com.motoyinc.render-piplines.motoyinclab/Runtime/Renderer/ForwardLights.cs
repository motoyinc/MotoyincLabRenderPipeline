using Unity.Collections;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class ForwardLights
    {
        Vector4[] m_AdditionalLightPositions;
        Vector4[] m_AdditionalLightColors;
        private int m_maxLights;
        
        static class LightConstantBuffer
        {
            public static int _MainLightPosition; 
            public static int _MainLightColor;
            
            public static int _AdditionalLightsPosition;
            public static int _AdditionalLightsColor;
            public static int _AdditionalLightsCount;
        }
        
        internal ForwardLights()
        {
            LightConstantBuffer._MainLightPosition = Shader.PropertyToID("_MainLightPosition");
            LightConstantBuffer._MainLightColor = Shader.PropertyToID("_MainLightColor");
            
            LightConstantBuffer._AdditionalLightsPosition = Shader.PropertyToID("_AdditionalLightsPosition");
            LightConstantBuffer._AdditionalLightsColor = Shader.PropertyToID("_AdditionalLightsColor");
            LightConstantBuffer._AdditionalLightsCount = Shader.PropertyToID("_AdditionalLightsCount");

            m_maxLights = 4;
            m_AdditionalLightPositions = new Vector4[m_maxLights];
            m_AdditionalLightColors = new Vector4[m_maxLights];
        }
        
        public void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ContextContainer frameData = renderingData.frameData;
            MotoyincLabCameraData cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            MotoyincLabRenderingData motoyincLabRenderingData = renderingData.frameData.Get<MotoyincLabRenderingData>();
            MotoyincLabLightData lightData = renderingData.frameData.Get<MotoyincLabLightData>();
            var cmd = CommandBufferHelpers.GetUnsafeCommandBuffer(renderingData.commandBuffer);
            SetupLights(cmd, motoyincLabRenderingData, cameraData, lightData);
        }

        internal void SetupLights(UnsafeCommandBuffer cmd, MotoyincLabRenderingData renderingData, MotoyincLabCameraData cameraData, MotoyincLabLightData lightData)
        {
            // 向GPU发送主光源灯光
            SetupMainLightConstants(cmd, lightData);
            
            // 向GPU发送附加光源灯光
            SetupAdditionalLightConstants(cmd, ref renderingData.cullResults, lightData);
        }

        // 向GPU发送主光源信息
        void SetupMainLightConstants(UnsafeCommandBuffer cmd, MotoyincLabLightData lightData)
        {
            // 获取灯光信息
            InitializeLightConstants(lightData.visibleLights, lightData.mainLightIndex,
                out var lightPos,
                out var lightColor);
            
            // 向GPU发送灯光信息
            cmd.SetGlobalVector(LightConstantBuffer._MainLightPosition, lightPos);
            cmd.SetGlobalVector(LightConstantBuffer._MainLightColor, lightColor);
        }

        // 向GPU发送附加光源信息
        void SetupAdditionalLightConstants(UnsafeCommandBuffer cmd, ref CullingResults cullResults, MotoyincLabLightData lightData)
        {
            var lights = lightData.visibleLights;
            var lightCount = 0;
            if (lights.Length != 0)
            {
                for (int i = 0,lightIter = 0; i < lights.Length && lightIter < m_maxLights; ++i)
                {
                    
                    if (i != lightData.mainLightIndex)
                    {
                        InitializeLightConstants(lightData.visibleLights, i,
                            out m_AdditionalLightPositions[lightIter],
                            out m_AdditionalLightColors[lightIter]);
                        
                        lightIter++;
                        lightCount = lightIter;
                    }
                    
                    cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsPosition, m_AdditionalLightPositions);
                    cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsColor, m_AdditionalLightColors);
                }
                cmd.SetGlobalVector(LightConstantBuffer._AdditionalLightsCount, new Vector4(lightCount, 0.0f, 0.0f, 0.0f));
            }
            else
            {
                cmd.SetGlobalVector(LightConstantBuffer._AdditionalLightsCount, Vector4.zero);
            }

        }

        // 根据引索 获取VisibleLight里的单个灯光信息
        void InitializeLightConstants(NativeArray<VisibleLight> lights, int lightIndex, 
            out Vector4 lightPos,
            out Vector4 lightColor)
        {
            var visibleLights = lights.UnsafeElementAtMutable(lightIndex);
            Light light = visibleLights.light;
            lightColor = light.color.linear * light.intensity;
            lightPos = -light.transform.forward;
        }
        
        internal void Cleanup()
        {
            
        }

    }
}