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
                        RenderCameraStack(context, camera);
                    }
                    else
                    {
                        RenderSingleCameraInternal(context,camera);
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

        // 性能分析用结构体
        readonly struct ContextRenderingScope : IDisposable
        {
            private static readonly ProfilingSampler beginContextRenderingSampler = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(BeginContextRendering)}");
            private static readonly ProfilingSampler endContextRenderingSampler = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(EndContextRendering)}");
            private readonly ScriptableRenderContext m_Context;
            private readonly List<Camera> m_Cameras;
            
            public ContextRenderingScope(ScriptableRenderContext context, List<Camera> cameras)
            {
                m_Context = context;
                m_Cameras = cameras;
                using (new ProfilingScope(beginContextRenderingSampler))
                {
                    BeginContextRendering(m_Context, m_Cameras);
                }
            }
            
            public void Dispose()
            {
                using (new ProfilingScope(endContextRenderingSampler))
                {
                    EndContextRendering(m_Context, m_Cameras);
                }
            }
        }

        

    }
}