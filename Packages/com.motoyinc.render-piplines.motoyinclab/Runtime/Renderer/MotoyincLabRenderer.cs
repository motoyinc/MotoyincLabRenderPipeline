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
        
        public MotoyincLabRenderer(MotoyincLabRendererData rendererData) : base(rendererData)
        {
            this.m_RenderingMode = rendererData.renderingMode;
            
            // 创建灯光对象
            m_ForwardLights = new ForwardLights();
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
        }
    }
}