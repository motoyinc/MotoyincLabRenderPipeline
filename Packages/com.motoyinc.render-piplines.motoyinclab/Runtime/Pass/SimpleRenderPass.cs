namespace UnityEngine.Rendering.MotoyincLab
{
    public class SimpleRenderPass: ScriptableRenderPass
    {
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            camera = cameraData.camera;
            cmd = renderingData.commandBuffer;
            passName = "SimpleRenderPass";
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd.BeginSample(passName);
            
            // 设置RT
            context.SetupCameraProperties(camera);
            cmd.ClearRenderTarget(true, true, Color.clear);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            // 简易Render范例
            camera.TryGetCullingParameters(out var cullingParameters);
            var cullingResults = context.Cull(ref cullingParameters);
            ShaderTagId shaderTagId = new ShaderTagId("Unlit");
            var sortingSettings = new SortingSettings(camera);
            DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
            FilteringSettings filteringSettings = FilteringSettings.defaultValue;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                context.DrawSkybox(camera);
            }
            
            cmd.EndSample(passName);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
    }
}