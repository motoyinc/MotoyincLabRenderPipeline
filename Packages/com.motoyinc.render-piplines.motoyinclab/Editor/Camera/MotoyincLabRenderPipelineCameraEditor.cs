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

        public new void OnEnable()
        {
            base.OnEnable();
            camera.GetMotoyincLabAdditionalCameraData();
        }
    }
    
}