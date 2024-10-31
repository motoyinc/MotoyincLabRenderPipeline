using System;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.MotoyincLab
{
    public enum LightRenderingMode
    {
        Disabled,
        PerPixel,
        PerVertex,
    }
    public enum RendererType
    {
        Custom,
        MotoyincLabRenderer,
        _2DRenderer,
    }
    
    public partial class MotoyincLabRenderPipelineAsset
    {
        // Renderer Data 与 Renderer
        [SerializeField] internal ScriptableRendererData[] m_RendererDataList = new ScriptableRendererData[1];
        [SerializeField] internal int m_DefaultRendererIndex = 0;
        ScriptableRenderer[] m_Renderers = new ScriptableRenderer[1];
        
        // Quality settings
        [SerializeField] bool m_SupportsHDR = true;
        
        // Advanced settings
        [SerializeField] bool m_UseSRPBatcher = true;
        [SerializeField] bool m_SupportsDynamicBatching = false;
        
        // Lighting setting
        [SerializeField] LightRenderingMode m_MainLightRenderingMode = LightRenderingMode.PerPixel;

#if UNITY_EDITOR
        // Debug mode
        [SerializeField] private GlobalDebugMode m_GlobalDebugMode = GlobalDebugMode.Off;

        public GlobalDebugMode globalDebugMode
        {
            get => m_GlobalDebugMode;
            set => m_GlobalDebugMode = value;
        }
#endif

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
        
        public bool supportsDynamicBatching
        {
            get => m_SupportsDynamicBatching;
            set => m_SupportsDynamicBatching = value;
        }
        
        public LightRenderingMode mainLightRenderingMode
        {
            get => m_MainLightRenderingMode;
            internal set => m_MainLightRenderingMode = value;
        }
        
        public ReadOnlySpan<ScriptableRenderer> renderers => m_Renderers;
        
        // 获取RendererData
        internal ScriptableRendererData scriptableRendererData
        {
            get
            {
                return m_RendererDataList[m_DefaultRendererIndex];
            }
        }
        
        // 获取默认Renderer渲染器（会检测RenderData的对应的Renderer渲染器是否正常）
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
        
        // 按引索获取Renderer渲染器（会检查所以有RenderData的对应的Renderer渲染器状态是否正常）
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
        
        // 用于检查当前序号是否允许获取Renderer
        internal bool ValidateRendererData(int index)
        {
            // if (index == -1) index = m_DefaultRendererIndex;
            // return index < m_RendererDataList.Length ? m_RendererDataList[index] != null : false;
            if (index == -1)
                index = m_DefaultRendererIndex;
            if (index < m_RendererDataList.Length)
                if (m_RendererDataList[index] != null)
                    return true;
            return false;
        }

    }
}