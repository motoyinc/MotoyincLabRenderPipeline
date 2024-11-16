

namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabRenderPipeline
    {
        static ScriptableRenderer GetRenderer(Camera camera , MotoyincLabAdditionalCameraData additionalCameraData)
        {
            ScriptableRenderer renderer = null;
            if (additionalCameraData != null)
                renderer = additionalCameraData.scriptableRenderer;
            if (renderer == null || camera.cameraType == CameraType.SceneView)
                renderer = asset.scriptableRenderer;
            return renderer;
        }
        

    }
}