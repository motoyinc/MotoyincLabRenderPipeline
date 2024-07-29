using System;
using System.IO;
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
        
        internal bool IsAtLastVersion() => k_LastVersion == m_AssetVersion;
        internal const int k_LastVersion = 8;
        [SerializeField][FormerlySerializedAs("k_AssetVersion")]
        internal int m_AssetVersion = k_LastVersion;
        
#if UNITY_EDITOR
        public const string defaultAssetName = "MotoyincLabRPGlobalSettings";
        internal static string defaultPath => $"Assets/{defaultAssetName}.asset";

        internal static MotoyincLabRenderPipelineGlobalSettings Ensure(bool canCreateNewAsset = true)
        {
            MotoyincLabRenderPipelineGlobalSettings currentInstance =
                GraphicsSettings.GetSettingsForRenderPipeline<MotoyincLabRenderPipeline>() as
                    MotoyincLabRenderPipelineGlobalSettings;
            if (RenderPipelineGlobalSettingsUtils.TryEnsure<MotoyincLabRenderPipelineGlobalSettings, MotoyincLabRenderPipeline>(ref currentInstance, defaultPath, canCreateNewAsset))
            {
                
                if (currentInstance != null && !currentInstance.IsAtLastVersion())
                {
                    UpgradeAsset(currentInstance.GetInstanceID());
                    AssetDatabase.SaveAssetIfDirty(currentInstance);
                }

                return currentInstance;
            }
            return currentInstance;
        }

        public override void Initialize(RenderPipelineGlobalSettings source = null)
        {

        }
        
#endif
        public override void Reset()
        {
            base.Reset();
        }

        public static void UpgradeAsset(int assetInstanceID)
        {
            if (EditorUtility.InstanceIDToObject(assetInstanceID) is not MotoyincLabRenderPipelineGlobalSettings asset)
                return;
        }
    }
}
