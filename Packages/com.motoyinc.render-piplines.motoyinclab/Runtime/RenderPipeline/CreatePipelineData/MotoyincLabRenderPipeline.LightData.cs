using Unity.Collections;
namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabRenderPipeline
    {
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
            if (totalVisibleLights == 0)
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