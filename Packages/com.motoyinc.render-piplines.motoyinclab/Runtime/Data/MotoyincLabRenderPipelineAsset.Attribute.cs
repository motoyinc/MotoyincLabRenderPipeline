using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabRenderPipelineAsset
    {
        // Render Data
        [SerializeField] internal ScriptableRendererData[] m_RendererDataList = new ScriptableRendererData[1];
        [SerializeField] internal int m_DefaultRendererIndex = 0;
        
        // Quality settings
        [SerializeField] bool m_SupportsHDR = true;
        
        // Advanced settings
        [SerializeField] bool m_UseSRPBatcher = true;

        public bool supportsHDR
        {
            get => m_SupportsHDR;
            set => m_SupportsHDR = value;
        }

        public bool useSRPBatcher
        {
            get => m_UseSRPBatcher;
            set => m_UseSRPBatcher = value;
        }
    }
}