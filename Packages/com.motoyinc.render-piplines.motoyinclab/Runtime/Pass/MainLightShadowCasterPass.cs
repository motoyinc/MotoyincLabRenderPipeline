﻿using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;
namespace UnityEngine.Rendering.MotoyincLab
{
    public class MainLightShadowCasterPass: ScriptableRenderPass
    {
        public static class MainLightShadowConstantBuffer
        {
            public static readonly int _MainLightShadowmapID = Shader.PropertyToID(k_MainLightShadowMapTextureName);
            public static readonly int _WorldToShadow = Shader.PropertyToID("_MainLightWorldToShadow");
            public static readonly int _ShadowParams = Shader.PropertyToID("_MainLightShadowParams");
            public static readonly int _ShadowmapSize = Shader.PropertyToID("_MainLightShadowmapSize");
            
            public static readonly int _CascadeShadowSplitSpheres0 = Shader.PropertyToID("_CascadeShadowSplitSpheres0");
            public static readonly int _CascadeShadowSplitSpheres1 = Shader.PropertyToID("_CascadeShadowSplitSpheres1");
            public static readonly int _CascadeShadowSplitSpheres2 = Shader.PropertyToID("_CascadeShadowSplitSpheres2");
            public static readonly int _CascadeShadowSplitSpheres3 = Shader.PropertyToID("_CascadeShadowSplitSpheres3");
            public static readonly int _CascadeShadowSplitSphereRadii = Shader.PropertyToID("_CascadeShadowSplitSphereRadii");
            public static readonly int _MainLightPCSSData0 = Shader.PropertyToID("_MainLightPCSSData0");
            public static readonly int _MainLightPCSSData1 = Shader.PropertyToID("_MainLightPCSSData1");
            public static readonly int _AtlasOffset = Shader.PropertyToID("_AtlasOffset");
        }
        private const string k_MainLightShadowMapTextureName = "_MainLightShadowmapTexture";
        private const int k_MaxCascades = 4;                // 最大联级数量
        
        internal RTHandle m_MainLightShadowmapTexture;      // 阴影贴图
        private int renderTargetWidth;                      // 阴影贴图宽度
        private int renderTargetHeight;                     // 阴影贴图高度
        private int shadowResolution;                       // 单个联级大小
        
        private int m_ShadowCasterCascadesCount;            // 联级阴影数量
        private float m_CascadeBorder;
        private float m_MaxShadowDistanceSq;
        
        private Vector3 m_ratios;                                               // 联级阴影分割比例
        private Vector4[] m_CascadeSplitDistances = new Vector4[k_MaxCascades]; // 联级阴影裁切球      .xyz:起始点  .w:距离
        private Matrix4x4[] m_MainLightShadowMatrices;      // 投影矩阵
        private ShadowSliceData[] m_CascadeSlices;          // 综合Shadow数据
        
        // private Matrix4x4[] m_DeviceProjectionMatrix = new Matrix4x4[k_MaxCascades];
        // private Vector4[] m_DeviceProjection = new Vector4[k_MaxCascades];
        private Vector4[] m_AtlasOffset;
        
        //Pass管线数据
        private PassData m_PassData;                        
        private class PassData
        {
            internal MotoyincLabRenderingData renderingData;
            internal MotoyincLabCameraData cameraData;
            internal MotoyincLabLightData lightData;
            internal MotoyincLabShadowData shadowData;
            internal MainLightShadowCasterPass pass;
            
            internal RendererList[] shadowRendererLists = new RendererList[k_MaxCascades];
        }
        
        private void InitPassData(ref PassData passData, MotoyincLabRenderingData renderingData, MotoyincLabCameraData cameraData, MotoyincLabLightData lightData, MotoyincLabShadowData shadowData)
        {
            passData.pass = this;
            passData.renderingData = renderingData;
            passData.cameraData = cameraData;
            passData.lightData = lightData;
            passData.shadowData = shadowData;
        }
        
        public MainLightShadowCasterPass()
        {
            passName = "MainLightShadowCasterPass";
            
            m_PassData = new PassData();
            m_MainLightShadowMatrices = new Matrix4x4[k_MaxCascades + 1];
            m_CascadeSlices = new ShadowSliceData[k_MaxCascades];
            m_AtlasOffset = new Vector4[k_MaxCascades];
        }
        
        public override void Dispose()
        {
            m_MainLightShadowmapTexture?.Release();
            m_MainLightShadowmapTexture = null;
        }
        
        public override bool Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var shadowData =renderingData.frameData.Get<MotoyincLabShadowData>();
            var lightData = renderingData.frameData.Get<MotoyincLabLightData>();
            var cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            var motoyincLabRenderingData = renderingData.frameData.Get<MotoyincLabRenderingData>();
            return Setup(motoyincLabRenderingData, cameraData, lightData, shadowData);
            
        }

