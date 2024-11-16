using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class ScriptableRenderPass
    {
        // 默认相机RT
        internal bool overrideCameraTarget { get; set; }
        public static RTHandle k_CameraTarget = RTHandles.Alloc(BuiltinRenderTextureType.CameraTarget);
        
        // PassRT
        RTHandle[] m_ColorAttachments;
        RTHandle m_DepthAttachment;
        RenderBufferStoreAction[] m_ColorStoreActions = new RenderBufferStoreAction[] { RenderBufferStoreAction.Store };
        RenderBufferStoreAction m_DepthStoreAction = RenderBufferStoreAction.Store;
        
        public RTHandle[] colorAttachmentHandles => m_ColorAttachments;
        public RTHandle colorAttachmentHandle => m_ColorAttachments[0];
        public RTHandle depthAttachmentHandle => m_DepthAttachment;
        public RenderBufferStoreAction[] colorStoreActions => m_ColorStoreActions;
        public RenderBufferStoreAction depthStoreAction => m_DepthStoreAction;
        
        // 相机RT渲染前清理
        ClearFlag m_ClearFlag = ClearFlag.None;
        Color m_ClearColor = Color.black;
        public ClearFlag clearFlag => m_ClearFlag;
        public Color clearColor => m_ClearColor;

        public void ConfigureClear(ClearFlag clearFlag, Color clearColor)
        {
            m_ClearFlag = clearFlag;
            m_ClearColor = clearColor;
        }
        
        
        // 构造函数初始化
        public ScriptableRenderPass()
        {
            overrideCameraTarget = false;
            m_ColorAttachments = new RTHandle[] { k_CameraTarget, null, null, null, null, null, null, null };
            m_DepthAttachment = k_CameraTarget;
            m_ColorStoreActions = new RenderBufferStoreAction[] { RenderBufferStoreAction.Store, 0, 0, 0, 0, 0, 0, 0 };
            m_DepthStoreAction = RenderBufferStoreAction.Store;
            m_ClearFlag = ClearFlag.None;
            m_ClearColor = Color.black;
        }

        // 清理RT
        public void ResetTarget()
        {
            overrideCameraTarget = false;
            m_DepthAttachment = null;
            m_ColorAttachments[0] = null;
            for (int i = 1; i < m_ColorAttachments.Length; ++i)
            {
                m_ColorAttachments[i] = null;
            }
        }
        
        
        // 设置Pass 渲染目标
        public void ConfigureTarget(RTHandle colorAttachment)
        {
            ConfigureTarget(colorAttachment, k_CameraTarget);
        }
        
        public void ConfigureTarget(RTHandle colorAttachment, RTHandle depthAttachment)
        {
            overrideCameraTarget = true;

            m_DepthAttachment = depthAttachment;
            m_ColorAttachments[0] = colorAttachment;
            for (int i = 1; i < m_ColorAttachments.Length; ++i)
            {
                m_ColorAttachments[i] = null;
            }
        }
        
        
        
        // 设置MRT 渲染目标
        public void ConfigureTarget(RTHandle[] colorAttachments)
        {
            ConfigureTarget(colorAttachments, k_CameraTarget);
        }
        
        public void ConfigureTarget(RTHandle[] colorAttachments, RTHandle depthAttachment)
        {
            overrideCameraTarget = true;
            
            // 检查MRT数量
            uint nonNullColorBuffers = RenderingUtils.GetValidColorBufferCount(colorAttachments);
            if (nonNullColorBuffers > SystemInfo.supportedRenderTargetCount)
                Debug.LogError($"当前 <\b>{m_PassName}<b> 尝试传入MRT数量： <\b>{nonNullColorBuffers}<b>，已经超出当前平台允许的最大MRT数量：{SystemInfo.supportedRenderTargetCount}");
            if (colorAttachments.Length > m_ColorAttachments.Length)
                Debug.LogError($"当前 <\b>{m_PassName}<b> 尝试传入RTHandls[]长度：  <\b>{colorAttachments.Length}<b> + 超出本管线设置的最大长度:" + m_ColorAttachments.Length);

            // RTHandle传入Pass
            for (int i = 0; i < colorAttachments.Length; ++i)
                m_ColorAttachments[i] = colorAttachments[i];
            // 将多出来的长度，设置为null
            for (int i = colorAttachments.Length; i < m_ColorAttachments.Length; ++i)
                m_ColorAttachments[i] = null;

            m_DepthAttachment = depthAttachment;
        }

    }
}