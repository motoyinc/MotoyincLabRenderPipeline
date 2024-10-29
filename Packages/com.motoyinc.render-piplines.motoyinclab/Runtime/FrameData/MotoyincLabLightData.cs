using Unity.Collections;
namespace UnityEngine.Rendering.MotoyincLab
{
    public class MotoyincLabLightData : ContextItem
    {
        public int mainLightIndex;
        public NativeArray<VisibleLight> visibleLights;
        public override void Reset()
        {
            mainLightIndex = -1;
            visibleLights = default;
        }
    }
}