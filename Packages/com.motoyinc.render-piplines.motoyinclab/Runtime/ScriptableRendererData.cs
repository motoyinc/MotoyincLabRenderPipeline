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
    public class ScriptableRendererData : ScriptableObject
    {
        [SerializeField] bool m_UseNativeRenderPass = false;
    }

}
