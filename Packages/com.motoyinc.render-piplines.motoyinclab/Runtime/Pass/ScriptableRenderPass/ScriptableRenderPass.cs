using System;

namespace UnityEngine.Rendering.MotoyincLab
{
    public abstract partial class ScriptableRenderPass: IDisposable
    {
        private string m_PassName;
        protected internal string passName{ 
            get => m_PassName; 
            set => m_PassName = value;
        }
        
        public abstract bool Setup(ScriptableRenderContext context, ref RenderingData renderingData);
        
        public abstract void Execute(ScriptableRenderContext context, ref RenderingData renderingData);

        public virtual bool FinishExecute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            return false;
        }

        public virtual void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) { }
        
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}