using System;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using ShaderKeywordFilter = UnityEditor.ShaderKeywordFilter;
#endif

namespace UnityEngine.Rendering.MotoyincLab
{
    public abstract class ScriptableRendererData : ScriptableObject
    {
        // 用于判断当前 RendererData 是否过期
        internal bool isInvalidated { get; set; }
        
        [SerializeField] bool m_UseNativeRenderPass = false;

        protected abstract ScriptableRenderer Create();
        
        internal ScriptableRenderer InternalCreateRenderer()
        {
            isInvalidated = false;
            return Create();
        }

        public new void SetDirty()
        {
            isInvalidated = true;
        }
        
    }
}
