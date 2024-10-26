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
            MotoyincLabCameraData cameraData = frameData.Create<MotoyincLabCameraData>();
            cameraData.Reset();
            cameraData.camera = camera;
            
            // 初始化填充CameraData
            InitializeStackedCamerData(camera, additionalCameraData, resolveFinalTarget, cameraData);
            
            // ...
            
            return cameraData;
        }
        
        // 初始化 CameraData 通用部分数据
        static void InitializeStackedCamerData(Camera baseCamera, MotoyincLabAdditionalCameraData additionalCameraData,
            bool resolveFinalTarget, MotoyincLabCameraData cameraData)
        {
            var settings = asset;

            // 填充数据
            cameraData.cameraType = baseCamera.cameraType;
            // ... 
        }
        
        // 初始化 CameraData 专用部分数据
        static void InitializeAdditionalCameraData(Camera camera, MotoyincLabAdditionalCameraData additionalCameraData,
            bool resolveFinalTarget, MotoyincLabCameraData cameraData)
        {
            var renderer = GetRenderer(camera, additionalCameraData);
            var settings = asset;
            
            // 填充数据
            cameraData.renderer = renderer;
            // ... 
        }

        static MotoyincLabRenderingData CreateRenderingData(ContextContainer frameData,
            MotoyincLabRenderPipelineAsset settings, CommandBuffer cmd, bool isForwardPlus, ScriptableRenderer renderer)
        {
            MotoyincLabRenderingData data = frameData.Get<MotoyincLabRenderingData>();
            data.supportsDynamicBatching = settings.supportsDynamicBatching;
            data.m_CommandBuffer = cmd;
            
            return data;
        }
        
    }
}