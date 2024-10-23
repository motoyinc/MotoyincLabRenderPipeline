using System;

namespace UnityEngine.Rendering.MotoyincLab
{
    public abstract partial class ScriptableRenderPass: IDisposable
    {
        public string passName = "";
        public CommandBuffer cmd;
        public MotoyincLabCameraData cameraData;
        public Camera camera;
        public CullingResults cullingResults;
        public abstract void Setup(ScriptableRenderContext context, ref RenderingData renderingData);
        public abstract void Execute(ScriptableRenderContext context, ref RenderingData renderingData);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}