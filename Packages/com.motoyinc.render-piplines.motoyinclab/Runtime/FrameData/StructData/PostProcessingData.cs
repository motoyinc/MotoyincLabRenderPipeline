namespace UnityEngine.Rendering.MotoyincLab
{
    public struct PostProcessingData
    {
        internal ContextContainer frameData;

        internal PostProcessingData(ContextContainer frameData)
        {
            this.frameData = frameData;
        }
    }
}