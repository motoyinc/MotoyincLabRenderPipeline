namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabRenderPipeline
    {
        static MotoyincLabRenderingData CreateRenderingData(ContextContainer frameData,
            MotoyincLabRenderPipelineAsset settings, CommandBuffer cmd, bool isForwardPlus, ScriptableRenderer renderer)
        {
            MotoyincLabRenderingData data = frameData.Get<MotoyincLabRenderingData>();
            data.supportsDynamicBatching = settings.supportsDynamicBatching;
            data.m_CommandBuffer = cmd;
#if UNITY_EDITOR
            data.globalDebugMode = settings.globalDebugMode;
#endif
            
            return data;
        }
    }
}