namespace UnityEngine.Rendering.MotoyincLab
{
    public class ForwardLights
    {
        static class LightConstantBuffer
        {
            public static int _MainLightPosition; 
            public static int _MainLightColor;
        }
        internal ForwardLights()
        {
            LightConstantBuffer._MainLightPosition = Shader.PropertyToID("_MainLightPosition");
            LightConstantBuffer._MainLightColor = Shader.PropertyToID("_MainLightColor");
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
            var visibleLights =lightData.visibleLights.UnsafeElementAtMutable(lightData.mainLightIndex);
            Light light = visibleLights.light;
            // Light light = RenderSettings.sun;
            var lightPos = -light.transform.forward;
            var lightColor = light.color.linear  * light.intensity;
            cmd.SetGlobalVector(LightConstantBuffer._MainLightPosition, lightPos);
            cmd.SetGlobalVector(LightConstantBuffer._MainLightColor, lightColor);
        }
        
        internal void Cleanup()
        {
            
        }

    }
}