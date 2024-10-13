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
            // 因为相机中 Overlay 和 Base 这两者属性
            // 而一般 SceneView、Preview、Reflection 相机才会进入该方法 RenderSingleCameraInternal()
            // Preview、Reflection 没有 additionalCameraData，而 SceneView 不需要使用 Overlay属性 的相机进行渲染
            if (additionalCameraData != null && additionalCameraData.renderType != CameraRenderType.Base)
            {
                Debug.LogWarning($"<b>{camera.name}: </b>当前相机类型 <b>CameraRenderType {additionalCameraData.renderType}</b> 不支持渲染，本摄像机将会跳过");
                return;
            }
            
            // ...
            
            // 创建和获取数据
            var frameData = GetRenderer(camera, additionalCameraData).frameData;
            var cameraData = CreateCameraData(frameData, camera, additionalCameraData, true);
            
            // 数据创建于处理
            // ...
            
            // 进入渲染摄像机
            RenderSingleCamera(context, cameraData);
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
            var renderer = GetRenderer(baseCamera, baseAdditionalCameraData);
            
            // 检查是否支持摄像机堆叠
            List<Camera> cameraStack = null;
            bool supportsCameraStacking = renderer != null && renderer.SupportsCameraStackingType(CameraRenderType.Base);
            if (supportsCameraStacking)
            {
                if (baseAdditionalCameraData != null)
                    cameraStack = baseAdditionalCameraData.cameraStack;
            }

            
            // ...
            // 注：堆叠相机由于处理的摄像机类型会不一样，因此代码结构不会像下面这么简单
            //    目前就先按照最简单的结构写
            
            // 创建和获取数据
            var frameData = GetRenderer(baseCamera, baseAdditionalCameraData).frameData;
            var cameraData = CreateCameraData(frameData, baseCamera, baseAdditionalCameraData, true);
            
            // 进入渲染摄像机
            RenderSingleCamera(context, cameraData);
        }
        
    }
}