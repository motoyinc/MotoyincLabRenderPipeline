namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class ScriptableRenderer
    {
        private static partial class Profiling
        {
            private const string k_Name = nameof(ScriptableRenderer);

            public static readonly ProfilingSampler clearRenderingState = new ProfilingSampler($"{k_Name}.{nameof(ClearRenderingState)}");
        }
    }
}