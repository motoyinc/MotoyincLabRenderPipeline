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
        
        public override void Reset()
        {
            renderer = null;
            camera = null;
            cameraType = CameraType.Game;
        }
    }
}
