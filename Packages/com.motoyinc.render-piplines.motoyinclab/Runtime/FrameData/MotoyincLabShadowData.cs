namespace UnityEngine.Rendering.MotoyincLab
{
    public class MotoyincLabShadowData : ContextItem
    {
        // Asset信息
        public int mainLightShadowmapWidth;             // 阴影贴图宽度
        public int mainLightShadowmapHeight;            // 阴影贴图高度
        public bool supportsMainLightShadows;           // 支持主光阴影
        public bool supportsAdditionalLightShadows;     // 支持附加光阴影
        public int mainLightShadowCascadesCount;        // 联级 联级数量
        public Vector3 mainLightShadowCascadesSplit;    // 联级分割比
        
        // 主光信息
        internal bool mainLightShadowsEnabled;          // 开启阴影
        internal int mainLightShadowResolution;         // 联级 每个联级RT大小
        internal int mainLightRenderTargetWidth;        // 阴影RT 整个RT的宽度
        internal int mainLightRenderTargetHeight;       // 阴影RT 整个RT的高度
        
        // 附加光信息
        internal bool additionalLightShadowsEnabled;
        internal bool isKeywordAdditionalLightShadowsEnabled;
        
        // 其他信息
        internal bool isKeywordSoftShadowsEnabled;
        
        public override void Reset()
        {
            
            
            mainLightShadowmapWidth = 0;
            mainLightShadowmapHeight = 0;
            supportsMainLightShadows = false;
            supportsAdditionalLightShadows = false;
            mainLightShadowCascadesCount = 0;
            mainLightShadowCascadesSplit = Vector3.zero;
            
            mainLightShadowsEnabled = false;
            mainLightShadowResolution = 0;
            mainLightRenderTargetWidth = 0;
            mainLightRenderTargetHeight = 0;

            additionalLightShadowsEnabled = false;
            isKeywordAdditionalLightShadowsEnabled = false;
            isKeywordSoftShadowsEnabled = false;
        }
    }
}