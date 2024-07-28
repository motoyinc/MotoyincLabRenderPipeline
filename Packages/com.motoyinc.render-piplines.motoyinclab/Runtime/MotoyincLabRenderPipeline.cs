using System;
using Unity.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
// using UnityEditor.Rendering.MotoyincLab;
#endif
using UnityEngine.Scripting.APIUpdating;
using Lightmapping = UnityEngine.Experimental.GlobalIllumination.Lightmapping;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Profiling;
using static UnityEngine.Camera;

namespace UnityEngine.Rendering.MotoyincLab
{
    
    public class MotoyincLabRenderPipeline : RenderPipeline
    {
        private readonly MotoyincLabRenderPipelineAsset pipelineAsset;
        public MotoyincLabRenderPipeline(MotoyincLabRenderPipelineAsset asset)
        {
            pipelineAsset = asset;
        }
        
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            
        }

        static void RenderSingleCamera(ScriptableRenderContext context, MotoyincLabCameraData cameraData)
        {
        }

        static void RenderCameraStack(ScriptableRenderContext context, Camera baseCamera)
        {
        }
    }
}

