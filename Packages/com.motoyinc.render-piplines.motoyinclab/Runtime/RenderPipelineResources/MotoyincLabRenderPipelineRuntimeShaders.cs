using System;
namespace UnityEngine.Rendering.MotoyincLab
{
    [Serializable]
    [SupportedOnRenderPipeline(typeof(MotoyincLabRenderPipelineAsset))]
    public class MotoyincLabRenderPipelineRuntimeShaders : IRenderPipelineResources
    {
        // 资源版本
        [SerializeField][HideInInspector] private int m_Version = 0;
        public int version => m_Version;
        
        // 管线Shader资源
        bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;
        
        [SerializeField, ResourcePath("Shaders/Utils/FallbackError.shader")]
        Shader m_FallbackErrorShader;
        public Shader fallbackErrorShader
        {
            get => m_FallbackErrorShader;
            set => this.SetValueAndNotify(ref m_FallbackErrorShader, value, nameof(m_FallbackErrorShader));
        }
        
    }
}