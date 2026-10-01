using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NOcean
{
    public class CausticFeature : ScriptableRendererFeature
    {
        [Header("Caustics Generation")]
        [Range(0f, 2f)]
        public float causticDisplaceScale = 0.55f; // Caustic distortion / displacement strength
        public Vector3 causticIOR = new Vector3(2.5f, 2.0f, 1.5f); // R/G/B channel IORs; larger differences give stronger dispersion

        [Header("Project Caustics To Sea Floor")]
        public bool projectToSeaFloor = true;
        public Color causticColor = Color.white;
        [Range(0f, 8f)]
        public float causticIntensity = 2.5f;
        [Range(0.1f, 100f)]
        public float causticMaxDepth = 20f;
        [Range(0f, 2f)]
        public float causticSurfaceOffset = 0.1f;
        [Range(0.1f, 16f)]
        public float causticTiling = 1f;

        CausticPass m_CausticPass;
        CausticDecalPass m_DecalPass;

        public override void Create()
        {
            m_CausticPass = new CausticPass();
            m_DecalPass = new CausticDecalPass();
        }


        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            m_CausticPass.Setup(causticDisplaceScale, causticIOR);
            renderer.EnqueuePass(m_CausticPass);

            if (projectToSeaFloor)
            {
                m_DecalPass.Setup(causticColor, causticIntensity, causticMaxDepth, causticSurfaceOffset, causticTiling);
                renderer.EnqueuePass(m_DecalPass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            m_CausticPass.Release();
            m_DecalPass.Release();
        }
    }
}
 