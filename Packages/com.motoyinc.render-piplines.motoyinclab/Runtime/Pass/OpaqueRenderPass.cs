﻿using UnityEditor;
using UnityEngine.Rendering.RendererUtils;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class OpaqueRenderPass: ScriptableRenderPass
    {
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            var data = renderingData.frameData.Get<MotoyincLabRenderingData>();
            cullingResults = data.cullResults;
            camera = cameraData.camera;
            cmd = renderingData.commandBuffer;
            passName = "OpaqueRenderPass";
            
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
                sortingCriteria = SortingCriteria.CommonOpaque, // 渲染顺序
                rendererConfiguration = PerObjectData.None,     // 附加数据（探针、光照贴图等）
                renderQueueRange = RenderQueueRange.opaque      // 过滤设置
            };
            RendererList rendererList = context.CreateRendererList(rendererListDesc);
            cmd.DrawRendererList(rendererList);
            
            cmd.EndSample(passName);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
#if UNITY_EDITOR 
            DrawUnsupportedShaders(context,cullingResults);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
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
        void DrawUnsupportedShaders(ScriptableRenderContext context, CullingResults cullingResults)
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