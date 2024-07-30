
namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabRenderPipeline
    {
        // 兼容性相机的 RenderCamera
        public static void RenderSingleCamera(ScriptableRenderContext context,Camera camera)
        {
            RenderSingleCameraInternal(context,camera);
        }
        
        // 其他类型相机的 RenderCamera
        internal static void RenderSingleCameraInternal(ScriptableRenderContext context, Camera camera)
        {
            MotoyincLabAdditionalCameraData additionalCameraData = null;
            if (isGameCamera(camera))
                camera.gameObject.TryGetComponent(out additionalCameraData);

            RenderSingleCameraInternal(context,camera,ref additionalCameraData);
        }
        
        internal static void RenderSingleCameraInternal(ScriptableRenderContext context, Camera camera, ref MotoyincLabAdditionalCameraData additionalCameraData)
        {
            // 获取 Renderer
            camera.TryGetComponent<MotoyincLabAdditionalCameraData>(out var baseAdditionalCameraData);
            ScriptableRenderer renderer = null;
            if (baseAdditionalCameraData != null)
                renderer = baseAdditionalCameraData.scriptableRenderer;
            if (renderer == null || camera.cameraType == CameraType.SceneView)
                renderer = asset.scriptableRenderer;
            
        }
        
        
        // 正常游戏相机的 RenderCamera
        static void RenderCameraStack(ScriptableRenderContext context, Camera baseCamera)
        {
            // 获取 Renderer
            baseCamera.TryGetComponent<MotoyincLabAdditionalCameraData>(out var baseAdditionalCameraData);
            ScriptableRenderer renderer = null;
            if (baseAdditionalCameraData != null)
                renderer = baseAdditionalCameraData.scriptableRenderer;
            if (renderer == null || baseCamera.cameraType == CameraType.SceneView)
                renderer = asset.scriptableRenderer;
        }
        
        
        // RenderCamera
        static void RenderSingleCamera(ScriptableRenderContext context, MotoyincLabCameraData cameraData)
        {
            
        }
        
    }
}