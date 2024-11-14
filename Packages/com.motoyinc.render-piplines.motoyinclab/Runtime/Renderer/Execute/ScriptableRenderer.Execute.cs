using UnityEditor;

namespace UnityEngine.Rendering.MotoyincLab
{
    public abstract partial class ScriptableRenderer
    {
        
        public abstract void Setup(ScriptableRenderContext context, ref RenderingData renderingData);
        
        public virtual void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData) { }

        public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData =renderingData.frameData.Get<MotoyincLabCameraData>();
            Camera camera = cameraData.camera;
            var cmd = renderingData.commandBuffer;
            
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
                DrawGizmos(context,camera);
#endif
        }

#if UNITY_EDITOR     
        void DrawGizmos (ScriptableRenderContext context,Camera camera) 
        {
            if (Handles.ShouldRenderGizmos()) 
            {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
        }
#endif
        
        // 初始相机设置
        internal void SetPerCameraProperties(ScriptableRenderContext context, MotoyincLabCameraData cameraData, Camera camera, CommandBuffer cmd)
        {
            context.SetupCameraProperties(camera);
            cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, cameraData.worldSpaceCameraPos);
        }
    }
}