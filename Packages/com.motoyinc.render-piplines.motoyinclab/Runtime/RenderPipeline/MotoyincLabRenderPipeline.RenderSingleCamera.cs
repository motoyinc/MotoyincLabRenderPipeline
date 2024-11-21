
namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabRenderPipeline
    {
        static void RenderSingleCamera(ScriptableRenderContext context, MotoyincLabCameraData cameraData)
        {
            
            // 渲染前数据收集
            Camera camera = cameraData.camera;
            ScriptableRenderer renderer = cameraData.renderer;
            if (renderer == null)
            {
                Debug.LogWarning($"<b>{camera.name}</b> 没有找到渲染器. 本次渲染将被跳过.");
                return;
            }
            // 使用帧数据
            using ContextContainer frameData = renderer.frameData;
            
            // 获取摄像机剔除
            if (!TryGetCullingParameters(cameraData, out var cullingParameters))
                return;
            
            // 将渲染器对象保存在抽象类的静态属性里，方便用户直接从Class里获取而不是从instance里获取
            ScriptableRenderer.current = renderer;
            
            CommandBuffer cmd = CommandBufferPool.Get();
            CommandBuffer cmdScope = cmd;
            
            renderer.Clear(cameraData.renderType);

            var data = frameData.Create<MotoyincLabRenderingData>(); 
            data.cullResults = context.Cull(ref cullingParameters);
            var cameraMetadataSampler = CameraMetadataCache.GetCached(camera).sampler;
            using (new ProfilingScope(cmdScope, cameraMetadataSampler))
            {
                // 渲染操作
                cmd.ClearRenderTarget(true, true, Color.black);
                // Catlikecoding_ClearRenderTarget(context, cmd, camera);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                
                // 渲染UI编辑器的UI
#if UNITY_EDITOR
                if (cameraData.isSceneViewCamera)
                    ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
#endif
                
                RTHandles.SetReferenceSize(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);
                
                var isForwardPlus = false;
                
                // 整理帧数据
                using (new ProfilingScope(Profiling.Pipeline.initializeRenderingData))
                {
                    CreateLightData(frameData, asset, data.cullResults.visibleLights);
                    CreateShadowData(frameData, asset, isForwardPlus);
                    CreateRenderingData(frameData, asset, cmd, false, cameraData.renderer);
                }
                RenderingData legacyRenderingData = new RenderingData(frameData);
                
                // 渲染器
                using (new ProfilingScope(Profiling.Pipeline.Renderer.setup))
                {
                    renderer.Setup(context, ref legacyRenderingData);
                }
                renderer.Execute(context, ref legacyRenderingData);
            }
            
            // 释放cmd对象
            context.ExecuteCommandBuffer(cmd); 
            CommandBufferPool.Release(cmd);
            
            // 提交渲染
            using (new ProfilingScope(Profiling.Pipeline.Context.submit))
            {
                context.Submit();
            }
            ScriptableRenderer.current = null;
        }
        
        
        static bool TryGetCullingParameters(MotoyincLabCameraData cameraData, out ScriptableCullingParameters cullingParams)
        {
            // 在URP中这里还做了一些VR相关的处理，暂时没有这个需求不用了
            return cameraData.camera.TryGetCullingParameters(false, out cullingParams);
        }

        // 该方法是Catlikecoding版本的ClearRenderTarget方法
        static void Catlikecoding_ClearRenderTarget(ScriptableRenderContext context ,CommandBuffer cmd, Camera camera)
        {
            context.SetupCameraProperties(camera);
            var depthflage = false;
            var colorflage = false;
            var colorFlags = Color.clear;
            if (camera.clearFlags <= CameraClearFlags.Depth)
                depthflage = true;
            if (camera.clearFlags == CameraClearFlags.Color || camera.clearFlags == CameraClearFlags.SolidColor)
            {
                colorflage = true;
                colorFlags = camera.backgroundColor.linear;
            }
            cmd.ClearRenderTarget(depthflage, colorflage, colorFlags);
        }
        
    }
}