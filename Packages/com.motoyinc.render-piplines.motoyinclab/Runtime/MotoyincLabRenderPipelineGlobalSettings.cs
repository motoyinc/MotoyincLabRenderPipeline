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
        
        // Define any global resources for your custom pipeline
        [SerializeField] private MotoyincLabRenderPipelineRuntimeTextures runtimeTextures;

        private static MotoyincLabRenderPipelineGlobalSettings _instance;
        public static MotoyincLabRenderPipelineGlobalSettings instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GraphicsSettings.GetSettingsForRenderPipeline<MotoyincLabRenderPipeline>() as MotoyincLabRenderPipelineGlobalSettings;
                }
                return _instance;
            }
            set => _instance = value;
        }

        // public MotoyincLabRenderPipelineRuntimeTextures RuntimeTextures => runtimeTextures;
        public MotoyincLabRenderPipelineRuntimeTextures RuntimeTextures => GetOrCreateGraphicsSettings<MotoyincLabRenderPipelineRuntimeTextures>(this);

        private static T GetOrCreateGraphicsSettings<T>(MotoyincLabRenderPipelineGlobalSettings data)
            where T : class, IRenderPipelineResources, new()
        {
            T settings;

            if (data.TryGet(typeof(T), out var baseSettings))
            {
                settings = baseSettings as T;
            }
            else
            {
                settings = new T();
                data.Add(settings);
            }

            return settings;
        }

        [SettingsProvider]
        public static SettingsProvider CreateMotoyincGraphicsSettingsProvider()
        {
            var provider = new SettingsProvider("Project/MotoyincLab Graphics", SettingsScope.Project)
            {
                guiHandler = (searchContext) =>
                {
                    instance = GraphicsSettings.GetSettingsForRenderPipeline<MotoyincLabRenderPipeline>() as MotoyincLabRenderPipelineGlobalSettings;
                    if (instance == null)
                    {
                        EditorGUILayout.HelpBox("No MotoyincLab global settings assigned. Please create or assign one.", MessageType.Warning);
                        if (GUILayout.Button("Create New Settings"))
                        {
                            instance = CreateInstance<MotoyincLabRenderPipelineGlobalSettings>();
                            string path = "Assets/MotoyincLabGlobalSettings.asset";
                            AssetDatabase.CreateAsset(instance, path);
                            GraphicsSettings.RegisterRenderPipelineSettings<MotoyincLabRenderPipeline>(instance);
                        }
                    }
                    else
                    {
                        // Draw GUI to edit the settings
                        SerializedObject serializedSettings = new SerializedObject(instance);
                        EditorGUILayout.PropertyField(serializedSettings.FindProperty("runtimeTextures"), new GUIContent("Runtime Textures"));
                        serializedSettings.ApplyModifiedProperties();
                    }
                },
                keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(new SerializedObject(instance))
            };
            return provider;
        }
        
        public static MotoyincLabRenderPipelineGlobalSettings EnsureInstance()
        {
            if (_instance == null)
            {
                _instance = GraphicsSettings.GetSettingsForRenderPipeline<MotoyincLabRenderPipeline>() as MotoyincLabRenderPipelineGlobalSettings;

                if (_instance == null)
                {
                    Debug.LogError("No MotoyincLabRenderPipelineGlobalSettings assigned. Please assign it in the Graphics Settings.");
                }
            }
            return _instance;
        }
        
        /// <summary>Default name when creating an URP Global Settings asset.</summary>
        public const string defaultAssetName = "MotoyincLabRenderPipelineGlobalSettings";
        internal static string defaultPath => $"Assets/{defaultAssetName}.asset";
        
        internal static MotoyincLabRenderPipelineGlobalSettings Ensure(bool canCreateNewAsset = true)
        {
            MotoyincLabRenderPipelineGlobalSettings currentInstance = GraphicsSettings.
                GetSettingsForRenderPipeline<MotoyincLabRenderPipeline>() as MotoyincLabRenderPipelineGlobalSettings;

            if (RenderPipelineGlobalSettingsUtils.TryEnsure<MotoyincLabRenderPipelineGlobalSettings, MotoyincLabRenderPipeline>(ref currentInstance, defaultPath, canCreateNewAsset))
            {
                return currentInstance;
            }

            return null;
        }
    }
}
