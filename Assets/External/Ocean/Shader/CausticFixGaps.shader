Shader "Hidden/URPOcean/CausticFixGaps"
{
	Properties
	{
		_MainTex ("Base", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
				ZTest Always Cull Off ZWrite Off
				Fog{ Mode off }

				HLSLPROGRAM
				#include "NeoInclude.hlsl"
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest

				struct v2f
				{
					float4  pos : SV_POSITION;
					float2  uv : TEXCOORD0;
				};

				v2f vert(appdata_base v)
				{
					v2f OUT;
					OUT.pos = UnityObjectToClipPos(v.vertex);
					OUT.uv = v.texcoord.xy;
					return OUT;
				}

				TEXTURE2D_X(_MainTex);
				half4 _MainTex_TexelSize;	

				float4 frag(v2f IN) : SV_Target
				{
					float2 outputUV = IN.uv.xy;

					// Map output UV (0-1) to the middle 2x2 region's position within the 4x4 grid
					// The middle 2x2 spans (1/4, 1/4) to (3/4, 3/4) in the 4x4 grid
					float2 sourceUV = outputUV * 0.5 + 0.25; //0.125 0.25
					
					half4 colorA = SAMPLE_TEXTURE2D_X(_MainTex, sampler_PointClamp, sourceUV);

					return colorA;
				}

				ENDHLSL
			}
		}
		Fallback off
}
