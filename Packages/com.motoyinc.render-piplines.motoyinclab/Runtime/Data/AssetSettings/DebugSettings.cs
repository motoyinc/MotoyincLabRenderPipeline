using System;

namespace UnityEngine.Rendering.MotoyincLab
{
    public enum DebugMode
    {
        Off = 0,
        On = 1,
    }

    
    [System.Serializable]
    public class DebugSettings
    {
        [SerializeField] private DebugMode m_debugMode = DebugMode.Off;
        
        public DebugMode DebugMode
        {
            get { return m_debugMode; }
            set { m_debugMode = value; }
        }
        
    }
}