        public bool Setup(MotoyincLabRenderingData renderingData, MotoyincLabCameraData cameraData,
            MotoyincLabLightData lightData, MotoyincLabShadowData shadowData)
        {
            if(!shadowData.mainLightShadowsEnabled)
                return false;
            
#if UNITY_EDITOR
            if (CoreUtils.IsSceneLightingDisabled(cameraData.camera))
                return false;
#endif
            
            if(!shadowData.supportsMainLightShadows)
                return false;
            
            int shadowLightIndex = lightData.mainLightIndex;
            if(shadowLightIndex == -1)
                return false;
            
            VisibleLight shadowLight = lightData.visibleLights[shadowLightIndex];
            Light light = shadowLight.light;
            if (light.shadows == LightShadows.None)
                return false;
            if (shadowLight.lightType != LightType.Directional)
                Debug.LogWarning("请将直射光作为主光源.");
            
            // 查询灯光包围盒中是否有物体
            if (!renderingData.cullResults.GetShadowCasterBounds(shadowLightIndex, out Bounds shadowBounds))
                return false;

            m_ShadowCasterCascadesCount = shadowData.mainLightShadowCascadesCount;
            renderTargetWidth = shadowData.mainLightRenderTargetWidth;
            renderTargetHeight = shadowData.mainLightRenderTargetHeight;
            shadowResolution = shadowData.mainLightShadowResolution;
            m_ratios = shadowData.mainLightShadowCascadesSplit;
            var shadowFilterMode = ShadowUtils.forceShadowPointSampling ? FilterMode.Point : FilterMode.Bilinear;
            
            // 检查RT是否释放
            if (m_MainLightShadowmapTexture != null)
            {
                m_MainLightShadowmapTexture?.Release();
                m_MainLightShadowmapTexture = null;
            }
            // 初始化阴影RT
            m_MainLightShadowmapTexture = RTHandles.Alloc(
                renderTargetWidth, renderTargetHeight,       // 固定尺寸
                depthBufferBits: DepthBits.Depth32, // 深度缓冲格式
                colorFormat: GraphicsFormat.None,  // 不需要颜色缓冲
                filterMode: shadowFilterMode,      // 防止模糊边缘
                wrapMode: TextureWrapMode.Clamp,   // 阴影贴图的包装模式
                name: k_MainLightShadowMapTextureName
            );
            // ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_MainLightShadowmapTexture, renderTargetWidth, renderTargetHeight, k_ShadowmapBufferBits, name: k_MainLightShadowMapTextureName);
            m_CascadeBorder = shadowData.mainLightShadowCascadeBorder;
            m_MaxShadowDistanceSq = cameraData.maxShadowDistance * cameraData.maxShadowDistance;
            // 启用Pass
            return true;
        }
        
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureTarget(m_MainLightShadowmapTexture);
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var motoyincLabRenderingData = renderingData.frameData.Get<MotoyincLabRenderingData>();
            var cameraData = renderingData.frameData.Get<MotoyincLabCameraData>();
            var lightData = renderingData.frameData.Get<MotoyincLabLightData>();
            var shadowData = renderingData.frameData.Get<MotoyincLabShadowData>();
            
            int shadowLightIndex = lightData.mainLightIndex;
            if (shadowLightIndex == -1)
                return;
            VisibleLight shadowLight = lightData.visibleLights[shadowLightIndex];
            
