namespace UnityEngine.Rendering.MotoyincLab
{
    public struct RenderingData
    {
        internal ContextContainer frameData;
        internal CameraData cameraData;
        internal LightData lightData;
        internal ShadowData shadowData;
        internal PostProcessingData postProcessingData;

        internal RenderingData(ContextContainer frameData)
        {
            this.frameData = frameData;
            cameraData = new CameraData(frameData);
            lightData = new LightData(frameData);
            shadowData = new ShadowData(frameData);
            postProcessingData = new PostProcessingData(frameData);
        }
        internal MotoyincLabRenderingData MotoyincLabRenderingData => frameData.Get<MotoyincLabRenderingData>();

        // Non-rendergraph path only. Do NOT use with rendergraph!
        internal ref CommandBuffer commandBuffer
        {
            get
            {
                ref var cmd = ref frameData.Get<MotoyincLabRenderingData>().m_CommandBuffer;
                if (cmd == null)
                    Debug.LogError("RenderingData.commandBuffer is null. RenderGraph does not support this property. Please use the command buffer provided by the RenderGraphContext.");

                return ref cmd;
            }
        }
        
        public ref bool supportsDynamicBatching => ref frameData.Get<MotoyincLabRenderingData>().supportsDynamicBatching;

#if UNITY_EDITOR
        public ref GlobalDebugMode globalDebugMode => ref frameData.Get<MotoyincLabRenderingData>().globalDebugMode;
#endif

    }
    
}