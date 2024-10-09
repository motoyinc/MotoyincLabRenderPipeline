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
	public enum RendererType
	{
		Custom,
		MotoyincLabRenderer,
		_2DRenderer,
	}
	
	public partial class MotoyincLabRenderPipelineAsset:RenderPipelineAsset
	{
		protected override RenderPipeline CreatePipeline()
		{
			if (m_RendererDataList == null)
				m_RendererDataList =  new ScriptableRendererData[1];
			
			DestroyRenderers();
			var pipeline = new MotoyincLabRenderPipeline(this);
			CreateRenderers();
			
			return pipeline;
		}
		
		internal void DestroyRenderers()
		{
			if (m_Renderers == null)
				return;
			for (int i = 0; i < m_Renderers.Length; i++)
			{
				DestroyRenderer(ref m_Renderers[i]);
			}
		}

		void DestroyRenderer(ref ScriptableRenderer renderer)
		{
			if (renderer != null)
			{
				renderer.Dispose();
				renderer = null;
			}
		}

		void CreateRenderers()
		{
			// 检查旧列表状态
			if (m_Renderers != null)
			{
				for(int i =0; i<m_Renderers.Length; ++i)
					if (m_Renderers[i] != null)
						Debug.LogError($" m_Renderer[{i}] 旧渲染器数据没有被正常清除，新渲染器会直接覆盖掉旧渲染器");
			}
			
			// 初始化渲染器列表
			if (m_Renderers == null || m_Renderers.Length != m_RendererDataList.Length)
				m_Renderers = new ScriptableRenderer[m_RendererDataList.Length];
			
			// 为每个 RendererData 分配一个渲染器 Renderer
			for (int i = 0; i < m_RendererDataList.Length; ++i)
			{
				if (m_RendererDataList[i] != null)
					m_Renderers[i] = m_RendererDataList[i].InternalCreateRenderer();
			}
		}
		
		/// <summary>
		/// Ensures Global Settings are ready and registered into GraphicsSettings
		/// </summary>
		protected override void EnsureGlobalSettings()
		{
			base.EnsureGlobalSettings();

#if UNITY_EDITOR
			MotoyincLabRenderPipelineGlobalSettings.Ensure();
#endif
		}
		
	}
}

