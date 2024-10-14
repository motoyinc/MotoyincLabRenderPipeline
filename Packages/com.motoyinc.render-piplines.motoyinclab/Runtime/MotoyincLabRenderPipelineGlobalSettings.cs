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
        
        /// <summary>Default name when creating an URP Global Settings asset.</summary>
        public const string defaultAssetName = "MotoyincLabRenderPipelineGlobalSettings";
        internal static string defaultPath => $"Assets/{defaultAssetName}.asset";
        
        // 版本控制
        internal static int k_LastVersion = 8;
        [SerializeField] internal int m_AssetVersion = k_LastVersion;
        

#if UNITY_EDITOR
        public static void UpgradeAsset(int assetInstanceID)
        {
            if (EditorUtility.InstanceIDToObject(assetInstanceID) is not MotoyincLabRenderPipelineGlobalSettings asset)
                return;
            int assetVersionBeforeUpgrade = asset.m_AssetVersion;

            if (asset.m_AssetVersion < 2)
            {
                // 2...
                asset.m_AssetVersion = 2;
            }
            if (asset.m_AssetVersion < 3)
            {
                // 3...
                asset.m_AssetVersion = 3;
            }
            if (asset.m_AssetVersion < 4)
            {
                // 4...
                asset.m_AssetVersion = 4;
            }
            if (asset.m_AssetVersion < 5)
            {
                // 5...
                asset.m_AssetVersion = 5;
            }
            if (asset.m_AssetVersion < 6)
            {
                // 6...
                asset.m_AssetVersion = 6;
            }
            if (asset.m_AssetVersion < 7)
            {
                // 7...
                asset.m_AssetVersion = 7;
            }
            if (asset.m_AssetVersion < 8)
            {
                // 8...
                asset.m_AssetVersion = 8;
            }
            
            if (assetVersionBeforeUpgrade != asset.m_AssetVersion)
                EditorUtility.SetDirty(asset);
        }
        
        
        internal static MotoyincLabRenderPipelineGlobalSettings Ensure(bool canCreateNewAsset = true)
        {
            MotoyincLabRenderPipelineGlobalSettings currentInstance = GraphicsSettings.
                GetSettingsForRenderPipeline<MotoyincLabRenderPipeline>() as MotoyincLabRenderPipelineGlobalSettings;

            if (RenderPipelineGlobalSettingsUtils.TryEnsure<MotoyincLabRenderPipelineGlobalSettings, MotoyincLabRenderPipeline>(ref currentInstance, defaultPath, canCreateNewAsset))
            {
                if (currentInstance != null)
                {
                    UpgradeAsset(currentInstance.GetInstanceID());
                    AssetDatabase.SaveAssetIfDirty(currentInstance);
                }
                return currentInstance;
            }

            return null;
        }
#endif
    }
}
