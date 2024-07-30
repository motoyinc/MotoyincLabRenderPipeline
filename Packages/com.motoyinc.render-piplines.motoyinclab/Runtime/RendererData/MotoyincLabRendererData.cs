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
            ReloadAllNullProperties();
            return new MotoyincLabRenderer(this);
        }
        
    }
}
