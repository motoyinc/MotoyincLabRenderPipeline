using UnityEditor;
using UnityEngine.Rendering.RendererUtils;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class SimpleRenderPass: ScriptableRenderPass
    {
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            passName = "SimpleRenderPass";
            
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = renderingData.commandBuffer;
            var motoyincLabRenderingData = renderingData.frameData.Get<MotoyincLabRenderingData>();
            var cullingResults = motoyincLabRenderingData.cullResults;
            var cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            var camera = cameraData.camera;
            
            
            cmd.BeginSample(passName);
            
            // --------------------------Begin----------------------------
            // 设置摄像机属性
            //【本段代码已迁移至 Renderer.Execute() 中】
            //
            // context.SetupCameraProperties(camera);
            // context.ExecuteCommandBuffer(cmd);
            // cmd.Clear();
            
            // 设置RT (一般是放在 RenderSingleCamera里做)
            //【本段代码已迁移至 RenderPipeline.RenderSingleCamera() 中】
            //
            // var depthflage = false;
            // var colorflage = false;
            // var colorFlags = Color.clear;
            // if (camera.clearFlags <= CameraClearFlags.Depth)
            //     depthflage = true;
            // if (camera.clearFlags == CameraClearFlags.Color || camera.clearFlags == CameraClearFlags.SolidColor)
            // {
            //     colorflage = true;
            //     colorFlags = camera.backgroundColor.linear;
            // }
            // cmd.ClearRenderTarget(depthflage, colorflage, colorFlags);
            // context.ExecuteCommandBuffer(cmd);
            // cmd.Clear();
            
            
            // ----------Begin：UI编辑器---------
            //【本段代码已迁移至 RenderPipeline.RenderSingleCamera() 中】
            //
// #if UNITY_EDITOR 
//             PrepareForSceneWindow();
// #endif
            // ----------END---------
            
            
            // 摄像机剔除设置
            //【本段代码已迁移至 RenderPipeline.RenderSingleCamera() 中】
            //
            // if (!camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
            // {
            //     Debug.LogWarning($"<b>{camera.name}: <\b>无法从相机获取剪裁数据，<b>{passName}: <\b>当前Pass将终止渲染"  );
            //     return;
            // }
            // var cullingResults = context.Cull(ref cullingParameters);
            
#if UNITY_EDITOR
            var globalDebug = GlobalKeyword.Create("_GLOBAL_DEBUG");
            if (renderingData.globalDebugMode != GlobalDebugMode.Off)
            {
                cmd.SetKeyword(globalDebug,true);
                // for(int i = 0; i < globalDebugKeywordList.Length; i++)
                    // cmd.SetKeyword(globalDebugKeywordList[i],false);
                if(renderingData.globalDebugMode == GlobalDebugMode.Color)
                    cmd.SetGlobalInt(Shader.PropertyToID("_debugFlag"), 1);
                else if(renderingData.globalDebugMode == GlobalDebugMode.Alpha)
                    cmd.SetGlobalInt(Shader.PropertyToID("_debugFlag"), 2);
                else if(renderingData.globalDebugMode == GlobalDebugMode.Normal)
                    cmd.SetGlobalInt(Shader.PropertyToID("_debugFlag"), 3);
                else if(renderingData.globalDebugMode == GlobalDebugMode.NormalNormalizeCheck)
                    cmd.SetGlobalInt(Shader.PropertyToID("_debugFlag"), 4);
                
            }
            else
                cmd.SetKeyword(globalDebug,false);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
#endif
            
            // ----------Begin：不透明---------
            ShaderTagId[] shaderTagId = {
                new ShaderTagId("Unlit"),
                new ShaderTagId("Lit"),
            };
            
            // 创建 RendererListDesc
            RendererListDesc rendererListDesc = new RendererListDesc(shaderTagId, cullingResults, camera)
            {
                sortingCriteria = SortingCriteria.CommonOpaque, // 渲染顺序
                rendererConfiguration = PerObjectData.None,     // 附加数据（探针、光照贴图等）
                renderQueueRange = RenderQueueRange.opaque      // 过滤设置
            };
            RendererList rendererList = context.CreateRendererList(rendererListDesc);
            // 绘制命令
            cmd.DrawRendererList(rendererList);
            // ----------END---------
            
            
            
             // ----------Begin：渲染内置天空球---------
             context.ExecuteCommandBuffer(cmd);
             cmd.Clear();
             if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
             {
                 context.DrawSkybox(camera);
             }
             // ----------END---------
             
             
             
             // ----------Begin：渲染半透明---------
             rendererListDesc = new RendererListDesc(shaderTagId, cullingResults, camera)
             {
                 sortingCriteria = SortingCriteria.CommonTransparent,
                 rendererConfiguration = PerObjectData.None,
                 renderQueueRange = RenderQueueRange.transparent
             };
             rendererList = context.CreateRendererList(rendererListDesc);
             cmd.DrawRendererList(rendererList);
             // ----------END---------
            
            
            
            // ----------Begin：编辑器内渲染---------
#if UNITY_EDITOR 
            DrawUnsupportedShaders(context, cullingResults, cmd, camera);
            
            //【本段代码已迁移至 Renderer.Execute() 中】
            //
            // DrawGizmos(context);
#endif
            // ----------END---------
            
            
            
            // --------------------------END----------------------------
            
            cmd.EndSample(passName);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
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
        
        // 渲染编辑器内UI
        //【本段代码已迁移至 Renderer.Execute() 中】
        //
        // void DrawGizmos (ScriptableRenderContext context) 
        // {
        //     if (Handles.ShouldRenderGizmos()) 
        //     {
        //         context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
        //         context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        //     }
        // }
        
        // 渲染UI编辑器UI
        //【本段代码已迁移至 RenderPipeline.RenderSingleCamera() 中】
        //
        // void PrepareForSceneWindow () 
        // {
        //     if (camera.cameraType == CameraType.SceneView) 
        //     {
        //         ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        //     }
        // }
#endif

    }
}