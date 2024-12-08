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
    
    public enum HDRColorBufferPrecision
    {
        /// <summary> Typically R11G11B10f for faster rendering. Recommend for mobile.
        /// R11G11B10f can cause a subtle blue/yellow banding in some rare cases due to lower precision of the blue component.</summary>
        [Tooltip("Use 32-bits per pixel for HDR rendering.")]
        _32Bits,
        /// <summary>Typically R16G16B16A16f for better quality. Can reduce banding at the cost of memory and performance.</summary>
        [Tooltip("Use 64-bits per pixel for HDR rendering.")]
        _64Bits,
    }
    
    public enum MsaaQuality
    {
        Disabled = 1,
        _2x = 2,
        _4x = 4,
        _8x = 8
    }
    
    public partial class MotoyincLabRenderPipelineAsset
    {
        // Renderer Data 与 Renderer
        [SerializeField] internal ScriptableRendererData[] m_RendererDataList = new ScriptableRendererData[1];
        [SerializeField] internal int m_DefaultRendererIndex = 0;
        ScriptableRenderer[] m_Renderers = new ScriptableRenderer[1];
        
        // Quality settings
        [SerializeField] bool m_SupportsHDR = true;
        [SerializeField] HDRColorBufferPrecision m_HDRColorBufferPrecision = HDRColorBufferPrecision._32Bits;
        [SerializeField] MsaaQuality m_MSAA = MsaaQuality.Disabled;
        [SerializeField] float m_RenderScale = 1.0f;
        
        
        // Advanced settings
        [SerializeField] bool m_UseSRPBatcher = true;
        [SerializeField] bool m_SupportsDynamicBatching = false;
        
        // Lighting setting
        [SerializeField] LightSettings m_lightSettings = default;
        
        // Shadow
        [SerializeField] private ShadowSettings m_shadowSettings = default;

#if UNITY_EDITOR
        // Debug mode
        [SerializeField] private DebugSettings m_DebugSettings = default;

        public DebugSettings debugSettings
        {
            get => m_DebugSettings;
            set => m_DebugSettings = value;
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
        
        
        public LightSettings lightSettings
        {
            get => m_lightSettings;
        }

        public ShadowSettings shadowSettings
        {
            get => m_shadowSettings;
        }
        
        public HDRColorBufferPrecision hdrColorBufferPrecision
        {
            get => m_HDRColorBufferPrecision;
            set => m_HDRColorBufferPrecision = value;
        }
        
        public int msaaSampleCount
        {
            get => (int)m_MSAA;
            set => m_MSAA = (MsaaQuality)value;
        }
        
        public float renderScale
        {
            get => m_RenderScale;
            set => m_RenderScale = ValidateRenderScale(value);
        }
        float ValidateRenderScale(float value)
        {
            return Mathf.Max(MotoyincLabRenderPipeline.minRenderScale, Mathf.Min(value, MotoyincLabRenderPipeline.maxRenderScale));
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
            if (index == -1)
                index = m_DefaultRendererIndex;
            if (index < m_RendererDataList.Length && index >= 0)
                if (m_RendererDataList[index] != null)
                    return true;
            return false;
        }

    }
}