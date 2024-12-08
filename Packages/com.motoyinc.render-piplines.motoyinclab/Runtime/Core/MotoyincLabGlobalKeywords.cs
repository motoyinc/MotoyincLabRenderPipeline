namespace UnityEngine.Rendering.MotoyincLab
{
    public static class ShaderKeywordStrings
    {
        public const string MainLightShadows = "_MAIN_LIGHT_SHADOWS";
        public const string MainLightShadowCascades = "_MAIN_LIGHT_SHADOWS_CASCADE";
#if UNITY_EDITOR
        public const string DebugMode = "_DEBUG_MODE";
#endif
    }

    internal static class ShaderGlobalKeywords
    {
        public static GlobalKeyword MainLightShadows;
        public static GlobalKeyword MainLightShadowCascades;
#if UNITY_EDITOR
        public static GlobalKeyword DebugMode;
#endif
        
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