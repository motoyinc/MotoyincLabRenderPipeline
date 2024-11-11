namespace UnityEngine.Rendering.MotoyincLab
{
    public struct LightData
    {
        internal ContextContainer frameData;

        internal LightData(ContextContainer frameData)
        {
            this.frameData = frameData;
        }
    }
}