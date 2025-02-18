﻿using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.MotoyincLab
{
    public sealed partial class MotoyincLabRenderer
    {
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 检查PassList，并清空List
            ClearRenderPassList();
            var cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            var camera = cameraData.camera;
            
            // 配置默认RT
            ConfigureCameraTarget(k_CameraTarget, k_CameraTarget);
            
            // 主光阴影Pass
            var isEnableShadowsPass = m_mainLightShadowCasterPass.Setup(context,ref renderingData);
            if (isEnableShadowsPass)
                renderPassList.Add(m_mainLightShadowCasterPass);
            
            // 不透明Pass
            var isEnableOpaqueRenderPass = m_opaqueRenderPass.Setup(context,ref renderingData);
            if (isEnableOpaqueRenderPass)
                renderPassList.Add(m_opaqueRenderPass);
            
            // 天空球Pass
            var isEnableSkyboxRenderPass = m_skyboxRenderPass.Setup(context,ref renderingData);
            if (isEnableSkyboxRenderPass)
                renderPassList.Add(m_skyboxRenderPass);
            
            // 半透明Pass
            var isEnableTransparentRenderPass = m_transparentRenderPass.Setup(context,ref renderingData);
            if (isEnableTransparentRenderPass)
                renderPassList.Add(m_transparentRenderPass);
            
        }


        public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferHelpers.GetUnsafeCommandBuffer(renderingData.commandBuffer);
            MotoyincLabCameraData cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            MotoyincLabRenderingData motoyincLabRenderingData = renderingData.frameData.Get<MotoyincLabRenderingData>();
            MotoyincLabLightData lightData = renderingData.frameData.Get<MotoyincLabLightData>();
            
            m_ForwardLights.SetupLights(cmd, motoyincLabRenderingData, cameraData, lightData);
        }

        public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData) 
        {
            cullingParameters.shadowDistance = cameraData.maxShadowDistance; 
        }
    }
    
}