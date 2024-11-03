using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.Assertions;

namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabAdditionalCameraData : MonoBehaviour, ISerializationCallbackReceiver, IAdditionalData
    {
        [HideInInspector] [SerializeField] float m_Version = 2;
        public float version => m_Version;
        
        public void OnBeforeSerialize()
        {
        }
        
        public void OnAfterDeserialize()
        {
            if (version <= 1)
            {
                // 将旧属性，迁移给新属性
                m_Version = 2;
            }
        }
    }

    public static class CameraExtensions
    {
        public static MotoyincLabAdditionalCameraData GetMotoyincLabAdditionalCameraData(this Camera camera)
        {
            var gameObject = camera.gameObject;
            bool componentExists = gameObject.TryGetComponent<MotoyincLabAdditionalCameraData>(out var cameraData);
            if (!componentExists)
                cameraData = gameObject.AddComponent<MotoyincLabAdditionalCameraData>();

            return cameraData;
        }
    }
}