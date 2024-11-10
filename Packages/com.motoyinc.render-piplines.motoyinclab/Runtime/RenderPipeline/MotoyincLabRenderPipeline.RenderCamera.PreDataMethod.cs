using Unity.Collections;

namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabRenderPipeline
    {
        static ScriptableRenderer GetRenderer(Camera camera , MotoyincLabAdditionalCameraData additionalCameraData)
        {
            ScriptableRenderer renderer = null;
            if (additionalCameraData != null)
                renderer = additionalCameraData.scriptableRenderer;
            if (renderer == null || camera.cameraType == CameraType.SceneView)
                renderer = asset.scriptableRenderer;
            return renderer;
        }

        static MotoyincLabCameraData CreateCameraData(ContextContainer frameData, Camera camera, MotoyincLabAdditionalCameraData additionalCameraData, bool resolveFinalTarget)
        {
            var renderer = GetRenderer(camera, additionalCameraData);
            MotoyincLabCameraData cameraData = frameData.Create<MotoyincLabCameraData>();
            cameraData.Reset();
            cameraData.camera = camera;
            
            // 初始化填充CameraData
            InitializeStackedCamerData(camera, additionalCameraData, resolveFinalTarget, cameraData);
            
            // ...
            
            return cameraData;
        }
        
        // 初始化 CameraData 通用部分数据
        static void InitializeStackedCamerData(Camera baseCamera, MotoyincLabAdditionalCameraData additionalCameraData,
            bool resolveFinalTarget, MotoyincLabCameraData cameraData)
        {
            var settings = asset;

            // 填充数据
            cameraData.cameraType = baseCamera.cameraType;
            // ... 
        }
        
        // 初始化 CameraData 专用部分数据
        static void InitializeAdditionalCameraData(Camera camera, MotoyincLabAdditionalCameraData additionalCameraData,
            bool resolveFinalTarget, MotoyincLabCameraData cameraData)
        {
            var renderer = GetRenderer(camera, additionalCameraData);
            var settings = asset;
            cameraData.worldSpaceCameraPos = camera.transform.position;
            
            // 填充数据
            cameraData.renderer = renderer;
            // ... 
        }

        static MotoyincLabRenderingData CreateRenderingData(ContextContainer frameData,
            MotoyincLabRenderPipelineAsset settings, CommandBuffer cmd, bool isForwardPlus, ScriptableRenderer renderer)
        {
            MotoyincLabRenderingData data = frameData.Get<MotoyincLabRenderingData>();
            data.supportsDynamicBatching = settings.supportsDynamicBatching;
            data.m_CommandBuffer = cmd;
#if UNITY_EDITOR
            data.globalDebugMode = settings.globalDebugMode;
#endif
            
            return data;
        }

        static MotoyincLabLightData CreateLightData(ContextContainer frameData, MotoyincLabRenderPipelineAsset settings, NativeArray<VisibleLight> visibleLights)
        {
            MotoyincLabLightData lightData = frameData.Create<MotoyincLabLightData>();
            lightData.visibleLights = visibleLights;
            lightData.mainLightIndex = GetMainLightIndex(settings, visibleLights);
            return lightData;
        }

        static int GetMainLightIndex(MotoyincLabRenderPipelineAsset settings, NativeArray<VisibleLight> visibleLights)
        {
            int totalVisibleLights = visibleLights.Length;
            if (totalVisibleLights == 0 || settings.mainLightRenderingMode != LightRenderingMode.PerPixel)
                return -1;
            
            Light sunLight = RenderSettings.sun;
            int brightestDirectionalLightIndex = -1;
            float brightestLightIntensity = 0.0f;

            for (int i = 0; i < totalVisibleLights; i++)
            {
                // 获取灯光
                ref VisibleLight currVisibleLight = ref visibleLights.UnsafeElementAtMutable(i);
                Light currLight = currVisibleLight.light;
                if(currLight == null)
                    break;
                
                // 检查直射光类型
                if (currVisibleLight.lightType == LightType.Directional)
                {
                    // 检查直射光是否是太阳光
                    if (currLight == sunLight)
                        return i;
                    
                    // 对比直射光的亮度
                    if (currLight.intensity > brightestLightIntensity)
                    {
                        brightestLightIntensity = currLight.intensity;
                        brightestDirectionalLightIndex = i;
                    }
                }
            }
            return brightestDirectionalLightIndex;
        }

    }
}