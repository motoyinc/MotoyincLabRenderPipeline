using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.MotoyincLab
{
    public sealed partial class MotoyincLabRenderer
    {
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 检查PassList，并清空List
            ClearRenderPassList();
            var cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            var camera = cameraData.camera;
            
            // 配置默认RT
            ConfigureCameraTarget(k_CameraTarget, k_CameraTarget);
            
            
            // 不透明Pass
            var isOpaqueRenderPass = m_opaqueRenderPass.Setup(context,ref renderingData);
            if (isOpaqueRenderPass)
                renderPassList.Add(m_opaqueRenderPass);
            
            // 天空球Pass
            var isSkyboxRenderPass = m_skyboxRenderPass.Setup(context,ref renderingData);
            if (isSkyboxRenderPass)
                renderPassList.Add(m_skyboxRenderPass);
            
            // 半透明Pass
            var isTransparentRenderPass = m_transparentRenderPass.Setup(context,ref renderingData);
            if (isTransparentRenderPass)
                renderPassList.Add(m_transparentRenderPass);
            
        }


        public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferHelpers.GetUnsafeCommandBuffer(renderingData.commandBuffer);
            MotoyincLabCameraData cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            MotoyincLabRenderingData motoyincLabRenderingData = renderingData.frameData.Get<MotoyincLabRenderingData>();
            MotoyincLabLightData lightData = renderingData.frameData.Get<MotoyincLabLightData>();
            
            m_ForwardLights.SetupLights(cmd, motoyincLabRenderingData, cameraData, lightData);
        }

        public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData) 
        {
            cullingParameters.shadowDistance = cameraData.maxShadowDistance; 
        }
    }
    
}