using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
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

        public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData =renderingData.frameData.Get<MotoyincLabCameraData>();
            Camera camera = cameraData.camera;
            
            ExecuteRenderPass(context, ref renderingData);
            
            // camera.TryGetCullingParameters(out var cullingParameters);
            // var cullingResults = context.Cull(ref cullingParameters);
            // context.SetupCameraProperties(camera);
            // ShaderTagId shaderTagId = new ShaderTagId("Unlit");
            // var sortingSettings = new SortingSettings(camera);
            // DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
            // FilteringSettings filteringSettings = FilteringSettings.defaultValue;
            // context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            // if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            // {
            //     context.DrawSkybox(camera);
            // }
        }
        
        void ExecuteRenderPass(ScriptableRenderContext context, ref RenderingData renderingData)
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
    }
}