using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.Assertions;

namespace UnityEngine.Rendering.MotoyincLab
{
    public enum CameraRenderType
    {
        Base,
        Overlay
    }

    public partial class MotoyincLabAdditionalCameraData
    {
        // 相机类型
        [SerializeField] CameraRenderType m_CameraType = CameraRenderType.Base;
        public CameraRenderType renderType
        {
            get => m_CameraType;
            set => m_CameraType = value;
        }
        
        // 相机列表（堆叠相机）
        [SerializeField] private List<Camera> m_Cameras = new List<Camera>();
        
        public List<Camera> cameraStack
        {
            get
            {
                if (renderType != CameraRenderType.Base)
                {
                    var camera = gameObject.GetComponent<Camera>();
                    Debug.LogWarning($"{camera.name} : 该摄像机的渲染类型是 {renderType}，该类型的摄像机不支持摄像机堆叠");
                    return null;
                }

                if (!scriptableRenderer.SupportsCameraStackingType(CameraRenderType.Base))
                {
                    var camera = gameObject.GetComponent<Camera>();
                    Debug.LogWarning($"{camera.name} : 渲染器不支持摄像机堆叠，请设置渲染器");
                    return null;
                }
                return m_Cameras;
            }
        }
        
        
        // 相机深度清理
        [SerializeField] bool m_ClearDepth = true;
        public bool clearDepth
        {
            get => m_ClearDepth;
        }
        
        // 相机属性变动设置
        void Start()
        {
            if (m_CameraType == CameraRenderType.Overlay)
                camera.clearFlags = CameraClearFlags.Nothing;
        }
        public void OnValidate()
        {
            if (m_CameraType == CameraRenderType.Overlay && m_Camera != null)
            {
                m_Camera.clearFlags = CameraClearFlags.Nothing;
            }
        }
        
        // 相机
        [NonSerialized] Camera m_Camera;
#if UNITY_EDITOR
        internal new Camera camera
#else
        internal Camera camera
#endif
        {
            get
            {
                if (!m_Camera)
                {
                    gameObject.TryGetComponent<Camera>(out m_Camera);
                }
                return m_Camera;
            }
        }
        
        // 摄像机中活动状态的Renderer序号
        [SerializeField] int m_RendererIndex = -1;
        
        public void SetRenderer(int index)
        {
            m_RendererIndex = index;
        }
        
        // 获取相机中活动状态的Renderer
        public ScriptableRenderer scriptableRenderer
        {
            get
            {
                if (MotoyincLabRenderPipeline.asset == null)
                    return null;
                if (!MotoyincLabRenderPipeline.asset.ValidateRendererData(m_RendererIndex))
                {
                    int defaultIndex = MotoyincLabRenderPipeline.asset.m_DefaultRendererIndex;
                    Debug.LogWarning(
                        $"相机指定的渲染器引索 <b>Renderer： {m_RendererIndex.ToString()}</b> 无效。 <b>Camera：{camera.name}</b> 会使用默认渲染器进行渲染. <b>默认 RendererData：{MotoyincLabRenderPipeline.asset.m_RendererDataList[defaultIndex].name}</b>",
                        MotoyincLabRenderPipeline.asset);
                    return MotoyincLabRenderPipeline.asset.GetRenderer(defaultIndex);
                }
                return MotoyincLabRenderPipeline.asset.GetRenderer(m_RendererIndex);
            }
        }

        // 获取相机中活动状态的Renderer
        ScriptableRenderer GetRawRenderer()
        {
            if (MotoyincLabRenderPipeline.asset is null)
                return null;

            ReadOnlySpan<ScriptableRenderer> renderers = MotoyincLabRenderPipeline.asset.renderers;
            if (renderers == null || renderers.IsEmpty)
                return null;

            if (m_RendererIndex >= renderers.Length || m_RendererIndex < 0)
                return null;

            return renderers[m_RendererIndex];
        }

    }
}