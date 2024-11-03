using Unity.Collections;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class ForwardLights
    {
        Vector4[] m_AdditionalLightPositions;
        Vector4[] m_AdditionalLightColors;
        Vector4[] m_AdditionalLightAttenuations;
        Vector4[] m_AdditionalLightSpotDirections;
        private int m_maxLights;
        
        static class LightConstantBuffer
        {
            public static int _MainLightPosition; 
            public static int _MainLightColor;
            
            public static int _AdditionalLightsPosition;
            public static int _AdditionalLightsColor;
            public static int _AdditionalLightsCount;
            
            public static int _AdditionalLightsAttenuation;
            public static int _AdditionalLightsSpotDir;
        }
        
        internal ForwardLights()
        {
            LightConstantBuffer._MainLightPosition = Shader.PropertyToID("_MainLightPosition");
            LightConstantBuffer._MainLightColor = Shader.PropertyToID("_MainLightColor");
            
            LightConstantBuffer._AdditionalLightsPosition = Shader.PropertyToID("_AdditionalLightsPosition");
            LightConstantBuffer._AdditionalLightsColor = Shader.PropertyToID("_AdditionalLightsColor");
            LightConstantBuffer._AdditionalLightsCount = Shader.PropertyToID("_AdditionalLightsCount");
            LightConstantBuffer._AdditionalLightsAttenuation = Shader.PropertyToID("_AdditionalLightsAttenuation");
            LightConstantBuffer._AdditionalLightsSpotDir = Shader.PropertyToID("_AdditionalLightsSpotDir");

            m_maxLights = 4;
            m_AdditionalLightPositions = new Vector4[m_maxLights];
            m_AdditionalLightColors = new Vector4[m_maxLights];
            m_AdditionalLightAttenuations = new Vector4[m_maxLights];
            m_AdditionalLightSpotDirections = new Vector4[m_maxLights];
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
                out var lightColor,
                out var lightAttenuation,
                out var lightSpotDir);
            
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
                            out m_AdditionalLightColors[lightIter],
                            out m_AdditionalLightAttenuations[lightIter],
                            out m_AdditionalLightSpotDirections[lightIter]);
                        
                        lightIter++;
                        lightCount = lightIter;
                    }
                    
                    cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsPosition, m_AdditionalLightPositions);
                    cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsColor, m_AdditionalLightColors);
                    cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsAttenuation, m_AdditionalLightAttenuations);
                    cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsSpotDir, m_AdditionalLightSpotDirections);
                }
                cmd.SetGlobalVector(LightConstantBuffer._AdditionalLightsCount, new Vector4(lightCount, 0.0f, 0.0f, 0.0f));
            }
            else
            {
                cmd.SetGlobalVector(LightConstantBuffer._AdditionalLightsCount, Vector4.zero);
            }

        }

        //默认灯光数据
        static Vector4 k_DefaultLightPosition = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
        static Vector4 k_DefaultLightColor = Color.black;
        
        static Vector4 k_DefaultLightAttenuation = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        static Vector4 k_DefaultLightSpotDirection = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
        
        // 根据引索 获取VisibleLight里的单个灯光数据
        void InitializeLightConstants(NativeArray<VisibleLight> lights, int lightIndex, 
            out Vector4 lightPos,
            out Vector4 lightColor, 
            out Vector4 lightAttenuation, 
            out Vector4 lightSpotDir)
        {
            // 初始化默认灯光参数
            lightPos = k_DefaultLightPosition;
            lightColor = k_DefaultLightColor;
            lightAttenuation = k_DefaultLightAttenuation;
            lightSpotDir = k_DefaultLightSpotDirection;
            
            // 当引索小于0时 直接退出（主光源不存在时）
            if(lightIndex < 0)
                return;
            
            // 获取灯光参数
            var visibleLights = lights.UnsafeElementAtMutable(lightIndex);
            Light light = visibleLights.light;
            lightColor = light.color.linear * light.intensity;
            
            // 区分直射光和非直射光（用w分量区分, 0为直射光\1为非直射光）
            if (visibleLights.lightType == LightType.Directional)
            {
                var dir = -light.transform.forward;     // Vector4 dir = -lightLocalToWorld.GetColumn(2); 是一样的
                lightPos = new Vector4(dir.x, dir.y, dir.z, 0.0f);
            }
            else
            {
                // 获取点光源坐标
                var lightLocalToWorld = visibleLights.localToWorldMatrix;
                Vector4 pos = lightLocalToWorld.GetColumn(3);
                lightPos = new Vector4(pos.x, pos.y, pos.z, 1.0f);
                
                // 获取点光源范围的平方 Rnage^2 （用于光线衰减）
                //      为什么不直接传Range？
                //      因为我们把部分hlsl的计算移动到了管线内，节省GPU的计算资源
                //      光线衰减 = max(0, 1- (Distance^2/Rnage^2)^2) 
                lightAttenuation.x = light.range * light.range;
                
                // 聚光灯
                if (visibleLights.lightType == LightType.Spot)
                {
                    // 获取聚光灯朝向
                    //      dot（聚光灯朝向，光线方向）
                    Vector4 dir = lightLocalToWorld.GetColumn(2);
                    lightSpotDir = new Vector4(-dir.x, -dir.y, -dir.z, 0.0f);
                    
                    
                    // 计算灯光衰减
                    float spotAngle = visibleLights.spotAngle;
                    float? innerSpotAngle = light?.innerSpotAngle;
                    float cosOuter = Mathf.Cos(Mathf.Deg2Rad * 0.5f * spotAngle);
                    float cosInner;
                    if (innerSpotAngle.HasValue)
                        cosInner = Mathf.Cos(Mathf.Deg2Rad * 0.5f * innerSpotAngle.Value);
                    else
                        cosInner = 1;

                    float angleScale = 1.0f / Mathf.Max(0.0001f, cosInner - cosOuter);
                    lightAttenuation.z = angleScale;
                    lightAttenuation.w = angleScale * -cosOuter;
                }
            }
        }
        
        internal void Cleanup()
        {
            
        }

    }
}