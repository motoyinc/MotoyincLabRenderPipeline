using System;

namespace UnityEngine.Rendering.MotoyincLab
{
    public enum DebugMode
    {
        Off = 0,
        On = 1,
    }

    public enum GeometryBufferMode
    {
        RendingComplete = 0,
        
        // 物体信息
        Color = 1,
        Metallic = 2,
        Roughness = 3,
        Alpha = 4,
        
        // 空间信息
        PositionWorldSpace = 10,
        NormalWorldSpace = 11,
        ViewDirectionWorldSpace = 12,
        MainShadowSpace = 13,
        
        // 灯光
        MainLightColor = 30,
        MainLightShadow = 31,
        
        // Debug
        DebugOutput = 100
        
    }

    
    [System.Serializable]
    public class DebugSettings
    {
        [SerializeField] private DebugMode m_DebugMode = DebugMode.Off;
        [SerializeField] private bool m_DiplayShadowCascade = false;
        [SerializeField] private GeometryBufferMode m_GeometryBufferMode = GeometryBufferMode.RendingComplete;
        
        public DebugMode DebugMode
        {
            get { return m_DebugMode; }
            set { m_DebugMode = value; }
        }
        
        public int displayShadowCascade
        {
            get
            {
                if (m_DiplayShadowCascade)
                    return 1;
                return 0;
            }
        }

        public GeometryBufferMode geometryBufferMode
        {
            get { return m_GeometryBufferMode; }
        }
    }
}