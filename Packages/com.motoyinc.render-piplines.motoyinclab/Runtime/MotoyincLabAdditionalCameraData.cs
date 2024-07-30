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
    }
}