            InitPassData(ref m_PassData, motoyincLabRenderingData, cameraData, lightData, shadowData);
            
            
            var cmd = renderingData.commandBuffer;
            using var profScope = new ProfilingScope(ProfilingSampler.Get(MLRPProfileId.MainLightShadow));
            {
                
                cmd.BeginSample(passName);
                
                // -------------渲染阴影RT Pass-------------
                // 收集RT数据
                GetShadowsRendererListAndSetting(ref m_PassData, context);
                
                // 渲染阴影RT
                for (int i = 0; i < m_ShadowCasterCascadesCount; i++)
                {
                    // shadowBias 阴影偏移
                    // X：Depth Bias     Y：normal Bias    Z: light Type      W：闲置
                    Vector4 shadowBias = ShadowUtils.GetShadowBias(ref shadowLight, shadowLightIndex, shadowData, m_CascadeSlices[i].projectionMatrix, m_CascadeSlices[i].resolution);
                    ShadowUtils.SetupShadowCasterConstantBuffer(cmd, ref shadowLight, shadowBias);
                    ShadowUtils.DrawShadowCascadesRT(cmd, ref  m_CascadeSlices[i], ref m_PassData.shadowRendererLists[i]);
                }
                
                
                // -------------收集阴影信息-------------
                // 向GPU发送阴影数据
                SetupMainLightShadowDataConstants(cmd, ref m_PassData, shadowData);
                
                // 将阴影RT所为Tex输入
                cmd.SetGlobalTexture(MainLightShadowConstantBuffer._MainLightShadowmapID, m_MainLightShadowmapTexture.nameID);
                
                cmd.EndSample(passName);
            }
        }

        
        public override bool FinishExecute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // -------------阴影数据收集完成，恢复渲染配置-------------
            // 恢复相机的视图和投影矩阵
            //      不切换会导致视口渲染因矩阵还是灯光的投影矩阵而出错
            var cameraData = m_PassData.cameraData;
            var cmd = renderingData.commandBuffer;
            var camera = cameraData.camera;
            context.SetupCameraProperties(camera);
            
            return true;
        }


        // 收集 阴影RT渲染数据
        private void GetShadowsRendererListAndSetting(ref PassData passData, ScriptableRenderContext context)
        {
            var shadowLightIndex = passData.lightData.mainLightIndex;
            if (shadowLightIndex == -1)
                return;
            var cullingResults = passData.renderingData.cullResults;

            VisibleLight visibleLight = passData.lightData.visibleLights[shadowLightIndex];
            
            for (int i = 0; i < m_ShadowCasterCascadesCount; i++)
            {
                var shadowSettings = new ShadowDrawingSettings(cullingResults, shadowLightIndex);

                cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                    shadowLightIndex,
                    i,
                    m_ShadowCasterCascadesCount,
                    m_ratios,
                    shadowResolution,
                    visibleLight.light.shadowNearPlane,
                    out Matrix4x4 viewMatrix,
                    out Matrix4x4 projectionMatrix,
                    out ShadowSplitData splitData
                );
                
                // 创建RenderLists
                passData.shadowRendererLists[i] = context.CreateShadowRendererList(ref shadowSettings);
                
                                                                                    
                // 每个联级在RT偏移量
                // m_CascadeSlices[i].offsetX = ( (i == 0 || i == 2) ? 0 : 1 )*shadowResolution;
                m_CascadeSlices[i].offsetX =  (i % 2) * shadowResolution;           // 0、2号阴影在第一列，1、3号阴影在第二列
                m_CascadeSlices[i].offsetY = (i < 2 ? 0 : 1) * shadowResolution;    // 0、1阴影在第一行，2、3阴影在第二行
                
                m_CascadeSlices[i].resolution = shadowResolution;                   // 联级尺寸
                m_CascadeSlices[i].projectionMatrix = projectionMatrix;             // 投影矩阵
                m_CascadeSlices[i].viewMatrix = viewMatrix;                         // 灯光视角矩阵
                m_CascadeSlices[i].splitData = splitData;                           // 分割数据
                
                // 设备矩阵，会根据GPU设备不同，有可能反转Z轴
                // Matrix4x4 deviceProjectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, false);
                // m_CascadeSlices[i].deviceProjectionMatrix = deviceProjectionMatrix;
                // m_CascadeSlices[i].deviceProjection = new Vector4(deviceProjectionMatrix.m00, deviceProjectionMatrix.m11, deviceProjectionMatrix.m22, deviceProjectionMatrix.m23);

            }
        }
        
        // 收集 阴影相关数据
        void SetupMainLightShadowDataConstants(CommandBuffer cmd, ref PassData data, MotoyincLabShadowData shadowData)
        {
            var lightData = data.lightData;
            var shadowLightIndex = lightData.mainLightIndex;
            if (shadowLightIndex == -1)
                return;
            VisibleLight shadowLight = lightData.visibleLights[shadowLightIndex];
            Light light = shadowLight.light;
            
            
            // ///【阴影数据】/// //
            // 最远距离淡出
            ShadowUtils.GetScaleAndBiasForLinearDistanceFade(m_MaxShadowDistanceSq, m_CascadeBorder, out float shadowFadeScale, out float shadowFadeBias);
            
            // 软阴影支持
            int shadowQuality = 0;
            bool isSoftShadowsEnable = false;
            if (shadowData.supportsSoftShadows)
            {
                shadowQuality = shadowData.mainShadowQuality;
                isSoftShadowsEnable = ShadowUtils.SoftShadowQualityToShaderProperty(light, ref shadowQuality);
                if(isSoftShadowsEnable)
                {
                    // ShadowTexture尺寸
                    float invShadowAtlasWidth = 1.0f / renderTargetWidth;
                    float invShadowAtlasHeight = 1.0f / renderTargetHeight;
                    cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowmapSize, new Vector4(invShadowAtlasWidth,
                        invShadowAtlasHeight,
                        renderTargetWidth, renderTargetHeight));
                }
                else
                    shadowQuality = 0;
            }
            
            // x: 阴影强度[v]， y: 软阴影[x]， z：阴影淡化的范围[x]， w:阴影淡化偏移值[x]
            Vector4 shadowDataBuffer = new Vector4(shadowLight.light.shadowStrength, shadowQuality, shadowFadeScale, shadowFadeBias);
            cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowParams, shadowDataBuffer);
            
            
            
            // ///【阴影矩阵】/// //
            int cascadeCounts = m_ShadowCasterCascadesCount;
            for (int i = 0; i < cascadeCounts; ++i)
            {
                ShadowUtils.GetDirectionalLightMatrix(ref m_CascadeSlices[i], cascadeCounts, renderTargetWidth, renderTargetHeight);
                
                m_MainLightShadowMatrices[i]=m_CascadeSlices[i].shadowTransform;
                m_CascadeSplitDistances[i]=m_CascadeSlices[i].splitData.cullingSphere;
            }
            // 填充空余矩阵
            Matrix4x4 noOpShadowMatrix = Matrix4x4.zero;
            noOpShadowMatrix.m22 = (SystemInfo.usesReversedZBuffer) ? 1.0f : 0.0f;
            for (int i = cascadeCounts; i <= k_MaxCascades; ++i)
            {
                // 用最后一个层级填充空矩阵
                // m_MainLightShadowMatrices[i] = m_MainLightShadowMatrices[cascadeCounts-1];
                
                // 用零矩阵填充空矩阵
                m_MainLightShadowMatrices[i] = noOpShadowMatrix;
            }
            
            cmd.SetGlobalMatrixArray(MainLightShadowConstantBuffer._WorldToShadow, m_MainLightShadowMatrices);
            
    
            
            // ///【Caster信息】/// //
            if (m_ShadowCasterCascadesCount > 1)
            {
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres0,
                    m_CascadeSplitDistances[0]);
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres1,
                    m_CascadeSplitDistances[1]);
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres2,
                    m_CascadeSplitDistances[2]);
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres3,
                    m_CascadeSplitDistances[3]);
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSphereRadii, new Vector4(
                    m_CascadeSplitDistances[0].w * m_CascadeSplitDistances[0].w,
                    m_CascadeSplitDistances[1].w * m_CascadeSplitDistances[1].w,
                    m_CascadeSplitDistances[2].w * m_CascadeSplitDistances[2].w,
                    m_CascadeSplitDistances[3].w * m_CascadeSplitDistances[3].w));
            }
            
            // ///【其他数据】/// //
            cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, data.cameraData.worldSpaceCameraPos);
            cmd.SetKeyword(ShaderGlobalKeywords.MainLightShadows, data.shadowData.mainLightShadowCascadesCount == 1);
            cmd.SetKeyword(ShaderGlobalKeywords.MainLightShadowCascades, data.shadowData.mainLightShadowCascadesCount > 1);
            
            
            
            // /// 【PCSS】 /// //
            if (isSoftShadowsEnable && shadowQuality == 4)
            {
                Vector4[] pcssData = ShadowUtils.GetDirLightPCSSData(light);
                
                //-- [shadowmapInAtlasScale]--//
                // cascadeMapSize
                float shadowCascadeSize;
                if (m_ShadowCasterCascadesCount > 1)
                    shadowCascadeSize = renderTargetWidth/2.0f;
                else
                    shadowCascadeSize = renderTargetWidth;
                // TexelSize
                float invShadowAtlasHeight = 1.0f / renderTargetHeight;
                float invShadowAtlasWidth = 1.0f / renderTargetWidth;
                // cascadeMapSize * TexelSize
                pcssData[1].z = invShadowAtlasWidth * shadowCascadeSize;
                pcssData[1].w = invShadowAtlasHeight * shadowCascadeSize;
                
                
                //-- [shadowmapInAtlasOffset]--//
                // atlasOffset
                
                for (int i = 0; i <= cascadeCounts-1; ++i)
                {
                    m_AtlasOffset[i].x = renderTargetWidth;
                    m_AtlasOffset[i].y = renderTargetWidth;
                    m_AtlasOffset[i].w = m_CascadeSlices[i].offsetX;
                    m_AtlasOffset[i].z = m_CascadeSlices[i].offsetY;
                }
                
                
                cmd.SetGlobalVectorArray(MainLightShadowConstantBuffer._AtlasOffset, m_AtlasOffset);
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._MainLightPCSSData0, pcssData[0]);
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._MainLightPCSSData1, pcssData[1]);
                
            }
            
            
        }
        
    }
}