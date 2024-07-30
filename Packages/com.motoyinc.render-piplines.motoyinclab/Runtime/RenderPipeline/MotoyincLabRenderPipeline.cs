using System;
using Unity.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering.MotoyincLab;
#endif
using UnityEngine.Scripting.APIUpdating;
using Lightmapping = UnityEngine.Experimental.GlobalIllumination.Lightmapping;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Profiling;
using static UnityEngine.Camera;

namespace UnityEngine.Rendering.MotoyincLab
{
    
    public partial class MotoyincLabRenderPipeline : RenderPipeline
    {
        // 管线数据
        private readonly MotoyincLabRenderPipelineAsset pipelineAsset;
        private MotoyincLabRenderPipelineGlobalSettings m_GlobalSettings;
        
        // 内置资产
        internal MotoyincLabRenderPipelineRuntimeTextures runtimeTextures { get; private set; }
        
        public static MotoyincLabRenderPipelineAsset asset
        {
            get => GraphicsSettings.currentRenderPipeline as MotoyincLabRenderPipelineAsset;
        }
        
        public MotoyincLabRenderPipeline(MotoyincLabRenderPipelineAsset asset)
        {
            pipelineAsset = asset;
            m_GlobalSettings = MotoyincLabRenderPipelineGlobalSettings.instance;
            
            // 载入管线内置资源
            // TODO: 管线资源无法正常加载
            // runtimeTextures = GraphicsSettings.GetRenderPipelineSettings<MotoyincLabRenderPipelineRuntimeTextures>();
            // var shader = GraphicsSettings.GetRenderPipelineSettings<MotoyincLabRenderPipelineRuntimeShaders>();
            
            // Blitter.Initialize(shader.coreBlitPS, shader.coreBlitColorAndDepthPS);
            RTHandles.Initialize(Screen.width, Screen.height);
            
            // 支持渲染特性
            SupportedRenderingFeatures.active.supportsHDR = pipelineAsset.supportsHDR;
            SupportedRenderingFeatures.active.rendersUIOverlay = true;
            
            // SRP合批功能
            GraphicsSettings.useScriptableRenderPipelineBatching = pipelineAsset.useSRPBatcher;

        }
    }
}

