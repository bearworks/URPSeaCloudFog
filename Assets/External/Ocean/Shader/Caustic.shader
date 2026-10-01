Shader "Hidden/URPOcean/Caustic"
{
	HLSLINCLUDE

	#include "NeoInclude.hlsl"

	float _TexSize; // Set globally via cmd.SetGlobalFloat

	// Effect parameters, set by CausticFeature through the material (extracted from the original static const defaults)
	CBUFFER_START(UnityPerMaterial)
		float _DisplaceScale; // Vertex displacement scale (caustic distortion strength), originally dis_scale = 0.55
		float _IorR;          // R channel IOR, originally IOR_WATER1 = 2.5
		float _IorG;          // G channel IOR, originally IOR_WATER2 = 2
		float _IorB;          // B channel IOR, originally IOR_WATER3 = 1.5
	CBUFFER_END

	// Wave map sampling multiplier: must be 4. Other values break the tiling periodicity of the
	// temp grid displacement field, leaving coverage gaps at the edges of the CausticFixGaps
	// extraction window (middle 2x2) and cracks after tiling.
	// 4 also makes the caustic pattern match the ocean's 3rd octave (uv*4) frequency.
	static const float scale = 4;

	struct appdata_caustic
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f_caustic
	{
		float4 vertex : SV_POSITION;
		float3 oldPos : TEXCOORD0;
		float3 newPos : TEXCOORD1;
		float3 newPos2 : TEXCOORD2;
		float3 newPos3 : TEXCOORD3;
		float2 uv : TEXCOORD4;
	};

	float3 ComputeDisplacementXZ(float2 uv, float sss)
	{
		float3 displacement = tex2Dlod(_Map0, float4(((uv+ 0.5 / _TexSize)* 0.5 + 0.5) * sss , 0, 0)).zwx;

		// half4 c = tex2Dlod(_Map0, float4((uv* 0.5 + 0.5) * sss , 0, 0));
		// half4 c2 = tex2Dlod(_Map0, float4((uv* 0.5 + 0.5) * sss * 2, 0, 0)) * 0.5;
		// half4 c3 = tex2Dlod(_Map0, float4((uv* 0.5 + 0.5) * sss * 4, 0, 0)) * 0.25;

		// float3 displacement = c.zwx + c2.zwx + c3.zwx;
		// displacement.z += c.y + c2.y + c3.y;

		return displacement;
	}
	
	v2f_caustic vert_caustic(appdata_caustic v)
	{
		v2f_caustic o = (v2f_caustic)0;
		
		float3 vertexPos = float3(v.vertex.xyz);
		o.oldPos = vertexPos;

		float3 displacement = ComputeDisplacementXZ(v.vertex.xy,scale * 0.995);//0.997
		float3 displacement1 = ComputeDisplacementXZ(v.vertex.xy,scale);
		float3 displacement2 = ComputeDisplacementXZ(v.vertex.xy,scale * 1.005);//1.003

		// Smooth edge transition instead of hard boundary
		float edgeFadeX = smoothstep(0.0, 0.05, v.uv.x) * smoothstep(0.0, 0.05, 1.0 - v.uv.x);
		float edgeFadeY = smoothstep(0.0, 0.05, v.uv.y) * smoothstep(0.0, 0.05, 1.0 - v.uv.y);
		float edgeFade = 1;//float edgeFade = edgeFadeX * edgeFadeY;

		v.vertex.xyz += displacement1 * _DisplaceScale * edgeFade;

		o.newPos = vertexPos + displacement * _IorR * edgeFade;
		o.newPos2 = vertexPos + displacement1 * _IorG * edgeFade;
		o.newPos3 = vertexPos + displacement2 * _IorB * edgeFade;

		o.vertex = float4(v.vertex.xy, 0, 0.5);
		o.uv = v.uv;
		return o;
	}

	half4 frag_caustic(v2f_caustic i) : SV_Target
	{
		float oldArea = length(ddx(i.oldPos)) * length(ddy(i.oldPos));
		float newArea = length(ddx(i.newPos)) * length(ddy(i.newPos));
		float newArea2 = length(ddx(i.newPos2)) * length(ddy(i.newPos2));
		float newArea3 = length(ddx(i.newPos3)) * length(ddy(i.newPos3));

		// float2 oldDx = ddx(i.oldPos.xy);
		// float2 oldDy = ddy(i.oldPos.xy);
		// float2 newDx = ddx(i.newPos.xy);
		// float2 newDy = ddy(i.newPos.xy);

		// float oldArea = abs(oldDx.x * oldDy.y - oldDx.y * oldDy.x);
		// float newArea = abs(newDx.x * newDy.y - newDx.y * newDy.x);

		float color = oldArea / newArea * 0.2;
		float color2 = oldArea / newArea2 * 0.2;
		float color3 = oldArea / newArea3 * 0.2;

		return float4(color, color2, color3, max(max(color,color2),color3));


		//return tex2Dlod(_Map0, float4(i.uv.xy, 0, 0));
	}

		ENDHLSL

	SubShader
	{
		//Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		
		Blend One One
		ZWrite Off
		ZTest Always
		Cull Off

		Pass
		{
			HLSLPROGRAM
			
			#pragma vertex vert_caustic
			#pragma fragment frag_caustic

			ENDHLSL
		}
	}
}
