using UnityEditor;
using UnityEngine.Rendering.RendererUtils;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class SkyboxRenderPass: ScriptableRenderPass
    {
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            var data = renderingData.frameData.Get<MotoyincLabRenderingData>();
            cullingResults = data.cullResults;
            camera = cameraData.camera;
            cmd = renderingData.commandBuffer;
            passName = "SkyboxRenderPass";
            
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd.BeginSample(passName);
            
             if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
             {
                 context.DrawSkybox(camera);
             }

            cmd.EndSample(passName);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
    }
}