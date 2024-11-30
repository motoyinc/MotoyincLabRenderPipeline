namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class ScriptableRenderer
    {
        // 管线默认RT
        protected static readonly RTHandle k_CameraTarget = RTHandles.Alloc(BuiltinRenderTextureType.CameraTarget);
        
        
        // 管线全局RT
        RTHandle m_CameraColorTarget;
        RTHandle m_CameraDepthTarget;
        RTHandle m_CameraResolveTarget;
        
        
        // 当前管线的活跃RT
        static RTHandle[] m_ActiveColorAttachments = new RTHandle[8];
        static RTHandle m_ActiveDepthAttachment;
        static RenderTargetIdentifier[] m_ActiveColorAttachmentIDs = new RenderTargetIdentifier[8];
        private static RenderBufferStoreAction[] m_ActiveColorStoreActions = new RenderBufferStoreAction[]
        {
            RenderBufferStoreAction.Store, RenderBufferStoreAction.Store, RenderBufferStoreAction.Store, RenderBufferStoreAction.Store,
            RenderBufferStoreAction.Store, RenderBufferStoreAction.Store, RenderBufferStoreAction.Store, RenderBufferStoreAction.Store
        };
        private static RenderBufferStoreAction m_ActiveDepthStoreAction = RenderBufferStoreAction.Store;
        
        
        // 首次使用默认相机RT
        bool m_FirstTimeCameraColorTargetIsBound = true;
        bool m_FirstTimeCameraDepthTargetIsBound = true;
        
        
        // 初始化活跃RT
        internal void Clear(CameraRenderType cameraType)
        {
            m_ActiveColorAttachments[0] = k_CameraTarget;
            for (int i = 1; i < m_ActiveColorAttachments.Length; ++i)
                m_ActiveColorAttachments[i] = null;
            for (int i = 0; i < m_ActiveColorAttachments.Length; ++i)
                m_ActiveColorAttachmentIDs[i] = m_ActiveColorAttachments[i]?.nameID ?? 0;
            m_ActiveDepthAttachment = null;
            m_ActiveDepthAttachment = k_CameraTarget;
            m_FirstTimeCameraColorTargetIsBound = cameraType == CameraRenderType.Base;
            m_FirstTimeCameraDepthTargetIsBound = true;
            m_CameraColorTarget = null;
            m_CameraDepthTarget = null;
        }
        
        internal virtual void ReleaseRenderTargets()
        {
        }
        
        
        /// ///////////////////////////////////////////////////////////////////// /// 
        ///                        为Renderer 设置全局RT                            ///
        /// ///////////////////////////////////////////////////////////////////// ///  
        
        // 设置全局RT
        internal void ConfigureCameraColorTarget(RTHandle colorTarget)
        {
            m_CameraColorTarget = colorTarget;
        }
        
        public void ConfigureCameraTarget(RTHandle colorTarget, RTHandle depthTarget)
        {
            m_CameraColorTarget = colorTarget;
            m_CameraDepthTarget = depthTarget;
        }
        
        internal void ConfigureCameraTarget(RTHandle colorTarget, RTHandle depthTarget, RTHandle resolveTarget)
        {
            m_CameraColorTarget = colorTarget;
            m_CameraDepthTarget = depthTarget;
            m_CameraResolveTarget = resolveTarget;
        }
        
    }
}