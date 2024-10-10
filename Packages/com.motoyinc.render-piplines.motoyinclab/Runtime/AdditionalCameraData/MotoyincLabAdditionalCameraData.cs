using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.Assertions;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class MotoyincLabAdditionalCameraData : MonoBehaviour
    {
        [SerializeField] private CameraRenderType m_cameraRenderType = CameraRenderType.Base;
        [SerializeField] private List<Camera> m_cameras = new List<Camera>();
        
        public ScriptableRenderer scriptableRenderer
        {
            get
            {
                if (MotoyincLabRenderPipeline.asset == null)
                    return null;
                
                var index = MotoyincLabRenderPipeline.asset.m_DefaultRendererIndex;
                // TODO: 用户需要根据需求获取一个活跃状态的 Renderer Index 的一个方法
                if (false)
                    index = 0;
                
                return MotoyincLabRenderPipeline.asset.GetRenderer(index);
            }
        }

        public CameraRenderType renderType
        {
            get => m_cameraRenderType;
            set => m_cameraRenderType = value;
        }

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
                return m_cameras;
            }
        }
    }
}