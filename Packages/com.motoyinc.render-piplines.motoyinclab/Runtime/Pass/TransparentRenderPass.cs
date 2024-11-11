using UnityEditor;
using UnityEngine.Rendering.RendererUtils;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class TransparentRenderPass: ScriptableRenderPass
    {
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            var data = renderingData.frameData.Get<MotoyincLabRenderingData>();
            cullingResults = data.cullResults;
            camera = cameraData.camera;
            cmd = renderingData.commandBuffer;
            passName = "TransparentRenderPass";
            
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd.BeginSample(passName);
            using var profScope = new ProfilingScope(new ProfilingSampler(passName));
            

            ShaderTagId[] shaderTagId = {
                new ShaderTagId("MotoyincLabRenderPipeline"),
            };

             RendererListDesc rendererListDesc = new RendererListDesc(shaderTagId, cullingResults, camera)
             {
                 sortingCriteria = SortingCriteria.CommonTransparent,
                 rendererConfiguration = PerObjectData.None,
                 renderQueueRange = RenderQueueRange.transparent
             };
             RendererList rendererList = context.CreateRendererList(rendererListDesc);
             cmd.DrawRendererList(rendererList);

            
            cmd.EndSample(passName);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
    }
}