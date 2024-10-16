namespace UnityEngine.Rendering.MotoyincLab
{
    public sealed partial class MotoyincLabRenderer : ScriptableRenderer
    {
        private RenderingMode m_RenderingMode;
        
        public MotoyincLabRenderer(MotoyincLabRendererData rendererData) : base(rendererData)
        {
            this.m_RenderingMode = rendererData.renderingMode;
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
    }
}