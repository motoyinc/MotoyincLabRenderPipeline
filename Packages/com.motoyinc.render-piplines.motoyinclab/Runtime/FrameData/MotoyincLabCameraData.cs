using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class MotoyincLabCameraData : ContextItem
    {
        public ScriptableRenderer renderer;
        public Camera camera;

        /// <summary>
        /// 用于表示该摄像机是渲染到哪一个窗口的。
        /// 属性值: Game, SceneView, Preview, VR, Reflection
        /// </summary>
        public CameraType cameraType;
        
        /// <summary>
        /// 用于表明摄像机的渲染方式。
        /// 属性值:Overlay 、 Base
        /// </summary>
        public CameraRenderType renderType;
        
        public Vector3 worldSpaceCameraPos;
        public float maxShadowDistance;
        
        public bool clearDepth;
        public Color backgroundColor;
        
        public bool isHdrEnabled;
        public RenderTextureDescriptor cameraTargetDescriptor;
        internal HDRColorBufferPrecision hdrColorBufferPrecision;
        public float renderScale;

        public bool isGameCamera => cameraType == CameraType.Game;
        public bool isSceneViewCamera => cameraType == CameraType.SceneView;
        public bool isPreviewCamera => cameraType == CameraType.Preview;
        internal bool isRenderPassSupportedCamera => (cameraType == CameraType.Game || cameraType == CameraType.Reflection);
        public int scaledWidth => Mathf.Max(1, (int)(camera.pixelWidth * renderScale));
        public int scaledHeight => Mathf.Max(1, (int)(camera.pixelHeight * renderScale));
        
        
        public override void Reset()
        {
            renderType = CameraRenderType.Base;
            
            renderer = null;
            camera = null;
            cameraType = CameraType.Game;
            worldSpaceCameraPos = default;
            maxShadowDistance = 0.0f;
            
            isHdrEnabled = false;
            hdrColorBufferPrecision = HDRColorBufferPrecision._32Bits;
            cameraTargetDescriptor = default;
            renderScale = 1.0f;
            
            backgroundColor = Color.black;
            clearDepth = false;
        }
    }
}
