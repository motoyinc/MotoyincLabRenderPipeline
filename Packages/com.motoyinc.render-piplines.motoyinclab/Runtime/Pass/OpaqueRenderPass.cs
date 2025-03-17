using UnityEditor;
using UnityEngine.Rendering.RendererUtils;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class OpaqueRenderPass: ScriptableRenderPass
    {
        public override bool Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            passName = "OpaqueRenderPass";
            return true;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
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
                sortingCriteria = SortingCriteria.CommonOpaque, // 渲染顺序
                rendererConfiguration = motoyincLabRenderingData.perObjectData,     // 附加数据（探针、光照贴图等）
                renderQueueRange = RenderQueueRange.opaque      // 过滤设置
            };
            RendererList rendererList = context.CreateRendererList(rendererListDesc);
            cmd.DrawRendererList(rendererList);
            
            cmd.EndSample(passName);
            
#if UNITY_EDITOR 
            DrawUnsupportedShaders(context, cullingResults, cmd, camera);
#endif
        }
        
#if UNITY_EDITOR     
        static Material errorMaterial = null;
        static ShaderTagId[] legacyShaderTagIds = {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM")
        };
        
        // 渲染不受支持材质球
        void DrawUnsupportedShaders(ScriptableRenderContext context, CullingResults cullingResults, CommandBuffer cmd, Camera camera)
        {
            // 渲染顺序
            var sortingSettings = new SortingSettings(camera);
    
            // 创建错误材质球
            var fallbackErrorShader = GraphicsSettings.GetRenderPipelineSettings<MotoyincLabRenderPipelineRuntimeShaders>().fallbackErrorShader;
            if (errorMaterial == null) 
                errorMaterial = new Material(fallbackErrorShader);

            // 创建 RendererListDesc 描述
            var rendererListDesc = new RendererListDesc(legacyShaderTagIds, cullingResults, camera)
            {
                sortingCriteria = sortingSettings.criteria,
                rendererConfiguration = PerObjectData.None,
                renderQueueRange = RenderQueueRange.all,
                overrideMaterial = errorMaterial
            };
            var rendererList = context.CreateRendererList(rendererListDesc);
            if (rendererList.isValid)
            {
                cmd.DrawRendererList(rendererList);
            }
        }
        
#endif

    }
}