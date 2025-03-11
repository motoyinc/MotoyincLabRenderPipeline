using System;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.MotoyincLab
{
    public enum SupportSoftShadow
    {
        UsePipelineSettings,
        Off,
        On,
    }
    
    public enum SoftShadowQuality
    {
        PCF_3x3_Avg = 1,
        PCF_5x5_Tent = 2,
        PCF_7x7_Tent = 3,
        PCSS = 4
    }
    
    public partial class MotoyincLabAdditionalLightData 
    {
        [NonSerialized] private Light m_Light;
#if UNITY_EDITOR
        internal new Light light
#else
        internal Light light
#endif
        {
            get
            {
                if (!m_Light)
                {
                    gameObject.TryGetComponent(out m_Light);
                }
                return m_Light;
            }
        }
        
#if UNITY_EDITOR
        [SerializeField][Range(0,90)] private float m_innerSpotAngle;
        void OnValidate()
        {
            float? _innerSpotAngle = light?.innerSpotAngle;
            if (_innerSpotAngle.HasValue)
                light.innerSpotAngle = m_innerSpotAngle;
        }
#endif
        [SerializeField] private SupportSoftShadow m_SupportSoftShadow;
        public SupportSoftShadow supportSoftShadow
        {
            get => m_SupportSoftShadow;
            set => m_SupportSoftShadow = value;
        }
        
        [SerializeField] SoftShadowQuality m_ShadowQuality = SoftShadowQuality.PCF_5x5_Tent;
        public int shadowQuality
        {
            get
            {
                if (m_ShadowQuality == SoftShadowQuality.PCF_3x3_Avg)
                    return 1;
                if (m_ShadowQuality == SoftShadowQuality.PCF_5x5_Tent)
                    return 2;
                if (m_ShadowQuality == SoftShadowQuality.PCF_7x7_Tent)
                    return 3;
                if (m_ShadowQuality == SoftShadowQuality.PCSS)
                    return 4;
                return 2;
            }
            set
            {
                if (value == 1)
                    m_ShadowQuality = SoftShadowQuality.PCF_3x3_Avg;
                if (value == 2)
                    m_ShadowQuality = SoftShadowQuality.PCF_5x5_Tent;
                if (value == 3)
                    m_ShadowQuality = SoftShadowQuality.PCF_7x7_Tent;
                if (value == 4)
                    m_ShadowQuality = SoftShadowQuality.PCSS;
                m_ShadowQuality = SoftShadowQuality.PCF_5x5_Tent;
            }
        }

        [SerializeField] bool m_UsePipelineSettings = true;
        public bool usePipelineSettings
        {
            get { return m_UsePipelineSettings; }
            set { m_UsePipelineSettings = value; }
        }
        

        /// ////////////////////////////////////////////////////
        ///  pcss
        /// /////////////////////////////////////////////////////

        [Range(1, 100)]
        [SerializeField] float m_BlockerSearchRadiusWS = 2.0f;
        public float blockerSearchRadiusWS
        {
            get => m_BlockerSearchRadiusWS;
            set
            {
                m_BlockerSearchRadiusWS = Math.Max(value, 0.0f);
            }
        }
        
        // shadowFilterParams0.y
        [Range(1,64)]
        [SerializeField] int m_BlockerSampleCount = 24;
        public int blockerSampleCount
        {
            get => m_BlockerSampleCount;
            set
            {
                if(m_BlockerSampleCount == value)
                    return;
                m_BlockerSampleCount = Mathf.Clamp(value, 1, 64);
            }
        }
        
        // shadowFilterParams0.z
        [Range(1,64)]
        [SerializeField] int m_FilterSampleCount = 16;
        public int filterSampleCount
        {
            get => m_FilterSampleCount;
            set
            {
                if(m_FilterSampleCount == value)
                    return;
                m_FilterSampleCount = Mathf.Clamp(value, 1, 64);
            }
        }
        
        // 光源直径（太阳直径）
        [Range(1, 100)]
        [SerializeField] private float m_AngularDiameter = 0.5f;
        public float angularDiameter
        {
            get => m_AngularDiameter;
            set
            {
                if(m_AngularDiameter == value)
                    return;
                m_AngularDiameter = Mathf.Clamp(value, 0, float.MaxValue);
            }
        }
        
        [FormerlySerializedAs("m_DirLightPCSSBlockerSamplingClumpExponent")]
        [Range(1, 6)]
        [SerializeField] float BlockerSamplingClumpExponent = 2.0f;
        public float blockerSamplingClumpExponent
        {
            get => BlockerSamplingClumpExponent;
            set
            {
                BlockerSamplingClumpExponent = Math.Max(value, 0.0f);
            }
        }
        
        
        
    }
}