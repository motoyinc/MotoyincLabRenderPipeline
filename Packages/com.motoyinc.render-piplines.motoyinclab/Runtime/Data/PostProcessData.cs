#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif
using System;

namespace UnityEngine.Rendering.MotoyincLab
{
    

    [Serializable]
    public class PostProcessData: ScriptableObject
    {
        
        // 创建Asset文件
#if UNITY_EDITOR
        internal class CreatePostProcessDataAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var instance = CreateInstance<PostProcessData>();
                AssetDatabase.CreateAsset(instance, pathName);
                ResourceReloader.ReloadAllNullIn(instance, MotoyincLabRenderPipelineAsset.packagePath);
                Selection.activeObject = instance;
            }
        }
        
        [MenuItem("Assets/Create/MotoyincLabRP/MotoyincLabRP Post-process Data")]
        static void CreatePostProcessData()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreatePostProcessDataAsset>(), "CustomPostProcessData.asset", null, null);
        }
        
        internal static PostProcessData GetDefaultPostProcessData()
        {
            var path = System.IO.Path.Combine(MotoyincLabRenderPipelineAsset.packagePath, "Runtime/Data/PostProcessData.asset").Replace("\\", "/");
            var postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(path);
            if (postProcessData == null)
            {
                Debug.LogError("无法正确加载PostProcessData.asset");
                return null;
            }
            else
                return postProcessData;
        }
    }
#endif
}