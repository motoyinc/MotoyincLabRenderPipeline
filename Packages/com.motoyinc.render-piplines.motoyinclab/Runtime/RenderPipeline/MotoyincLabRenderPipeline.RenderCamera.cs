using System;
using Unity.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering.MotoyincLab;
#endif
using UnityEngine.Scripting.APIUpdating;
using Lightmapping = UnityEngine.Experimental.GlobalIllumination.Lightmapping;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Profiling;
using static UnityEngine.Camera;


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
        
        
        // 摄像机堆叠循环 
        static void RenderCameraStack(ScriptableRenderContext context, Camera baseCamera)
        {
            using var profScope = new ProfilingScope(ProfilingSampler.Get(MLRPProfileId.RenderCameraStack));
            
            // 获取 AdditionalCameraData
            baseCamera.TryGetComponent<MotoyincLabAdditionalCameraData>(out var baseAdditionalCameraData);
            
            // 检查相机类型
            if (baseAdditionalCameraData != null && baseAdditionalCameraData.renderType == CameraRenderType.Overlay)
                return;
            
            // 获取 Renderer
            ScriptableRenderer renderer = null;
            if (baseAdditionalCameraData != null)
                renderer = baseAdditionalCameraData.scriptableRenderer;
            if (renderer == null || baseCamera.cameraType == CameraType.SceneView)
                renderer = asset.scriptableRenderer;
            
            // 检查是否支持摄像机堆叠
            List<Camera> cameraStack = null;
            bool supportsCameraStacking = renderer != null && renderer.SupportsCameraStackingType(CameraRenderType.Base);
            if (supportsCameraStacking)
            {
                if (baseAdditionalCameraData != null)
                    cameraStack = baseAdditionalCameraData.cameraStack;
            }

        }
        
        
        // RenderCamera
        static void RenderSingleCamera(ScriptableRenderContext context, MotoyincLabCameraData cameraData)
        {
            
        }
        
    }
}