using System.Linq;

namespace UnityEngine.Rendering.MotoyincLab
{
    public sealed partial class MotoyincLabRenderer
    {
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 检查PassList，并清空List
            ClearRenderPassList();
            
            var shadowsPass = new MainLightShadowCasterPass();
            renderPassList.Add(shadowsPass);
            var opaqueRenderPass = new OpaqueRenderPass();
            renderPassList.Add(opaqueRenderPass);
            var skyboxRenderPass = new SkyboxRenderPass();
            renderPassList.Add(skyboxRenderPass);
            var transparentRenderPass = new TransparentRenderPass();
            renderPassList.Add(transparentRenderPass);
            
            // var simpleRenderPass = new SimpleRenderPass();
            // renderPassList.Add(simpleRenderPass);
            
            SetupRenderPass(context, ref renderingData);
            
        }

        void SetupRenderPass(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderPassList.Count != 0)
            {
                for (int i = 0; i < renderPassList.Count; ++i)
                {
                    if (renderPassList[i] != null)
                        renderPassList[i].Setup(context, ref renderingData);
                }
            }
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