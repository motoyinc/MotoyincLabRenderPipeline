using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.MotoyincLab
{
    public class MotoyincLabCameraData : ContextItem
    {
        public ScriptableRenderer renderer;
        public Camera camera;
        public CameraType cameraType;
        
        public bool isGameCamera => cameraType == CameraType.Game;
        public bool isSceneViewCamera => cameraType == CameraType.SceneView;
        public bool isPreviewCamera => cameraType == CameraType.Preview;
        internal bool isRenderPassSupportedCamera => (cameraType == CameraType.Game || cameraType == CameraType.Reflection);
        
        public override void Reset()
        {
            renderer = null;
            camera = null;
            cameraType = CameraType.Game;
        }
    }
}
