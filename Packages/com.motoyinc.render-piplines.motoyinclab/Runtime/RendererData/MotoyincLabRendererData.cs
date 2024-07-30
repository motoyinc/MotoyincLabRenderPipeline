#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using ShaderKeywordFilter = UnityEditor.ShaderKeywordFilter;
#endif
using System;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.MotoyincLab
{
    [Serializable, ReloadGroup, ExcludeFromPreset]
    public partial class MotoyincLabRendererData : ScriptableRendererData
    {
        public PostProcessData postProcessData = null;
        
        protected override ScriptableRenderer Create()
        {
            // Renderer并不存在RenderData里，虽然是由RenderData创建的
            ReloadAllNullProperties();
            return new MotoyincLabRenderer(this);
        }
        
    }
}
