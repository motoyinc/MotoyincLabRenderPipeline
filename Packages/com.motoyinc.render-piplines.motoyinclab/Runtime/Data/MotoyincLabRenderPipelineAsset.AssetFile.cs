using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using System.IO;
using ShaderKeywordFilter = UnityEditor.ShaderKeywordFilter;
#endif
using System.ComponentModel;
using UnityEngine.Serialization;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.MotoyincLab;

namespace UnityEngine.Rendering.MotoyincLab
{
    public partial class MotoyincLabRenderPipelineAsset
    {
#if UNITY_EDITOR
        public static readonly string packagePath = "Packages/com.motoyinc.render-piplines.motoyinclab";
        internal class CreateMotoyincLabRenderPipelineAsset : EndNameEditAction
        {
        	public override void Action(int instanceId, string pathName, string resourceFile)
        	{
        		// 创建Assets实例
        		var piplineAsset = CreateInstance<MotoyincLabRenderPipelineAsset>();
        		
        		// 创建RenderData
        		var rendererData = CreateRendererData(pathName, RendererType.MotoyincLabRenderer);
        		piplineAsset.m_RendererDataList[0] = rendererData;
        		
        		// 将实例创建成Assets文件
        		AssetDatabase.CreateAsset(piplineAsset, pathName);
        		ResourceReloader.ReloadAllNullIn(piplineAsset, packagePath);
        	}
        }

        [MenuItem("Assets/Create/MotoyincLabRP/MotoyincLabRP Asset")]
        static void CreateMotoyincLabPipeline()
        {
        	ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateMotoyincLabRenderPipelineAsset>(), "new MotoyincLabRP.asset", null, null);
        }
        
        internal static ScriptableRendererData CreateRendererData(string path, RendererType type, bool relativePath = true, string suffix = "RendererData")
        {
        	// rendererData路径
        	string dataPathName;
        	if (relativePath)
        		dataPathName =
        			$"{Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path))}_{suffix}{Path.GetExtension(path)}";
        	else
        		dataPathName = path;
        	
        	//创建renderdata文件
        	var rendererData = CreateInstance<MotoyincLabRendererData>();
        	rendererData.postProcessData = PostProcessData.GetDefaultPostProcessData();
        	AssetDatabase.CreateAsset(rendererData,dataPathName);
        	ResourceReloader.ReloadAllNullIn(rendererData, packagePath);
        	return rendererData;
        }
#endif
        
        // 配置好渲染相关的 Global Settings 并注册到窗口 ProjectSettings>Graphics
        protected override void EnsureGlobalSettings()
        {
        	base.EnsureGlobalSettings();
#if UNITY_EDITOR
        	MotoyincLabRenderPipelineGlobalSettings.Ensure();
#endif
        }
    }
}