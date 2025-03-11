using System;

namespace UnityEngine.Rendering.MotoyincLab
{
    [System.Serializable]
    public class ShadowSettings {
        [SerializeField][Min(0f)] float m_maxDistance = 100f;
        [SerializeField][Range(1,4)] int m_ShadowCascadeCount = 1;
        [SerializeField][Range(0.0f,1.0f)] float m_Cascade2Split = 0.25f;
        [SerializeField] Vector2 m_Cascade3Split = new Vector2(0.1f, 0.3f);
        [SerializeField] Vector3 m_Cascade4Split = new Vector3(0.067f, 0.2f, 0.467f);
        [SerializeField][Range(0.0f,1.0f)] float m_CascadeBorder = 0.2f;
        [SerializeField] ShadowResolution m_MainLightShadowmapResolution = ShadowResolution._1024;
        // 软阴影
        [SerializeField] bool m_SoftShadowsSupported = false;
        [SerializeField]SoftShadowQuality m_ShadowQuality = SoftShadowQuality.PCF_5x5_Tent;
        [SerializeField][Range(0,10)] float m_ShadowDepthBias = 1.0f;
        [SerializeField][Range(0,2)] float m_ShadowNormalBias = 0.3f;
        
        
        public float maxDistance
        {
            get => m_maxDistance;
        }
        public int mainLightShadowmapResolution
        {
            get => (int)m_MainLightShadowmapResolution;
            set => m_MainLightShadowmapResolution = (ShadowResolution)value;
        }
        
        internal const int k_ShadowCascadeMinCount = 1;
        internal const int k_ShadowCascadeMaxCount = 4;
        public int shadowCascadeCount
        {
            get => m_ShadowCascadeCount;
            set
            {
                if (value < k_ShadowCascadeMinCount || value > k_ShadowCascadeMaxCount)
                {
                    throw new ArgumentException($"Value ({value}) needs to be between {k_ShadowCascadeMinCount} and {k_ShadowCascadeMaxCount}.");
                }
                m_ShadowCascadeCount = value;
            }
        }
        
        public float cascade2Split
        {
            get => m_Cascade2Split;
            set => m_Cascade2Split = value;
        }
        
        public Vector2 cascade3Split
        {
            get => m_Cascade3Split;
            set => m_Cascade3Split = value;
        }
        
        public Vector3 cascade4Split
        {
            get => m_Cascade4Split;
            set => m_Cascade4Split = value;
        }
        public float cascadeBorder
        {
            get => m_CascadeBorder;
            set => m_CascadeBorder = value;
        }
        
        public bool supportsSoftShadows
        {
            get => m_SoftShadowsSupported;
            internal set => m_SoftShadowsSupported = value;
        }
        
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
        
        public float shadowDepthBias
        {
            get => m_ShadowDepthBias;
            set => m_ShadowDepthBias = ValidateShadowBias(value);
        }
        
        public float shadowNormalBias
        {
            get => m_ShadowNormalBias;
            set => m_ShadowNormalBias = ValidateShadowBias(value);
        }
        
        public static float maxShadowBias
        {
            get => 10.0f;
        }
        
        float ValidateShadowBias(float value)
        {
            return Mathf.Max(0.0f, Mathf.Min(value, maxShadowBias));
        }
        
    }
    
    public enum ShadowResolution {
        _256 = 256, 
        _512 = 512, 
        _1024 = 1024,
        _2048 = 2048, 
        _4096 = 4096, 
        _8192 = 8192
    } 
    
}