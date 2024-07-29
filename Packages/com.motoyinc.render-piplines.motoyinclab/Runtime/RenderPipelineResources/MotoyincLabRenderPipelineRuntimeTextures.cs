using System;
using NUnit.Framework.Constraints;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

namespace UnityEngine.Rendering.MotoyincLab
{
    [Serializable]
    [SupportedOnRenderPipeline(typeof(MotoyincLabRenderPipelineAsset))]
    public class MotoyincLabRenderPipelineRuntimeTextures : IRenderPipelineResources
    {
        // 资源版本
        [SerializeField][HideInInspector] private int m_Version = 1;
        public int version => m_Version;
        
        bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;

        // 管线Texture资源
        [SerializeField]
        [ResourcePath("Textures/BlueNoise64/L/LDR_LLL1_0.png")]
        private Texture2D m_BlueNoise64LTex;

        public Texture2D blueNoise64LTex
        {
            get => m_BlueNoise64LTex;
            set => this.SetValueAndNotify(ref m_BlueNoise64LTex, value, nameof(m_BlueNoise64LTex));
        }
        
        [SerializeField]
        [ResourcePath("Textures/BayerMatrix.png")]
        private Texture2D m_BayerMatrixTex;
        
        public Texture2D bayerMatrixTex
        {
            get => m_BayerMatrixTex;
            set => this.SetValueAndNotify(ref m_BayerMatrixTex, value, nameof(m_BayerMatrixTex));
        }

        [SerializeField]
        [ResourcePath("Textures/DebugFont.tga")]
        private Texture2D m_DebugFontTex;
        
        public Texture2D debugFontTexture
        {
            get => m_DebugFontTex;
            set => this.SetValueAndNotify(ref m_DebugFontTex, value, nameof(m_DebugFontTex));
        }



    }
}