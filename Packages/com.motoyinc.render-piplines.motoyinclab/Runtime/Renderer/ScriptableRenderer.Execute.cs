using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.Rendering.MotoyincLab
{
    public abstract partial class ScriptableRenderer
    {
        List<ScriptableRenderPass> m_RenderPassList;

        public List<ScriptableRenderPass> renderPassList
        {
            get
            {
                if (m_RenderPassList == null)
                    return m_RenderPassList = new List<ScriptableRenderPass>();
                else
                    return m_RenderPassList;
            }
        }

        public void ClearRenderPassList()
        {
            if (m_RenderPassList == null)
                return;
            if (m_RenderPassList.Count != 0)
            {
                for (int i = 0; i < m_RenderPassList.Count; ++i)
                {
                    if (m_RenderPassList[i] != null)
                    {
                        m_RenderPassList[i].Dispose();
                        m_RenderPassList[i] = null;
                    }
                }

                m_RenderPassList = null;
            }
        }
        
        
        public abstract void Setup(ScriptableRenderContext context, ref RenderingData renderingData);
        
        public virtual void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData) { }

        public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData =renderingData.frameData.Get<MotoyincLabCameraData>();
            Camera camera = cameraData.camera;
            var cmd = renderingData.commandBuffer;
            
            // 设置灯光
            SetupLights(context, ref renderingData);
            
            // 设置摄像机属性
            context.SetupCameraProperties(camera);
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
        
        void ExecuteRenderPassList(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderPassList.Count != 0)
            {
                for (int i = 0; i < renderPassList.Count; ++i)
                {
                    if (renderPassList[i] != null)
                    {
                        renderPassList[i].Execute(context, ref renderingData);
                    }
                }
            }
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
    }
}