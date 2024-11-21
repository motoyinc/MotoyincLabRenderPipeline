using UnityEditor;
using UnityEngine.Rendering.RendererUtils;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class TransparentRenderPass: ScriptableRenderPass
    {
        public override bool Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            passName = "TransparentRenderPass";
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