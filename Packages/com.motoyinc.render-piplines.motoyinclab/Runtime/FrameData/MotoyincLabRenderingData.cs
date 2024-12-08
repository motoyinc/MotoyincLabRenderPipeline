namespace UnityEngine.Rendering.MotoyincLab
{
    public class MotoyincLabRenderingData : ContextItem
    {
        internal CommandBuffer m_CommandBuffer;
        public CullingResults cullResults;
        public bool supportsDynamicBatching;
#if UNITY_EDITOR
        public DebugSettings debugSettings;
#endif
        

        internal CommandBuffer commandBuffer
        {
            get
            {
                if(m_CommandBuffer == null)
                    Debug.LogError("<b> MotoyincLabRenderingData.commandBuffer </b> 无法找到");
                return m_CommandBuffer;
            }
        }
        public override void Reset()
        {
            m_CommandBuffer = default;
            cullResults = default;
            supportsDynamicBatching = default;
#if UNITY_EDITOR
            debugSettings = default;
#endif
        }
    }
}