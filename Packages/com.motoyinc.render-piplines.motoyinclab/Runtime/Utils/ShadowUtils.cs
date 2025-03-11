using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;
namespace UnityEngine.Rendering.MotoyincLab
{
    
    /// <summary>
    /// GPU阴影数据结构体
    /// </summary>
    public struct ShadowSliceData
    {
        public Matrix4x4 viewMatrix;
        public Matrix4x4 projectionMatrix;
        public Matrix4x4 shadowTransform;
        public int offsetX;
        public int offsetY;
        public int resolution;
        public ShadowSplitData splitData;
        
        // public Matrix4x4 deviceProjectionMatrix;
        // public Vector4 deviceProjection;
            
        public void Clear()
        {
            viewMatrix = Matrix4x4.identity;
            projectionMatrix = Matrix4x4.identity;
            shadowTransform = Matrix4x4.identity;
            offsetX = offsetY = 0;
            resolution = 1024;
            
            // deviceProjectionMatrix = Matrix4x4.identity;
            // deviceProjection = Vector4.zero;
        }
    }
    
    public static class ShadowUtils
    {
        /// <summary>
        /// Metal Api平台兼容属性
        ///     <returns>"Ture"为 Matal 平台，"False"为其他类型的平台 </returns>
        /// </summary>
        /// <remarks>
        /// Meta平台平均建议使用 FilterMode.Point，
        /// 其他平台 建议使用 FilterMode.Bilinear
        /// </remarks>
        internal static bool forceShadowPointSampling => m_ForceShadowPointSampling;
        internal static readonly bool m_ForceShadowPointSampling;
        static ShadowUtils()
        {
            m_ForceShadowPointSampling = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal &&
                                         GraphicsSettings.HasShaderDefine(Graphics.activeTier, BuiltinShaderDefine.UNITY_METAL_SHADOWS_USE_POINT_FILTERING);
        }

        internal static void DrawShadowCascadesRT(CommandBuffer cmd, ref ShadowSliceData shadowSliceData, ref RendererList rendererList)
        {
            // 设置将联级渲染到RT上的位置
            cmd.SetViewport(new Rect(shadowSliceData.offsetX, shadowSliceData.offsetY, shadowSliceData.resolution, shadowSliceData.resolution));
            // 设置视角矩阵和投影矩阵
            cmd.SetViewProjectionMatrices(shadowSliceData.viewMatrix, shadowSliceData.projectionMatrix);
            cmd.SetGlobalDepthBias(1.0f, 2.5f); 
            //提交渲染
            if(rendererList.isValid)
                cmd.DrawRendererList(rendererList);
            cmd.SetGlobalDepthBias(0.0f, 0.0f);
        }
        
        // 获取直射光阴影矩阵
        public static void GetDirectionalLightMatrix(ref ShadowSliceData shadowSliceData, int cascadeCounts, int atlasWidth, int atlasHeight)
        {
            Matrix4x4 view = shadowSliceData.viewMatrix;
            Matrix4x4 proj = shadowSliceData.projectionMatrix;
            
            // 深度反转
            if (SystemInfo.usesReversedZBuffer)
            {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }
            Matrix4x4 worldToShadow = proj * view;
            
            // 深度范围调整 [-1,1] to [0,1]
            var textureScaleAndBias = Matrix4x4.identity;
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;
            shadowSliceData.shadowTransform = textureScaleAndBias * worldToShadow;
            
            // 联级缩放与偏移量
            if (cascadeCounts > 1)
                ApplySliceTransform(ref shadowSliceData, atlasWidth, atlasHeight);
        }
        
        // 联级缩放与偏移量
        public static void ApplySliceTransform(ref ShadowSliceData shadowSliceData, int atlasWidth, int atlasHeight)
        {
            Matrix4x4 sliceTransform = Matrix4x4.identity;
            float oneOverAtlasWidth = 1.0f / atlasWidth;
            float oneOverAtlasHeight = 1.0f / atlasHeight;
            sliceTransform.m00 = shadowSliceData.resolution * oneOverAtlasWidth;
            sliceTransform.m11 = shadowSliceData.resolution * oneOverAtlasHeight;
            sliceTransform.m03 = shadowSliceData.offsetX * oneOverAtlasWidth;
            sliceTransform.m13 = shadowSliceData.offsetY * oneOverAtlasHeight;
            
            shadowSliceData.shadowTransform = sliceTransform * shadowSliceData.shadowTransform;
        }
        
        // 最远阴影离淡出因子
        internal static void GetScaleAndBiasForLinearDistanceFade(float fadeDistance, float kBorder, out float scale, out float bias)
        {
            if (kBorder < 0.0001f)
            {
                scale = 1000f;
                bias = -fadeDistance * 1000f;
                return;
            }
            
            kBorder = 1 - kBorder;
            kBorder *= kBorder;
            
            float distanceFadeNear = kBorder * fadeDistance;
            scale = 1.0f / (fadeDistance - distanceFadeNear);
            bias = -distanceFadeNear / (fadeDistance - distanceFadeNear);
        }

