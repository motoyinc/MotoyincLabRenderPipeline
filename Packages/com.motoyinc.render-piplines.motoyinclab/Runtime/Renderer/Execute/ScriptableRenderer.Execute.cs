using UnityEditor;

namespace UnityEngine.Rendering.MotoyincLab
{
    public abstract partial class ScriptableRenderer
    {
        
        public abstract void Setup(ScriptableRenderContext context, ref RenderingData renderingData);
        
        public virtual void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData) { }
        
        public virtual void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData) { }

        public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData =renderingData.frameData.Get<MotoyincLabCameraData>();
            Camera camera = cameraData.camera;
            var cmd = renderingData.commandBuffer;
            
            // 清理 Keywords
            ClearRenderingState(CommandBufferHelpers.GetRasterCommandBuffer(cmd));
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
#if UNITY_EDITOR
            SetDebugMode(cmd, renderingData.debugSettings);
#endif
            
            // 执行Pass Configure
            ConfigureRenderPassList(context, ref renderingData);
            
            // 设置灯光
            SetupLights(context, ref renderingData);
            
            // 设置摄像机属性
            SetPerCameraProperties(context, cameraData, camera, cmd);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            // 执行Pass列表
            ExecuteRenderPassList(context, ref renderingData);
            
#if UNITY_EDITOR
            // 渲染Gizmos
            if (cameraData.isSceneViewCamera)
            {
                SetPerCameraProperties(context, cameraData, camera, cmd);
                DrawGizmos(context, cmd, camera);
            }
            
#endif
        }

#if UNITY_EDITOR     
        void DrawGizmos (ScriptableRenderContext context, CommandBuffer cmd, Camera camera) 
        {
            if (!Handles.ShouldRenderGizmos() || camera.sceneViewFilterMode == Camera.SceneViewFilterMode.ShowFiltered)
                return;
            {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
        }
        
        // Debug设置
        static void SetDebugMode(CommandBuffer cmd, DebugSettings debugSettings)
        {
            cmd.SetGlobalInt(ShaderPropertyId.DisplayShadowCascade, debugSettings.displayShadowCascade);
            cmd.SetGlobalInt(ShaderPropertyId.DisplayGeometryBufferMode, (int)debugSettings.geometryBufferMode);
            cmd.SetKeyword(ShaderGlobalKeywords.DebugMode, debugSettings.DebugMode == DebugMode.On);
        }
#endif
        
        // 初始相机设置
        internal void SetPerCameraProperties(ScriptableRenderContext context, MotoyincLabCameraData cameraData, Camera camera, CommandBuffer cmd)
        {
            context.SetupCameraProperties(camera);
            cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, cameraData.worldSpaceCameraPos);
        }
        

        static void ClearRenderingState(IBaseCommandBuffer cmd)
        {
            using var profScope = new ProfilingScope(Profiling.clearRenderingState);
            cmd.SetKeyword(ShaderGlobalKeywords.MainLightShadows, false);
            cmd.SetKeyword(ShaderGlobalKeywords.MainLightShadowCascades, false);
        }
        
    }
}