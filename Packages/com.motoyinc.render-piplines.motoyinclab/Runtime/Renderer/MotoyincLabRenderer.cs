namespace UnityEngine.Rendering.MotoyincLab
{
    public enum RenderingMode
    {
        Forward = 0,
        ForwardPuls = 1,
        Deferred = 2
    };
    public sealed partial class MotoyincLabRenderer : ScriptableRenderer
    {
        private RenderingMode m_RenderingMode;
        ForwardLights m_ForwardLights;
        private MainLightShadowCasterPass m_mainLightShadowCasterPass;
        private OpaqueRenderPass m_opaqueRenderPass;
        private SkyboxRenderPass m_skyboxRenderPass;
        private TransparentRenderPass m_transparentRenderPass;
        
        public MotoyincLabRenderer(MotoyincLabRendererData rendererData) : base(rendererData)
        {
            this.m_RenderingMode = rendererData.renderingMode;
            
            // 创建灯光对象
            m_ForwardLights = new ForwardLights();
            
            m_mainLightShadowCasterPass = new MainLightShadowCasterPass();
            m_opaqueRenderPass = new OpaqueRenderPass();
            m_skyboxRenderPass = new SkyboxRenderPass();
            m_transparentRenderPass = new TransparentRenderPass();
        }
        
        public override int SupportedCameraStackingTypes()
        {
            switch (m_RenderingMode)
            {
                case RenderingMode.Forward:
                    return 1 << (int)CameraRenderType.Base | 1 << (int)CameraRenderType.Overlay;
                case RenderingMode.Deferred:
                    return 1 << (int)CameraRenderType.Base;
                default:
                    return 0;
            }
            
        }

        protected override void Dispose(bool disposing)
        {
            m_ForwardLights.Cleanup();
            ClearRenderPassList();
            ReleaseRenderTargets();
        }

        internal override void ReleaseRenderTargets()
        {
            base.ReleaseRenderTargets();
            m_mainLightShadowCasterPass?.Dispose();
            m_opaqueRenderPass?.Dispose();
            m_skyboxRenderPass?.Dispose();
            m_transparentRenderPass?.Dispose();
        }
    }
}