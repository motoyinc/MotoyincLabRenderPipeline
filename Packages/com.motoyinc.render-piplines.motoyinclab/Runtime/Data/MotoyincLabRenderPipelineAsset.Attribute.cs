namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabRenderPipelineAsset
    {
        // Render Data
        [SerializeField] internal ScriptableRendererData[] m_RendererDataList = new ScriptableRendererData[1];
        [SerializeField] internal int m_DefaultRendererIndex = 0;
        ScriptableRenderer[] m_Renderers = new ScriptableRenderer[1];
        
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

        // 获取RendererData
        internal ScriptableRendererData scriptableRendererData
        {
            get
            {
                return m_RendererDataList[m_DefaultRendererIndex];
            }
        }
        
        // 获取当前RendererData渲染器（只管当前RenderData的渲染器）
        public ScriptableRenderer scriptableRenderer
        {
            get
            {
                if (m_RendererDataList?.Length > m_DefaultRendererIndex && m_RendererDataList[m_DefaultRendererIndex] == null)
                {
                    Debug.LogError("当前管线Asset默认渲染器丢失,请创建RendererData",this);
                    return null;
                }
                // 检查渲染器是否存在
                if (scriptableRendererData.isInvalidated || m_Renderers[m_DefaultRendererIndex] == null)
                {
                    DestroyRenderer(ref m_Renderers[m_DefaultRendererIndex]);
                    m_Renderers[m_DefaultRendererIndex] = scriptableRendererData.InternalCreateRenderer();
                }
                return m_Renderers[m_DefaultRendererIndex];
            }
        }
        
        // 按引索获取渲染器（会检查所以有RenderData的渲染器状态）
        public ScriptableRenderer GetRenderer(int index)
        {
            if (index == -1)
                index = m_DefaultRendererIndex;
            if (index >= m_RendererDataList.Length || index < 0 || m_RendererDataList[index] == null)
            {
                Debug.LogError($"渲染器引索 index 异常{index.ToString()}，无法从该引索中获取渲染器，该方法将返回默认引索 RendererData 的渲染器：");
                index = m_DefaultRendererIndex;
            }

            if (m_Renderers == null || m_Renderers.Length != m_RendererDataList.Length)
            {
                DestroyRenderers();
                CreateRenderers();
            }

            return m_Renderers[index];
        }
        

    }
}