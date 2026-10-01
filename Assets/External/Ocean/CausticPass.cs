using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NOcean
{
    public class CausticPass: ScriptableRenderPass
    {

        static readonly string k_RenderTag = "Caustics";

        // Generation parameters (set by CausticFeature; defaults match the original shader's static consts)
        float _displaceScale = 0.55f;
        Vector3 _ior = new Vector3(2.5f, 2.0f, 1.5f);

        public CausticPass()
        {
            _causticMaterial = CreateMaterial("Hidden/URPOcean/Caustic");
            _causticMaterial_FixGaps = CreateMaterial("Hidden/URPOcean/CausticFixGaps");
        }

        public void Setup(float displaceScale, Vector3 ior)
        {
            _displaceScale = displaceScale;
            _ior = ior;
        }
        
        public static Vector2Int GetScreenSize()
        {
            return new Vector2Int(Screen.width, Screen.height);
        }
        
        static RTHandleSystem _RTHandles;

        internal static RTHandleSystem RTHandles
        {
            get
            {
                if (_RTHandles == null)
                {
                    _RTHandles = new RTHandleSystem();
                    var screenSize = GetScreenSize();
                    _RTHandles.Initialize(screenSize.x, screenSize.y);
                }
                return _RTHandles;
            }
        }

        internal static void ReleaseRTHandles()
        {
            _RTHandles?.Dispose();
            _RTHandles = null;
        }

        public static RTHandle CausticRT { get; internal set; }
        public static RTHandle CausticRT_Temp { get; internal set; }

        internal static int MaxCausticResolution = 512;

        static        RTHandle _dummyRT;
        public static RTHandle dummyRT => _dummyRT ?? (_dummyRT = RTHandles.Alloc(1, 1));

        private Mesh _causticMesh;

        Material _causticMaterial;
        Material _causticMaterial_FixGaps;

     
        public static void SafeDestroy(params UnityEngine.Object[] components)
        {
            if (!Application.isPlaying)
            {
                foreach (var component in components)
                {
                    if (component != null) UnityEngine.Object.DestroyImmediate(component);
                }

            }
            else
            {
                foreach (var component in components)
                {
                    if (component != null) UnityEngine.Object.Destroy(component);
                }
            }
        }


        public static void ClearRenderTexture(RenderTexture rt, ClearFlag clearFlag, Color clearColor)
        {
            var activeRT = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear((clearFlag & ClearFlag.Depth) != 0, (clearFlag & ClearFlag.Color) != 0, clearColor);
            RenderTexture.active = activeRT;
        }

        public static Material CreateMaterial(string shaderName)
        {
            var waterShader = Shader.Find(shaderName);
            if (waterShader == null)
            {
                Debug.LogError("Can't find the shader '" + shaderName + "' in the resources folder. Try to reimport the package.");
                return null;
            }

            var waterMaterial = new Material(waterShader);
            waterMaterial.hideFlags = HideFlags.HideAndDontSave;
            return waterMaterial;
        }

        void InitializeTextures()
        {
            var size = MaxCausticResolution;

            CausticRT = RTHandles.Alloc(size, size, colorFormat: GraphicsFormat.R8G8B8A8_SRGB, name: "_CausticRT", useMipMap: false, autoGenerateMips: false);
            Shader.SetGlobalTexture("_CausticRT", CausticRT);

            CausticRT_Temp = RTHandles.Alloc(size * 2, size * 2, colorFormat: GraphicsFormat.R8G8B8A8_SRGB, name: "_CausticRT_Temp", useMipMap: false, autoGenerateMips: false);

            ClearRenderTexture(CausticRT_Temp.rt, ClearFlag.Color, new Color(0, 0, 0));
            ClearRenderTexture(CausticRT.rt, ClearFlag.Color, new Color(0, 0, 0));
        }

        void ReleaseTextures()
        {
            CausticRT?.Release();
            CausticRT = null;

        }

        public void Release()
        {
            ReleaseTextures();
            SafeDestroy(_causticMaterial);
            SafeDestroy(_causticMaterial_FixGaps);

            SafeDestroy(_causticMesh);
            _causticMesh = null;

        }

        public static Mesh CreatePlaneMesh(int meshResolution, float scale)
        {
            var vertices = new Vector3[(meshResolution + 1) * (meshResolution + 1)];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[meshResolution * meshResolution * 6];

            for (int i = 0, y = 0; y <= meshResolution; y++)
                for (var x = 0; x <= meshResolution; x++, i++)
                {
                    vertices[i] = new Vector3(x * scale / meshResolution - 0.5f * scale, y * scale / meshResolution - 0.5f * scale, 0);
                    uv[i] = new Vector2(x * 1.0f / meshResolution, y * 1.0f / meshResolution);
                }

            for (int ti = 0, vi = 0, y = 0; y < meshResolution; y++, vi++)
                for (var x = 0; x < meshResolution; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + meshResolution + 1;
                    triangles[ti + 5] = vi + meshResolution + 2;
                }

            var mesh = new Mesh { hideFlags = HideFlags.DontSave, indexFormat = IndexFormat.UInt32 };

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            return mesh;
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if(NeoOcean.instance == null)
                return;

            if(NeoOcean.instance.WaveMap == null)
                return;
                
            ConfigureTarget(dummyRT, dummyRT);

            var cmd = CommandBufferPool.Get(k_RenderTag);
            cmd.Clear();

            if(CausticRT == null) InitializeTextures();

            CoreUtils.SetRenderTarget(cmd, CausticRT_Temp, ClearFlag.Color, Color.black);

            cmd.SetGlobalFloat("_TexSize", NeoOcean.instance.WaveMap.width);

            cmd.SetGlobalTexture("_Map0", NeoOcean.instance.WaveMap);

            _causticMaterial.SetFloat("_DisplaceScale", _displaceScale);
            _causticMaterial.SetFloat("_IorR", _ior.x);
            _causticMaterial.SetFloat("_IorG", _ior.y);
            _causticMaterial.SetFloat("_IorB", _ior.z);

            var mesh = GetOrCreateCausticMesh();
            cmd.DrawMesh(mesh, Matrix4x4.identity, _causticMaterial);

            CoreUtils.SetRenderTarget(cmd, CausticRT, ClearFlag.Color, Color.black);

            cmd.Blit(CausticRT_Temp, CausticRT,_causticMaterial_FixGaps);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        Mesh GetOrCreateCausticMesh()
        {
            const int size = 512;
            if (_causticMesh == null)
            {
                _causticMesh = CreatePlaneMesh(size, 1.0f);
            }

            return _causticMesh;
        }

    }

    /// <summary>
    /// Projects the caustic texture generated by CausticPass onto the sea floor (opaque geometry below the water surface).
    /// Samples scene depth to reconstruct world position, samples _CausticRT aligned with the ocean waves by world XZ, additive blending.
    /// </summary>
    public class CausticDecalPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Caustics SeaFloor Decal";
        static readonly string k_CausticDecalShader = "Hidden/URPOcean/CausticDecal";

        Material _decalMaterial;
        Mesh _fullscreenMesh;

        Color _color = Color.white;
        float _intensity = 2.5f;
        float _maxDepth = 20f;
        float _surfaceOffset = 0.1f;
        float _tiling = 1f;

        public CausticDecalPass()
        {
            // Runs after opaques, before skybox/water (Queue=Transparent-11):
            // caustics land on the opaque sea floor first, then the water surface renders and blends correctly.
            // Not BeforeRenderingTransparents - it shares the same event value as URP's built-in transparent passes,
            // so the relative order is not guaranteed and caustics could be drawn after the water surface.
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            // Declare depth texture dependency: URP automatically schedules CopyDepthPass before this pass,
            // ensuring _CameraDepthTexture is ready when sampled (without triggering an extra depth prepass)
            ConfigureInput(ScriptableRenderPassInput.Depth);
            _decalMaterial = CausticPass.CreateMaterial(k_CausticDecalShader);
        }

        public void Setup(Color color, float intensity, float maxDepth, float surfaceOffset, float tiling)
        {
            _color = color;
            _intensity = intensity;
            _maxDepth = maxDepth;
            _surfaceOffset = surfaceOffset;
            _tiling = tiling;
        }

        public void Release()
        {
            CausticPass.SafeDestroy(_decalMaterial);
            _decalMaterial = null;

            CausticPass.SafeDestroy(_fullscreenMesh);
            _fullscreenMesh = null;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (NeoOcean.instance == null || NeoOcean.instance.WaveMap == null)
                return;

            // Requires the caustic texture output by the generation pass
            if (CausticPass.CausticRT == null || _decalMaterial == null)
                return;

            var cameraData = renderingData.cameraData;

            // Skip projection for preview / reflection cameras
            if (cameraData.cameraType == CameraType.Preview || cameraData.cameraType == CameraType.Reflection)
                return;

            if (_fullscreenMesh == null)
                _fullscreenMesh = CausticPass.CreatePlaneMesh(1, 2.0f); // quad covering NDC [-1,1]

            var cmd = CommandBufferPool.Get(k_RenderTag);
            cmd.Clear();

            // For water height sampling and caustic sampling
            cmd.SetGlobalTexture("_Map0", NeoOcean.instance.WaveMap);
            cmd.SetGlobalTexture("_CausticRT", CausticPass.CausticRT);

            _decalMaterial.SetColor("_CausticColor", _color);
            _decalMaterial.SetFloat("_CausticIntensity", _intensity);
            _decalMaterial.SetFloat("_CausticMaxDepth", _maxDepth);
            _decalMaterial.SetFloat("_CausticSurfaceOffset", _surfaceOffset);
            _decalMaterial.SetFloat("_CausticTiling", _tiling);
            // Caustic uv alignment: causticUV = worldXZ * mainScale * 2 + 0.5
            _decalMaterial.SetFloat("_CausticScale", NeoOcean.mainScale * 2f);
            _decalMaterial.SetFloat("_InvNeoScale", NeoOcean.mainScale);
            _decalMaterial.SetFloat("_OceanLevel", NeoOcean.oceanheight);

            CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTargetHandle,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, ClearFlag.None, Color.clear);

            cmd.DrawMesh(_fullscreenMesh, Matrix4x4.identity, _decalMaterial);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}