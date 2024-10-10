using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine.Rendering.MotoyincLab;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
#endif

namespace UnityEngine.Rendering.MotoyincLab
{
    [SupportedOnRenderPipeline(typeof(MotoyincLabRenderPipelineAsset))]
    [DisplayName("Motoyinc Lab RP")]
    partial class MotoyincLabRenderPipelineGlobalSettings : RenderPipelineGlobalSettings<MotoyincLabRenderPipelineGlobalSettings,MotoyincLabRenderPipeline>
    {
        [SerializeField] RenderPipelineGraphicsSettingsContainer m_Settings = new();
        protected override List<IRenderPipelineGraphicsSettings> settingsList => m_Settings.settingsList;
        
        private static MotoyincLabRenderPipelineGlobalSettings m_instance;

#if UNITY_EDITOR
        
        /// <summary>Default name when creating an URP Global Settings asset.</summary>
        public const string defaultAssetName = "MotoyincLabRenderPipelineGlobalSettings";
        internal static string defaultPath => $"Assets/{defaultAssetName}.asset";
        
        internal static MotoyincLabRenderPipelineGlobalSettings Ensure(bool canCreateNewAsset = true)
        {
            MotoyincLabRenderPipelineGlobalSettings currentInstance = GraphicsSettings.
                GetSettingsForRenderPipeline<MotoyincLabRenderPipeline>() as MotoyincLabRenderPipelineGlobalSettings;

            if (RenderPipelineGlobalSettingsUtils.TryEnsure<MotoyincLabRenderPipelineGlobalSettings, MotoyincLabRenderPipeline>(ref currentInstance, defaultPath, canCreateNewAsset))
            {
                if (currentInstance != null)
                {
                    AssetDatabase.SaveAssetIfDirty(currentInstance);
                }
                return currentInstance;
            }

            return null;
        }
#endif
    }
}
