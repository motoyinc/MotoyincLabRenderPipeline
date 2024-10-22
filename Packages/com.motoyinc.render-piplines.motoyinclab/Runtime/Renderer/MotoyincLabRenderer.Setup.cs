using System.Linq;

namespace UnityEngine.Rendering.MotoyincLab
{
    public sealed partial class MotoyincLabRenderer
    {
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 检查PassList，并清空List
            ClearRenderPassList();
            
            var simpleRenderPass = new SimpleRenderPass();
            renderPassList.Add(simpleRenderPass);
            
            SetupRenderPass(context, ref renderingData);
            
        }

        void SetupRenderPass(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderPassList.Count != 0)
            {
                for (int i = 0; i < renderPassList.Count; ++i)
                {
                    if (renderPassList[i] != null)
                    {
                        renderPassList[i].Setup(context, ref renderingData);
                    }
                }
            }
        }
    }
    
}