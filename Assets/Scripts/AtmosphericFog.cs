using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class AtmosphericFog : MonoBehaviour
{
    [HeaderAttribute("Height Fog")]
    public float skyFogDistanceFactor = 0.5f;
    [Range(0.1f,1.2f)]
    public float skyFogRange = 0.5f;
    public float skyFogStart = 0f;

    [Space]
    public float heightFogDistanceFactor = 0.5f;
    public float heightFogRange = 0.5f;
    public float heightFogStart = 0f;

    [Space]
    [Range(0.01f, 0.98f)]
    public float mMieAsymmetry = 0f;

    [ColorUsage(false, true)]
    public Color mAlbedoR;

    [ColorUsage(false, true)]
    public Color mAlbedoM;

    [ColorUsage(false, true)]
    public Color mSunColor;

    [ColorUsage(false, true)]
    public Color mAmbColor;

    [HeaderAttribute("Scattering Weights")]
    public Vector4 betaRsMs = Vector4.one;
    public float betaMa = 0f;

    // Update is called once per frame
    void Update()
    { 
        RenderSettings.fogMode = FogMode.Linear;

        Shader.SetGlobalVector("_SkyFogParams", new Vector4(Mathf.Max(0.0f, skyFogDistanceFactor), skyFogRange, skyFogStart, 0f));
        Shader.SetGlobalVector("_HeightFogParams", new Vector4(heightFogDistanceFactor, heightFogRange, heightFogStart, 0f));

        Shader.SetGlobalVector("_BetaRsMs", betaRsMs);
        Shader.SetGlobalFloat("_BetaMa", betaMa);
        Shader.SetGlobalFloat("mMieAsymmetry", mMieAsymmetry);

        Shader.SetGlobalColor("mAlbedoR", mAlbedoR);
        Shader.SetGlobalColor("mAlbedoM", mAlbedoM);
        Shader.SetGlobalColor("mSunColor", mSunColor);
        Shader.SetGlobalColor("mAmbColor", mAmbColor);
	}

    private void Reset()
    {
        Shader.SetGlobalVector("_SkyFogParams", new Vector4(0, 1, 1, 0));
        Shader.SetGlobalVector("_HeightFogParams", new Vector4(0, 1, 1, 0));
        Shader.SetGlobalVector("_BetaRsMs", Vector4.one);
        Shader.SetGlobalFloat("_BetaMa", 1);
        Shader.SetGlobalFloat("mMieAsymmetry", 0.5f);

        Shader.SetGlobalColor("mAlbedoR", Color.black);
        Shader.SetGlobalColor("mAlbedoM", Color.black);
        Shader.SetGlobalColor("mSunColor", Color.black);
        Shader.SetGlobalColor("mAmbColor", Color.black);
    }

    private void OnDisable()
    {
        Reset();
    }

    private void Start()
    {
        Reset();
    }

}
