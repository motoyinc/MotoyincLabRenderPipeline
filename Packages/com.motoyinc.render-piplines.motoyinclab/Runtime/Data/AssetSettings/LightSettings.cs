namespace UnityEngine.Rendering.MotoyincLab
{
    [System.Serializable]
    public class LightSettings
    {
        [SerializeField] bool m_MainLightShadowsSupported = true;
        [SerializeField] bool m_AdditionalLightShadowsSupported = false;
        
        public bool supportsMainLightShadows
        {
            get => m_MainLightShadowsSupported;
            set => m_MainLightShadowsSupported = value;
        }
        
        public bool supportsAdditionalLightShadows
        {
            get => m_AdditionalLightShadowsSupported;
            set => m_AdditionalLightShadowsSupported = value;
        }
    }
}