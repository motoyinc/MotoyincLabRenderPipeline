﻿using System;
namespace UnityEngine.Rendering.MotoyincLab
{
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
        
    }
}