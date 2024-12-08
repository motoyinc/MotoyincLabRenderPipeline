namespace UnityEngine.Rendering.MotoyincLab
{
#if UNITY_EDITOR
    internal static partial class ShaderPropertyId
    {
        public static readonly int DisplayShadowCascade = Shader.PropertyToID("_DisplayShadowCascade");
        public static readonly int DisplayGeometryBufferMode = Shader.PropertyToID("_DisplayGBuffer");
    }
    
    
    public static partial class ShaderKeywordStrings
    {
        public const string DebugMode = "_DEBUG_MODE";
    }
    
    
    internal static partial class ShaderGlobalKeywords
    {
        public static GlobalKeyword DebugMode;
    }
#endif
}