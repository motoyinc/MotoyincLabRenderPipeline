using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;
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
            
        public void Clear()
        {
            viewMatrix = Matrix4x4.identity;
            projectionMatrix = Matrix4x4.identity;
            shadowTransform = Matrix4x4.identity;
            offsetX = offsetY = 0;
            resolution = 1024;
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
        
        public static Matrix4x4 GetDirectionalLightMatrix(ref ShadowSliceData shadowSliceData, int cascadeCounts, int atlasWidth, int atlasHeight)
        {
            Matrix4x4 view = shadowSliceData.viewMatrix;
            Matrix4x4 proj = shadowSliceData.projectionMatrix;
            if (SystemInfo.usesReversedZBuffer)
            {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }

            
            Matrix4x4 worldToShadow = proj * view;
            
            // [-1,1] to [0,1]
            var textureScaleAndBias = Matrix4x4.identity;
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;
            Matrix4x4 shadowTransform = textureScaleAndBias * worldToShadow;
            

            return shadowTransform;
        }
        
        
        
        
        
    }
}