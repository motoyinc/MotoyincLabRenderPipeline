using System.Collections.Generic;

namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class ScriptableRenderer
    {
        /// ///////////////////////////////////////////////////////////////////// /// 
        ///                                 Pass 属性                              ///
        /// ///////////////////////////////////////////////////////////////////// ///  
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
        
        
        
        /// ///////////////////////////////////////////////////////////////////// /// 
        ///                                 Pass 执行                              ///
        /// ///////////////////////////////////////////////////////////////////// ///  
        // 渲染Pass列表
        void ExecuteRenderPassList(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            if (renderPassList.Count != 0)
            {
                for (int i = 0; i < renderPassList.Count; ++i)
                {
                    if (renderPassList[i] != null)
                    {
                        // renderPassList[i].Execute(context, ref renderingData);
                        ExecuteRenderPass(context, renderPassList[i], cameraData, ref renderingData);
                    }
                }
            }
        }
        
        // 渲染Pass
        void ExecuteRenderPass(ScriptableRenderContext context, ScriptableRenderPass renderPass, MotoyincLabCameraData cameraData, ref RenderingData renderingData)
        {
            var cmd = renderingData.commandBuffer;
            
            // 设置渲染目标 SetRenderTarget
            SetRenderPassAttachments(cmd, renderPass, cameraData);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            // 执行渲染Pass
            renderPass.Execute(context, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        
        // 设置渲染目标 SetRenderTarget
        void SetRenderPassAttachments(CommandBuffer cmd, ScriptableRenderPass renderPass, MotoyincLabCameraData cameraData)
        {
            // cmd.SetRenderTarget();
        }
    }
}