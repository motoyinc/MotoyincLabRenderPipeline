﻿using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;
namespace UnityEngine.Rendering.MotoyincLab
{
    public class MainLightShadowCasterPass: ScriptableRenderPass
    {
        private static class MainLightShadowConstantBuffer
        {
            public static readonly int _MainLightShadowmapID = Shader.PropertyToID(k_MainLightShadowMapTextureName);
        }
        private const string k_MainLightShadowMapTextureName = "_MainLightShadowmapTexture";
        private const int k_MaxCascades = 4;                // 最大联级数量
        
        internal RTHandle m_MainLightShadowmapTexture;      // 阴影贴图
        private int renderTargetWidth;                      // 阴影贴图宽度
        private int renderTargetHeight;                     // 阴影贴图高度
        private int shadowResolution;                       // 单个联级大小
        private Vector3 m_ratios;                           // 联级阴影比例
        private int m_ShadowCasterCascadesCount;            // 联级阴影数量
        
        private Matrix4x4[] m_MainLightShadowMatrices;      // 投影矩阵
        private ShadowSliceData[] m_CascadeSlices;          // 综合Shadow数据
        
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
                    ShadowUtils.DrawShadowCascadesRT(cmd, ref  m_CascadeSlices[i], ref m_PassData.shadowRendererLists[i]);
                }
                
                
                // -------------收集阴影信息-------------
                // 向GPU发送阴影数据
                
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
            
            for (int i = 0; i < m_ShadowCasterCascadesCount; i++)
            {
                var shadowSettings = new ShadowDrawingSettings(cullingResults, shadowLightIndex);

                cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                    shadowLightIndex,
                    i,
                    m_ShadowCasterCascadesCount,
                    m_ratios,
                    shadowResolution,
                    0f,
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
            }
        }
        
    }
}