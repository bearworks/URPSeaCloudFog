using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ZenithGradientColor : MonoBehaviour
{
	public XClouds.XClouds cloud = null;
	public AtmosphericFog fog = null;

	[GradientUsageAttribute(true, ColorSpace.Gamma)]
	public Gradient SunColor = new Gradient();
	[GradientUsageAttribute(true, ColorSpace.Gamma)]
	public Gradient AmbientColor = new Gradient();
	[GradientUsageAttribute(true, ColorSpace.Gamma)]
	public Gradient CloudSunColor = new Gradient();
	[GradientUsageAttribute(true, ColorSpace.Gamma)]
	public Gradient CloudBaseColor = new Gradient();

	// Update is called once per frame
	void Update()
    {
		if(cloud != null && fog != null)
        {
			float ForwardY = 0.75f;
			if (RenderSettings.sun != null)
			{
				Vector3 Forward = (RenderSettings.sun.transform.rotation * Vector3.forward).normalized;
				ForwardY = -Forward.y;
				ForwardY = Mathf.Max(ForwardY * 0.5f + 0.5f, 0f);
			}

			cloud.cloudBaseColor = CloudBaseColor.Evaluate(ForwardY);
			cloud.cloudSunColor = CloudSunColor.Evaluate(ForwardY);

			fog.mSunColor = SunColor.Evaluate(ForwardY);
			fog.mAmbColor = AmbientColor.Evaluate(ForwardY);
			cloud.cloudTopColor = fog.mAmbColor;
		}
	}

}
