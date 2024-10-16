namespace UnityEngine.Rendering.MotoyincLab
{
    public abstract partial class ScriptableRenderer
    {
        public abstract void Setup(ScriptableRenderContext context, ref RenderingData renderingData);

        public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData =renderingData.frameData.Get<MotoyincLabCameraData>();
            Camera camera = cameraData.camera;
            
            camera.TryGetCullingParameters(out var cullingParameters);
            var cullingResults = context.Cull(ref cullingParameters);
            context.SetupCameraProperties(camera);
            ShaderTagId shaderTagId = new ShaderTagId("ExampleLightModeTag");
            var sortingSettings = new SortingSettings(camera);
            DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
            FilteringSettings filteringSettings = FilteringSettings.defaultValue;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                context.DrawSkybox(camera);
            }
        }
    }
}