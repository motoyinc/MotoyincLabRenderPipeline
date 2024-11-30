using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabRenderPipeline
    {
        static MotoyincLabCameraData CreateCameraData(ContextContainer frameData, Camera camera, MotoyincLabAdditionalCameraData additionalCameraData, bool resolveFinalTarget)
        {
            var renderer = GetRenderer(camera, additionalCameraData);
            MotoyincLabCameraData cameraData = frameData.Create<MotoyincLabCameraData>();
            cameraData.Reset();
            cameraData.camera = camera;
            
            // 初始化填充CameraData
            InitializeStackedCamerData(camera, additionalCameraData, resolveFinalTarget, cameraData);
            
            // Descriptor settings                                            
            int msaaSamples = 1;
            if (camera.allowMSAA && asset.msaaSampleCount > 1)
                msaaSamples = (camera.targetTexture != null) ? camera.targetTexture.antiAliasing : asset.msaaSampleCount;
            if (camera.targetTexture == null)
                msaaSamples = (int)XRSystem.GetDisplayMSAASamples();
            bool needsAlphaChannel = Graphics.preserveFramebufferAlpha;
            cameraData.hdrColorBufferPrecision = asset ? asset.hdrColorBufferPrecision : HDRColorBufferPrecision._32Bits;
            cameraData.cameraTargetDescriptor = 
                CreateRenderTextureDescriptor(camera, cameraData, cameraData.isHdrEnabled, cameraData.hdrColorBufferPrecision, msaaSamples, needsAlphaChannel);
            
            return cameraData;
        }
        
        
        // 初始化 CameraData 通用部分数据
        static void InitializeStackedCamerData(Camera baseCamera, MotoyincLabAdditionalCameraData additionalCameraData,
            bool resolveFinalTarget, MotoyincLabCameraData cameraData)
        {
            var settings = asset;

            // 填充数据
            cameraData.cameraType = baseCamera.cameraType;
            cameraData.isHdrEnabled = baseCamera.allowHDR && settings.supportsHDR;
            
            const float kRenderScaleThreshold = 0.05f;
            bool isScenePreviewOrReflectionCamera = cameraData.cameraType == CameraType.SceneView || cameraData.cameraType == CameraType.Preview || cameraData.cameraType == CameraType.Reflection;
            bool disableRenderScale = ((Mathf.Abs(1.0f - settings.renderScale) < kRenderScaleThreshold) || isScenePreviewOrReflectionCamera);
            cameraData.renderScale = disableRenderScale ? 1.0f : settings.renderScale;
        }
        
        
        // 初始化 CameraData 专用部分数据
        static void InitializeAdditionalCameraData(Camera camera, MotoyincLabAdditionalCameraData additionalCameraData,
            bool resolveFinalTarget, MotoyincLabCameraData cameraData)
        {
            var renderer = GetRenderer(camera, additionalCameraData);
            var settings = asset;
            
            // 联级阴影距离
            bool anyShadowsSupports = settings.lightSettings.supportsAdditionalLightShadows || settings.lightSettings.supportsMainLightShadows;
            if (anyShadowsSupports && settings.shadowSettings.maxDistance >= camera.nearClipPlane)
                cameraData.maxShadowDistance = Mathf.Min(settings.shadowSettings.maxDistance, camera.farClipPlane);
            else
                cameraData.maxShadowDistance = 0.0f;
            
            // 相机背景色
            var backgroundColorSRGB = camera.backgroundColor;
#if UNITY_EDITOR
            if (camera.cameraType == CameraType.Preview && camera.clearFlags != CameraClearFlags.SolidColor)
            {
                backgroundColorSRGB = CoreRenderPipelinePreferences.previewBackgroundColor;
            }
#endif
            cameraData.backgroundColor = CoreUtils.ConvertSRGBToActiveColorSpace(backgroundColorSRGB);

            // 相机坐标
            cameraData.worldSpaceCameraPos = camera.transform.position;
            
            // 
            cameraData.renderer = renderer;
            
            // 对不同类型的相机创建不同属性
            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            if (isSceneViewCamera)
            {
                cameraData.renderType = CameraRenderType.Base;
                cameraData.clearDepth = true;
            }
            else if (additionalCameraData != null)
            {
                cameraData.renderType = additionalCameraData.renderType;
                cameraData.clearDepth = (additionalCameraData.renderType != CameraRenderType.Base) ? additionalCameraData.clearDepth : true;
            }
            else
            {
                cameraData.renderType = CameraRenderType.Base;
                cameraData.clearDepth = true;
            }
        }
        
        // 基于相机的RT描述
        internal static RenderTextureDescriptor CreateRenderTextureDescriptor(Camera camera, MotoyincLabCameraData cameraData,
            bool isHdrEnabled, HDRColorBufferPrecision requestHDRColorBufferPrecision, int msaaSamples, bool needsAlpha)
        {
            RenderTextureDescriptor desc;

            if (camera.targetTexture == null)
            {
                desc = new RenderTextureDescriptor(cameraData.scaledWidth, cameraData.scaledHeight);
                desc.graphicsFormat = MakeRenderTextureGraphicsFormat(isHdrEnabled, requestHDRColorBufferPrecision, needsAlpha);
                desc.depthBufferBits = 32;
                desc.msaaSamples = msaaSamples;
                desc.sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            }
            else
            {
                desc = camera.targetTexture.descriptor;
                desc.msaaSamples = msaaSamples;
                desc.width = cameraData.scaledWidth;
                desc.height = cameraData.scaledHeight;

                if (camera.cameraType == CameraType.SceneView && !isHdrEnabled)
                {
                    desc.graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
                }
            }

            desc.enableRandomWrite = false;
            desc.bindMS = false;
            desc.useDynamicScale = camera.allowDynamicResolution;

            // 检查当前平台是否支持请求的 MSAA 采样数。如果不支持，
            // 将 desc.msaaSamples 值替换为引擎回退到的实际值
            desc.msaaSamples = SystemInfo.GetRenderTextureSupportedMSAASampleCount(desc);

            // 如果目标平台不支持存储多重采样的渲染纹理，并且我们正在进行任何离屏 Pass，
            // 作为解决方法，我们禁用 MSAA，以确保之前 Pass 的结果被存储。
            if (!SystemInfo.supportsStoreAndResolveAction)
                desc.msaaSamples = 1;

            return desc;
        }
        
        // 根据需求返回一个合适的格式
        internal static GraphicsFormat MakeRenderTextureGraphicsFormat(bool isHdrEnabled, HDRColorBufferPrecision requestHDRColorBufferPrecision, bool needsAlpha)
        {
            if (isHdrEnabled)
            {
                if (!needsAlpha && requestHDRColorBufferPrecision != HDRColorBufferPrecision._64Bits && SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormatUsage.Blend))
                    return GraphicsFormat.B10G11R11_UFloatPack32;
                if (SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormatUsage.Blend))
                    return GraphicsFormat.R16G16B16A16_SFloat;
                return SystemInfo.GetGraphicsFormat(DefaultFormat.HDR); 
            }

            return SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
        }
    }
}