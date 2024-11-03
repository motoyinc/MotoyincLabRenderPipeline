namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabAdditionalLightData : MonoBehaviour, ISerializationCallbackReceiver, IAdditionalData
    {
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
        }
        
    }
    public static class LightExtensions
    {
        public static MotoyincLabAdditionalLightData GetMotoyincLabAdditionalLightData(this Light light)
        {
            var gameObject = light.gameObject;
            bool componentExists = gameObject.TryGetComponent<MotoyincLabAdditionalLightData>(out var lightData);
            if (!componentExists)
                lightData = gameObject.AddComponent<MotoyincLabAdditionalLightData>();

            return lightData;
        }
    }
}