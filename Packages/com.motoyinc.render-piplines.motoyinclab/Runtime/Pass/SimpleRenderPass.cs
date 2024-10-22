using UnityEditor;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class SimpleRenderPass: ScriptableRenderPass
    {
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            camera = cameraData.camera;
            cmd = renderingData.commandBuffer;
            passName = "SimpleRenderPass";
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd.BeginSample(passName);
            
            // --------------------------Begin----------------------------
            // 设置RT (一般是放在 RenderSingleCamera里做)
            context.SetupCameraProperties(camera);
            var depthflage = false;
            var colorflage = false;
            var colorFlags = Color.clear;
            if (camera.clearFlags <= CameraClearFlags.Depth)
                depthflage = true;
            if (camera.clearFlags == CameraClearFlags.Color || camera.clearFlags == CameraClearFlags.SolidColor)
            {
                colorflage = true;
                colorFlags = camera.backgroundColor.linear;
            }
            cmd.ClearRenderTarget(depthflage, colorflage, colorFlags);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            
            // ----------Begin：UI编辑器---------
#if UNITY_EDITOR 
            PrepareForSceneWindow();
#endif
            // ----------END---------
            
            
            // 设置Cull() (根据需求，每个Pass自己做处理)
            if (!camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
            {
                Debug.LogWarning($"<b>{camera.name}: <\b>无法从相机获取剪裁数据，<b>{passName}: <\b>当前Pass将终止渲染"  );
                return;
            }
            var cullingResults = context.Cull(ref cullingParameters);
            
            
            
            // ----------Begin：不透明---------
            // 渲染顺序
            var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
            // 渲染排序
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            
            // 设置支持的 LightMode Shader PassTag
            ShaderTagId shaderTagId = new ShaderTagId("Unlit");
            DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
            
            // 绘制命令
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            // ----------END---------
            
            
            
            // ----------Begin：渲染内置天空球---------
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                context.DrawSkybox(camera);
            }
            // ----------END---------
            
            
            
            // ----------Begin：渲染半透明---------
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            // ----------END---------
            
            
            
            // ----------Begin：编辑器内渲染---------
#if UNITY_EDITOR 
            DrawUnsupportedShaders(context,cullingResults);
            DrawGizmos(context);
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
        void DrawUnsupportedShaders(ScriptableRenderContext context, CullingResults cullingResults)
        {
            // 不受支持 LightMode Shader PassTag
            var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera));
            for (int i = 1; i < legacyShaderTagIds.Length; i++) 
                drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
            
            // 创建错误材质球
            if (errorMaterial == null) 
                errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
            
            drawingSettings.overrideMaterial = errorMaterial;
            // 剪裁设置
            var filteringSettings = FilteringSettings.defaultValue;
                
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        }
        
        // 渲染编辑器内UI
        void DrawGizmos (ScriptableRenderContext context) 
        {
            if (Handles.ShouldRenderGizmos()) 
            {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
        }
        
        // 渲染UI编辑器UI
        void PrepareForSceneWindow () 
        {
            if (camera.cameraType == CameraType.SceneView) 
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
        }
#endif

    }
}