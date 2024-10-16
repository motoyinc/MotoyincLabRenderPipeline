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
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            Render(context, new List<Camera>(cameras));
        }
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            int cameraCount = cameras.Count;
            
            // 进行摄象机排序
            SortCameras(cameras);
            
            // 摄像机循环
            using var profScope = new ProfilingScope(ProfilingSampler.Get(MLRPProfileId.MotoyincLabRenderTotal));
            using (new ContextRenderingScope(context, cameras))
            {
                for (int i = 0; i < cameras.Count; ++i)
                {
                    var camera = cameras[i];
                    
                    // 打开动态分辨率
                    RTHandles.SetHardwareDynamicResolutionState(true);
                    
                    // 判断摄像机是否存在 如果不存在直接抛出异常
                    if (camera == null)
                        throw new ArgumentNullException("camera");

                    // 判断摄象机类型
                    if (isGameCamera(camera))
                    {
                        using (new CameraRenderingScope(context, camera))
                        {
                            RenderCameraStack(context, camera);
                        }
                    }
                    else
                    {
                        using (new CameraRenderingScope(context, camera))
                        {
                            RenderSingleCameraInternal(context,camera);
                        }
                    }
                    
                }
            }
            
        }

        public static bool isGameCamera(Camera camera)
        {
            return camera.cameraType == CameraType.Game || camera.cameraType == CameraType.VR;
        }
        
        Comparison<Camera> cameraComparison = (camera1, camera2) => { return (int)camera1.depth - (int)camera2.depth; };
        void SortCameras(List<Camera> cameras)
        {
            if (cameras.Count > 1)
                cameras.Sort(cameraComparison);
        }

    }
}