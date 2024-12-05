namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabRenderPipeline
    {
        static MotoyincLabShadowData CreateShadowData(ContextContainer frameData, MotoyincLabRenderPipelineAsset settings, bool isForwardPlus)
        {
            MotoyincLabShadowData shadowData = frameData.Create<MotoyincLabShadowData>();
            MotoyincLabCameraData cameraData = frameData.Get<MotoyincLabCameraData>();
            
            // 初始化RT大小（并非RT大小）
            shadowData.mainLightShadowmapHeight = settings.shadowSettings.mainLightShadowmapResolution;
            shadowData.mainLightShadowmapWidth = settings.shadowSettings.mainLightShadowmapResolution;
            
            // 初始化联级属性
            shadowData.mainLightShadowCascadesCount = settings.shadowSettings.shadowCascadeCount;
            shadowData.mainLightShadowCascadesSplit = GetMainLightCascadeSplit(shadowData.mainLightShadowCascadesCount, settings);
            shadowData.mainLightShadowCascadeBorder = settings.shadowSettings.cascadeBorder;
            
            
            // 这部分内容是在Pass内做的，这里只先进行初始化
            shadowData.isKeywordAdditionalLightShadowsEnabled = false;
            shadowData.isKeywordSoftShadowsEnabled = false;
            
            // 检查是否允许开启Shadow
            bool isShadowsEnabled = cameraData.maxShadowDistance > 0.0f && SystemInfo.supportsShadows;
            if (isShadowsEnabled) {
                shadowData.supportsMainLightShadows = settings.lightSettings.supportsMainLightShadows;
                shadowData.mainLightShadowsEnabled = settings.lightSettings.supportsMainLightShadows;
                shadowData.supportsAdditionalLightShadows = settings.lightSettings.supportsAdditionalLightShadows;
                shadowData.additionalLightShadowsEnabled = settings.lightSettings.supportsAdditionalLightShadows;
            }
            else {
                shadowData.supportsMainLightShadows = false;
                shadowData.mainLightShadowsEnabled = false;
                shadowData.supportsAdditionalLightShadows = false;
                shadowData.additionalLightShadowsEnabled = false;
            }
            
            // 初始化主光RT和联级大小
            if (shadowData.mainLightShadowsEnabled) {
                // 计算每个联级的分辨率
                shadowData.mainLightShadowResolution = GetMaxTileResolutionInAtlas(shadowData.mainLightShadowmapWidth, shadowData.mainLightShadowmapHeight, shadowData.mainLightShadowCascadesCount);
                shadowData.mainLightRenderTargetWidth = shadowData.mainLightShadowmapWidth;
                // 如果只有两个联级，就把RT高度砍一半
                shadowData.mainLightRenderTargetHeight = (shadowData.mainLightShadowCascadesCount == 2) ? shadowData.mainLightShadowmapHeight >> 1 : shadowData.mainLightShadowmapHeight;
            }
            else {
                shadowData.mainLightShadowResolution = 0;
                shadowData.mainLightRenderTargetWidth = 0;
                shadowData.mainLightRenderTargetHeight = 0;
            }
            
            return shadowData;
        }
        
        // 根据RT和联级数量返回联级的实际尺寸（1个联级是占整个贴图，2个以上联级是占贴图的一半）
        public static int GetMaxTileResolutionInAtlas(int atlasWidth, int atlasHeight, int tileCount)
        {
            int resolution = Mathf.Min(atlasWidth, atlasHeight);
            int currentTileCount = atlasWidth / resolution * atlasHeight / resolution;
            while (currentTileCount < tileCount)
            {
                resolution = resolution >> 1;
                currentTileCount = atlasWidth / resolution * atlasHeight / resolution;
            }
            return resolution;
        }
        
        private static Vector3 GetMainLightCascadeSplit(int mainLightShadowCascadesCount, MotoyincLabRenderPipelineAsset asset)
        {
            switch (mainLightShadowCascadesCount)
            {
                case 1:  return new Vector3(1.0f, 0.0f, 0.0f);
                case 2:  return new Vector3(asset.shadowSettings.cascade2Split, 1.0f, 0.0f);
                case 3:  return asset.shadowSettings.cascade3Split;
                default: return asset.shadowSettings.cascade4Split;
            }
        }
    }
}