        // 返回 阴影质量 （主要是判断是从Light中获取，还是使用管线设置）
        internal static bool SoftShadowQualityToShaderProperty(Light light, ref int shadowQuality)
        {
            if (light.TryGetComponent(out MotoyincLabAdditionalLightData additionalLightData))
            {
                if (additionalLightData.supportSoftShadow == SupportSoftShadow.Off)
                    return false;
                if (additionalLightData.supportSoftShadow != SupportSoftShadow.UsePipelineSettings)
                    shadowQuality = additionalLightData.shadowQuality;
            }
            return true;
        }

        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///                                               Shadow Bias                                          ///
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public static Vector4 GetShadowBias(ref VisibleLight shadowLight, int shadowLightIndex, MotoyincLabShadowData shadowData, Matrix4x4 lightProjectionMatrix, float shadowResolution)
        {
            return GetShadowBias(ref shadowLight, shadowLightIndex, shadowData.bias, shadowData, lightProjectionMatrix, shadowResolution);
        }

        static Vector4 GetShadowBias(ref VisibleLight shadowLight, int shadowLightIndex, List<Vector4> bias, MotoyincLabShadowData shadowData, Matrix4x4 lightProjectionMatrix, float shadowResolution)
        {
            bool supportsSoftShadows = shadowData.supportsSoftShadows;
            if (shadowLightIndex < 0 || shadowLightIndex >= bias.Count)
            {
                Debug.LogWarning($"阴影灯光引索 \"{shadowLightIndex}\" 是无效引索，请检查 bias 列表.");
                return Vector4.zero;
            }
            
            // TODO ：  目前不支持非直射光 Bias 操作
            if (shadowLight.lightType != LightType.Directional)
            {
                Debug.LogWarning($"TODO : 目前不支持非直射光 Bias 操作");
                return Vector4.zero;
            }

            float frustumSize;
            if (shadowLight.lightType == LightType.Directional)
            {
                frustumSize = 2.0f / lightProjectionMatrix.m00;
            }
            else
            {
                Debug.LogWarning("Only point, spot and directional shadow casters are supported in universal pipeline");
                frustumSize = 0.0f;
            }

            float texelSize = frustumSize / shadowResolution;
            float depthBias = -bias[shadowLightIndex].x * texelSize;
            float normalBias = -bias[shadowLightIndex].y * texelSize;
            
            // 支持 SoftShadow 时扩大 Bias的值
            if (supportsSoftShadows && shadowLight.light.shadows == LightShadows.Soft)
            {
                int shadowQuality = shadowData.mainShadowQuality;
                bool isSoftShadowsEnable = ShadowUtils.SoftShadowQualityToShaderProperty(shadowLight.light, ref shadowQuality);
                
                float kernelRadius = 2.5f;

                switch (shadowQuality)
                {
                    case 3: kernelRadius = 3.5f; break; // 7x7
                    case 2: kernelRadius = 2.5f; break; // 5x5
                    case 1: kernelRadius = 1.5f; break; // 3x3
                    default: break;
                }
                depthBias *= kernelRadius;
                normalBias *= kernelRadius;
            }

            return new Vector4(depthBias, normalBias, (float)shadowLight.lightType, 0.0f);
        }
        
        
        // 设置ShadowsMap渲染信息
        internal static void SetupShadowCasterConstantBuffer(CommandBuffer cmd, ref VisibleLight shadowLight, Vector4 shadowBias)
        {
            SetShadowBias(cmd, shadowBias);
            
            Vector3 lightDirection = -shadowLight.localToWorldMatrix.GetColumn(2);
            SetLightDirection(cmd, lightDirection);
            
            Vector3 lightPosition = shadowLight.localToWorldMatrix.GetColumn(3);
            SetLightPosition(cmd, lightPosition);
        }
        
        internal static void SetShadowBias(CommandBuffer cmd, Vector4 shadowBias)
        {
            cmd.SetGlobalVector(ShaderPropertyId.shadowBias, shadowBias);
        }
        
        internal static void SetLightDirection(CommandBuffer cmd, Vector3 lightDirection)
        {
            cmd.SetGlobalVector(ShaderPropertyId.lightDirection, new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, 0.0f));
        }

        internal static void SetLightPosition(CommandBuffer cmd, Vector3 lightPosition)
        {
            cmd.SetGlobalVector(ShaderPropertyId.lightPosition, new Vector4(lightPosition.x, lightPosition.y, lightPosition.z, 1.0f));
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///                                               PCSS                                                 ///
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        internal static Vector4[] GetDirLightPCSSData(Light light)
        {

            Vector4[] pcssData = new Vector4[2];
            if (light.TryGetComponent(out MotoyincLabAdditionalLightData additionalLightData))
            {
                pcssData[0] = new Vector4(
                        additionalLightData.blockerSearchRadiusWS,
                        additionalLightData.blockerSampleCount,
                        additionalLightData.filterSampleCount,
                        0.5f * additionalLightData.blockerSamplingClumpExponent
                    );

                pcssData[1] = new Vector4(
                    additionalLightData.angularDiameter,
                    0, 0, 0
                );

            }
            else
            {
                pcssData[0] = Vector4.zero;
                pcssData[2] = Vector4.zero;
            }

            return pcssData;
        }
    }
}