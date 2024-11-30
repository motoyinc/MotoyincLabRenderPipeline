using UnityEditor;
using UnityEngine.Rendering.RendererUtils;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class SkyboxRenderPass: ScriptableRenderPass
    {
        public override bool Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            passName = "SkyboxRenderPass";
            return true;
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = renderingData.commandBuffer;
            var motoyincLabRenderingData = renderingData.frameData.Get<MotoyincLabRenderingData>();
            var cullingResults = motoyincLabRenderingData.cullResults;
            var cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            var camera = cameraData.camera;
            
            
            cmd.BeginSample(passName);
            
             if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
             {
                 var skyRendererList = context.CreateSkyboxRendererList(cameraData.camera);
                 cmd.DrawRendererList(skyRendererList);
             }

            cmd.EndSample(passName);
        }
    }
}