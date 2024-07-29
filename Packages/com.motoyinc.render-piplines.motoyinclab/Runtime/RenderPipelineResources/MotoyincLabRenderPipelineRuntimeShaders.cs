using System;
namespace UnityEngine.Rendering.MotoyincLab
{
    public class MotoyincLabRenderPipelineRuntimeShaders : IRenderPipelineResources
    {
        // 资源版本
        [SerializeField][HideInInspector] private int m_Version = 0;
        public int version => m_Version;
        
        // 管线Shader资源
        bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;
        [SerializeField]
        [ResourcePath("Shaders/Utils/CoreBlit.shader")]
        internal Shader m_CoreBlitPS;
        
        public Shader coreBlitPS
        {
            get => m_CoreBlitPS;
            set => this.SetValueAndNotify(ref m_CoreBlitPS, value, nameof(m_CoreBlitPS));
        }

        [SerializeField]
        [ResourcePath("Shaders/Utils/CoreBlitColorAndDepth.shader")]
        internal Shader m_CoreBlitColorAndDepthPS;
        
        public Shader coreBlitColorAndDepthPS
        {
            get => m_CoreBlitColorAndDepthPS;
            set => this.SetValueAndNotify(ref m_CoreBlitColorAndDepthPS, value, nameof(m_CoreBlitColorAndDepthPS));
        }
    }
}