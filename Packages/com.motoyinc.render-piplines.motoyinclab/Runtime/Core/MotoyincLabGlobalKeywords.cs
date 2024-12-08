namespace UnityEngine.Rendering.MotoyincLab
{
    public static partial class ShaderKeywordStrings
    {
        public const string MainLightShadows = "_MAIN_LIGHT_SHADOWS";
        public const string MainLightShadowCascades = "_MAIN_LIGHT_SHADOWS_CASCADE";
    }

    internal static partial class ShaderGlobalKeywords
    {
        public static GlobalKeyword MainLightShadows;
        public static GlobalKeyword MainLightShadowCascades;
        
        public static void InitializeShaderGlobalKeywords()
        {
            ShaderGlobalKeywords.MainLightShadows = GlobalKeyword.Create(ShaderKeywordStrings.MainLightShadows);
            ShaderGlobalKeywords.MainLightShadowCascades = GlobalKeyword.Create(ShaderKeywordStrings.MainLightShadowCascades);
#if UNITY_EDITOR
            ShaderGlobalKeywords.DebugMode = GlobalKeyword.Create(ShaderKeywordStrings.DebugMode);
#endif
        }
    }
    
    
}