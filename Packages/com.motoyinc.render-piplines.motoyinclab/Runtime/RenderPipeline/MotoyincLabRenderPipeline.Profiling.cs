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
         internal static class Profiling
        {
            public static class Pipeline
            {
                const string k_Name = nameof(MotoyincLabRenderPipeline);
                public static readonly ProfilingSampler initializeRenderingData = new ProfilingSampler($"{k_Name}.{nameof(CreateRenderingData)}");
                
                public static class Renderer
                {
                    const string k_Name = nameof(ScriptableRenderer);
                    public static readonly ProfilingSampler setup = new ProfilingSampler($"{k_Name}.{nameof(ScriptableRenderer.Setup)}");
                    public static readonly ProfilingSampler setupCullingParameters = new ProfilingSampler($"{k_Name}.{nameof(ScriptableRenderer.SetupCullingParameters)}");
                };
                
                public static class Context
                {
                    const string k_Name = nameof(ScriptableRenderContext);
                    public static readonly ProfilingSampler submit = new ProfilingSampler($"{k_Name}.{nameof(ScriptableRenderContext.Submit)}");
                };
            };
        }
        
        
        // 渲染分析器
        //      分析范围：Render方法执行到接受
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
        
        // 摄像机分析器
        //      分析范围：摄像机循环开始到结束
        readonly struct CameraRenderingScope : IDisposable
        {
            static readonly ProfilingSampler beginCameraRenderingSampler = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(BeginCameraRendering)}");
            static readonly ProfilingSampler endCameraRenderingSampler = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(EndCameraRendering)}");

            private readonly ScriptableRenderContext m_Context;
            private readonly Camera m_Camera;

            public CameraRenderingScope(ScriptableRenderContext context, Camera camera)
            {
                using (new ProfilingScope(beginCameraRenderingSampler))
                {
                    m_Context = context;
                    m_Camera = camera;

                    BeginCameraRendering(context, camera);
                }
            }

            public void Dispose()
            {
                using (new ProfilingScope(endCameraRenderingSampler))
                {
                    EndCameraRendering(m_Context, m_Camera);
                }
            }
        }
        
        
        // 分析器缓存，
        //      将对象的hash值作为键 对象name值作为值 保存在 k_NoAllocEntry 字典里，减少调用 camera.name 的开销
        //      下次再调用时，如果字典里有就直接从字典里取name，
        //      如果没有，则将将对象的 hash 值作为键，对象 name 值作为值，保存在 k_NoAllocEntry 字典里。
        internal static class CameraMetadataCache
        {
            public class CameraMetadataCacheEntry
            {
                public string name;
                public ProfilingSampler sampler;
            }

            static Dictionary<int, CameraMetadataCacheEntry> s_MetadataCache = new();

            static readonly CameraMetadataCacheEntry k_NoAllocEntry = new() { name = "Unknown", sampler = new ProfilingSampler("Unknown") };

            public static CameraMetadataCacheEntry GetCached(Camera camera)
            {
#if UNIVERSAL_PROFILING_NO_ALLOC
                return k_NoAllocEntry;
#else
                int cameraId = camera.GetHashCode();
                if (!s_MetadataCache.TryGetValue(cameraId, out CameraMetadataCacheEntry result))
                {
                    string cameraName = camera.name; // Warning: camera.name allocates
                    result = new CameraMetadataCacheEntry
                    {
                        name = cameraName,
                        sampler = new ProfilingSampler(
                            $"{nameof(MotoyincLabRenderPipeline)}.{nameof(RenderSingleCameraInternal)}: {cameraName}")
                    };
                    s_MetadataCache.Add(cameraId, result);
                }

                return result;
#endif
            }
        }

    }
}