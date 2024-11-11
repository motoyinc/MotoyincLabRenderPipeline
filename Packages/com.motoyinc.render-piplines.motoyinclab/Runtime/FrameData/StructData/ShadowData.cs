namespace UnityEngine.Rendering.MotoyincLab
{
    public struct ShadowData
    {
        internal ContextContainer frameData;

        internal ShadowData(ContextContainer frameData)
        {
            this.frameData = frameData;
        }
    }
}