namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class ScriptableRenderer
    {
        /// ///////////////////////////////////////////////////////////////////// /// 
        ///                        为Pass 设置渲染目标                              ///
        /// ///////////////////////////////////////////////////////////////////// ///  
        // 设置渲染目标 SetRenderTarget
        void SetRenderPassAttachments(CommandBuffer cmd, ScriptableRenderPass renderPass, MotoyincLabCameraData cameraData)
        {
            Camera camera = cameraData.camera;
            
            uint validColorBuffersCount = RenderingUtils.GetValidColorBufferCount(renderPass.colorAttachmentHandles);
            if (validColorBuffersCount == 0)
                return;
            
            if (RenderingUtils.IsMRT(renderPass.colorAttachmentHandles))
            {
                // MRT Pass
                Debug.LogError("目前不支持MRT渲染");
            }
            else
            {
                // 获取RT
                var passColorAttachment = m_CameraColorTarget;
                var passDepthAttachment = m_CameraDepthTarget;
                if (renderPass.overrideCameraTarget) {
                    // 如果Pass 没有启用覆盖RT，则使用默认RT进行渲染
                    passColorAttachment = renderPass.colorAttachmentHandle;
                    passDepthAttachment = renderPass.depthAttachmentHandle;
                }
                
                // 获取清理属性和清理颜色
                ClearFlag clearFlag;
                Color clearColor;
                GetClearFlagAndClearColor(out clearFlag, out clearColor, passColorAttachment, passDepthAttachment, cameraData, renderPass);
                
                // 判断RT是否发生变化，然后执行SetRenderTarget
                if (IsActiveAttachmentChanged(renderPass, passColorAttachment, passDepthAttachment, clearFlag))
                    SetRenderTarget(cmd, passColorAttachment, passDepthAttachment, clearFlag, clearColor, renderPass.colorStoreActions[0], renderPass.depthStoreAction);
            }
        }
        
        
        /// ///////////////////////////////////////////////////////////////////// /// 
        ///                      设置渲染目标的一些常用方法                           ///
        /// ///////////////////////////////////////////////////////////////////// ///  
        
        // 获取Pass 中的清理标志ClearFlag 和 清理颜色CleraColor
        void GetClearFlagAndClearColor(out ClearFlag clearFlag, out Color clearColor,RTHandle passColorAttachment, RTHandle passDepthAttachment, MotoyincLabCameraData cameraData, ScriptableRenderPass renderPass) 
        {
            // 该方法的主要作用是
            //      在首次渲染默认RT时：
            //          如果是主相机清理默认相机RT的内容，如果是Overlay就保留主相机的渲染RT内容
            //      在其他情况下：
            //          使用Pass内设置的RT内容
            
            ClearFlag cameraClearFlag = GetCameraClearFlag(cameraData);
            clearFlag = ClearFlag.None;
            
            // ////【颜色RT处理】//// //
            if (passColorAttachment.nameID == m_CameraColorTarget.nameID && m_FirstTimeCameraColorTargetIsBound)
            {
                m_FirstTimeCameraColorTargetIsBound = false; 
                clearColor = cameraData.backgroundColor;
                clearFlag |= (cameraClearFlag & ClearFlag.Color);
                
                if (SystemInfo.usesLoadStoreActions && new RenderTargetIdentifier(passColorAttachment.nameID, 0, depthSlice: 0) != BuiltinRenderTextureType.CameraTarget)
                    clearFlag |= renderPass.clearFlag;
                
                if (m_FirstTimeCameraDepthTargetIsBound)
                {
                    m_FirstTimeCameraDepthTargetIsBound = false;
                    clearFlag |= (cameraClearFlag & ClearFlag.DepthStencil); 
                }
            }
            else 
            {
                clearFlag |= (renderPass.clearFlag & ClearFlag.Color);
                clearColor = renderPass.clearColor; 
            }
            
            // ////【深度RT的处理】//// //
            if (new RenderTargetIdentifier(m_CameraDepthTarget.nameID, 0, depthSlice: 0) != BuiltinRenderTextureType.CameraTarget 
                && (passDepthAttachment.nameID == m_CameraDepthTarget.nameID || passColorAttachment.nameID == m_CameraDepthTarget.nameID) 
                && m_FirstTimeCameraDepthTargetIsBound)
            {
                m_FirstTimeCameraDepthTargetIsBound = false;
                clearFlag |= (cameraClearFlag & ClearFlag.DepthStencil); 
            }
            else
                clearFlag |= (renderPass.clearFlag & ClearFlag.DepthStencil);
            
#if UNITY_EDITOR
            if (CoreUtils.IsSceneFilteringEnabled() && cameraData.camera.sceneViewFilterMode == Camera.SceneViewFilterMode.ShowFiltered)
            {
                clearColor.a = 0;
                clearFlag &= ~ClearFlag.Depth;
            }
#endif
        }
        
        
        // 获取相机清除标志 Camera ClearFlag
        protected static ClearFlag GetCameraClearFlag(MotoyincLabCameraData cameraData)
        {
            var cameraClearFlags = cameraData.camera.clearFlags;

            if (cameraData.renderType == CameraRenderType.Overlay)
                return (cameraData.clearDepth) ? ClearFlag.DepthStencil : ClearFlag.None;
            
            if ((cameraClearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null) || cameraClearFlags == CameraClearFlags.Nothing)
            {
                // 抗锯齿没有开的情况下，不清理颜色（只是为了省性能）
                if (cameraData.cameraTargetDescriptor.msaaSamples > 1) 
                {
                    cameraData.camera.backgroundColor = Color.black;
                    return ClearFlag.DepthStencil | ClearFlag.Color; //等于 ClearFlag.All
                }else {
                    return ClearFlag.DepthStencil;
                }
            }
            return ClearFlag.All;
        }
        
        
        // 检查 PassRT 与 Renderer中的活跃RT是否有区别
        static bool IsActiveAttachmentChanged(ScriptableRenderPass renderPass, RTHandle passColorAttachment, RTHandle passDepthAttachment, ClearFlag clearFlag)
        {
            if (passColorAttachment.nameID != m_ActiveColorAttachments[0])
                return true;
            if (passDepthAttachment.nameID != m_ActiveDepthAttachment)
                return true;
            if (clearFlag != ClearFlag.None)
                return true;
            if (renderPass.colorStoreActions[0] != m_ActiveColorStoreActions[0])
                return true;
            if (renderPass.depthStoreAction != m_ActiveDepthStoreAction)
                return true;
            for (int i = 1; i < m_ActiveColorAttachments.Length; i++) {
                if (renderPass.colorAttachmentHandles[i] != m_ActiveColorAttachments[i]) {
                    return true;
                }
            }
            return false;
        }
        
        
        /// ///////////////////////////////////////////////////////////////////// /// 
        ///                      重新封装的SetRenderTarget                         ///
        /// ///////////////////////////////////////////////////////////////////// ///  
        
        internal static void SetRenderTarget(
            CommandBuffer cmd,
            RTHandle colorAttachment,
            RTHandle depthAttachment,
            ClearFlag clearFlags,
            Color clearColor,
            RenderBufferStoreAction colorStoreAction,
            RenderBufferStoreAction depthStoreAction)
        {
            // 设置活动RT
            m_ActiveColorAttachments[0] = colorAttachment;
            for (int i = 1; i < m_ActiveColorAttachments.Length; ++i)
                m_ActiveColorAttachments[i] = null;
            for (int i = 0; i < m_ActiveColorAttachments.Length; ++i)
                m_ActiveColorAttachmentIDs[i] = m_ActiveColorAttachments[i]?.nameID ?? 0;
            
            m_ActiveColorStoreActions[0] = colorStoreAction;
            m_ActiveDepthStoreAction = depthStoreAction;
            for (int i = 1; i < m_ActiveColorStoreActions.Length; ++i)
                m_ActiveColorStoreActions[i] = RenderBufferStoreAction.Store;
            m_ActiveDepthAttachment = depthAttachment;
            
            // RT清理设置
            RenderBufferLoadAction colorLoadAction = ((uint)clearFlags & (uint)ClearFlag.Color) != 0 ?
                RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;
            
            RenderBufferLoadAction depthLoadAction = ((uint)clearFlags & (uint)ClearFlag.Depth) != 0 ?
                RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;
            
            // 设置RT
            SetRenderTarget(cmd, colorAttachment, colorLoadAction, colorStoreAction,
                    depthAttachment, depthLoadAction, depthStoreAction, clearFlags, clearColor);
        }
        
        static void SetRenderTarget(CommandBuffer cmd,
            RTHandle colorAttachment,
            RenderBufferLoadAction colorLoadAction,
            RenderBufferStoreAction colorStoreAction,
            RTHandle depthAttachment,
            RenderBufferLoadAction depthLoadAction,
            RenderBufferStoreAction depthStoreAction,
            ClearFlag clearFlags,
            Color clearColor)
        {
            // 检查深度RT是否为默认值
            if (depthAttachment.nameID == BuiltinRenderTextureType.CameraTarget)
                CoreUtils.SetRenderTarget(cmd, colorAttachment, colorLoadAction, colorStoreAction,
                    colorAttachment, depthLoadAction, depthStoreAction, clearFlags, clearColor);
            else
                CoreUtils.SetRenderTarget(cmd, colorAttachment, colorLoadAction, colorStoreAction,
                    depthAttachment, depthLoadAction, depthStoreAction, clearFlags, clearColor);
        }

    }
}