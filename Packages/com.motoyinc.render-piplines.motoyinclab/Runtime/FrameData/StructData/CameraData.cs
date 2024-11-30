namespace UnityEngine.Rendering.MotoyincLab
{
    public struct CameraData
    {
        internal ContextContainer frameData;

        internal CameraData(ContextContainer frameData)
        {
            this.frameData = frameData;
        }
        
        public ref float maxShadowDistance => ref frameData.Get<MotoyincLabCameraData>().maxShadowDistance;
    }
}