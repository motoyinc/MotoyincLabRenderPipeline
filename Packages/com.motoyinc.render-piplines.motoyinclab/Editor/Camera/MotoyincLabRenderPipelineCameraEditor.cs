using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.MotoyincLab;

namespace UnityEditor.Rendering.MotoyincLab
{
    [CustomEditor(typeof(Camera))]
    [SupportedOnRenderPipeline(typeof(MotoyincLabRenderPipelineAsset))]
    [CanEditMultipleObjects]
    public class MotoyincLabRenderPipelineCameraEditor : CameraEditor
    {
        public Camera camera => target as Camera;
        private MotoyincLabAdditionalCameraData additionalData;

        public new void OnEnable()
        {
            if (camera == null)
                return;
            base.OnEnable();
            if (camera == null)
                return;
            additionalData = camera.GetMotoyincLabAdditionalCameraData();
            if (additionalData == null)
                Debug.LogError($"无法找到在 <b>{camera.name}</b> 找到 <b>MotoyincLabAdditionalCameraData</b> 脚本");
        }
        
        private SerializedObject serializedCamera;

        public override void OnInspectorGUI()
        {
            serializedCamera.Update();
            base.OnInspectorGUI();
        }
    }
    
}