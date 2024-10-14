using System;

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

        static MotoyincLabCameraData CreateCameraData(ContextContainer frameData, Camera camera, MotoyincLabAdditionalCameraData additionalCameraData, bool resolveFinalTarget)
        {
            var renderer = GetRenderer(camera, additionalCameraData);
            MotoyincLabCameraData cameraData = frameData.GetOrCreate<MotoyincLabCameraData>();
            cameraData.Reset();
            cameraData.camera = camera;
            
            // 数据处理
            // ...
            
            return cameraData;
        }
        
    }
}