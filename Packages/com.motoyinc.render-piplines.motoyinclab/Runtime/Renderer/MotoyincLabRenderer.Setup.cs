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
            var opaqueRenderPass = new OpaqueRenderPass();
            var isOpaqueRenderPass = opaqueRenderPass.Setup(context,ref renderingData);
            if (isOpaqueRenderPass)
                renderPassList.Add(opaqueRenderPass);
            
            // 天空球Pass
            var skyboxRenderPass = new SkyboxRenderPass();
            var isSkyboxRenderPass = skyboxRenderPass.Setup(context,ref renderingData);
            if (isSkyboxRenderPass)
                renderPassList.Add(skyboxRenderPass);
            
            // 半透明Pass
            var transparentRenderPass = new TransparentRenderPass();
            var isTransparentRenderPass = transparentRenderPass.Setup(context,ref renderingData);
            if (isTransparentRenderPass)
                renderPassList.Add(transparentRenderPass);
            
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