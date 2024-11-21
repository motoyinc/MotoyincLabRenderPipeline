using System;

namespace UnityEngine.Rendering.MotoyincLab
{
    [System.Serializable]
    public class ShadowSettings {
        [SerializeField][Min(0f)] float m_maxDistance = 100f;
        [SerializeField][Range(1,4)] int m_ShadowCascadeCount = 1;
        [SerializeField] ShadowResolution m_MainLightShadowmapResolution = ShadowResolution._1024;

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