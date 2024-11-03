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
    [CustomEditor(typeof(Light))]
    [SupportedOnRenderPipeline(typeof(MotoyincLabRenderPipelineAsset))]
    [CanEditMultipleObjects]
    public class MotoyincLabRenderPipelinelLightEditor : LightEditor
    {
        public Light light => target as Light;
        private MotoyincLabAdditionalLightData additionalData;

        public new void OnEnable()
        {
            if (light == null)
                return;
            base.OnEnable();
            if (light == null)
                return;
            additionalData = light.GetMotoyincLabAdditionalLightData();
            if (additionalData == null)
                Debug.LogError($"无法找到在 <b>{light.name}</b> 找到 <b>MotoyincLabAdditionallightData</b> 脚本");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
    
}