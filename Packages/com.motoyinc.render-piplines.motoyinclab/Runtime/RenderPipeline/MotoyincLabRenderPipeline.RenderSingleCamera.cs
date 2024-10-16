
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
            // 将渲染器对象保存在抽象类的静态属性里，方便用户直接从Class里获取而不是从instance里获取
            ScriptableRenderer.current = renderer;
            
            CommandBuffer cmd = CommandBufferPool.Get();
            CommandBuffer cmdScope = cmd;
            
            
            // 使用帧数据
            using ContextContainer frameData = renderer.frameData;
            
            // 创建帧数据
            var data = frameData.Create<MotoyincLabRenderingData>();
            
            // 整理帧数据
            CreateRenderingData(frameData, asset, cmd, false, cameraData.renderer);
            RenderingData legacyRenderingData = new RenderingData(frameData);
            
            // 渲染操作
            cmd.ClearRenderTarget(true, true, Color.black);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            // 渲染器
            renderer.Setup(context, ref legacyRenderingData);
            renderer.Execute(context, ref legacyRenderingData);
            
            context.ExecuteCommandBuffer(cmd); 
            CommandBufferPool.Release(cmd);
            
            context.Submit();
            
            ScriptableRenderer.current = null;
        }
        
    }
}