using System;
using System.Diagnostics;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.MotoyincLab
{
    public abstract partial class ScriptableRenderer: IDisposable
    {
        ContextContainer m_frameData = new();
        internal ContextContainer frameData => m_frameData;
        
        internal static ScriptableRenderer current = null;
        
        public ScriptableRenderer(ScriptableRendererData rendererData)
        {
            
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        
        public virtual int SupportedCameraStackingTypes()
        {
            return 0;
        }

        public bool SupportsCameraStackingType(CameraRenderType cameraRenderType)
        {
            return (SupportedCameraStackingTypes() & 1 << (int)cameraRenderType) != 0;
        }
    }
}