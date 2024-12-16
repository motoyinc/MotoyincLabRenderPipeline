using System;
namespace UnityEngine.Rendering.MotoyincLab
{
    public enum SupportSoftShadow
    {
        UsePipelineSettings,
        Off,
        On,
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
        
        [SerializeField][Range(1,3)] int m_ShadowQuality = 1;
        public int shadowQuality
        {
            get => m_ShadowQuality;
            set => m_ShadowQuality = value;
        }
        
        [SerializeField] bool m_UsePipelineSettings = true;
        public bool usePipelineSettings
        {
            get { return m_UsePipelineSettings; }
            set { m_UsePipelineSettings = value; }
        }
    }
}