using System;
using System.Diagnostics;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class ScriptableRenderer
    {
        public ScriptableRenderer(ScriptableRendererData rendererData)
        {
            
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}