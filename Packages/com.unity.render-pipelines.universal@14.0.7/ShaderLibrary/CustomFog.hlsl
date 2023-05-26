#ifndef CUSTOMFOG_HLSL_INCLUDED
#define CUSTOMFOG_HLSL_INCLUDED

#if defined(FOG_LINEAR)

    float4 _HeightFogParams;

    float4 _BetaRsMs;
    float _BetaMa;
    float mMieAsymmetry;
    float3 mAlbedoR;
    float3 mAlbedoM;
    float3 mSunColor;
    float3 mAmbColor;

    #define SKY_GROUND_THRESHOLD 0.02

    float Rayleigh(float mu)
    {
        return 0.75 * (1.0 + mu * mu);
    }

    float Mie(float mu, float g)
    {
        float g2 = g * g;
        return (1.0 - g2) / (pow(1.0+g2 - 2.0*g*mu,1.5));
    }

    half3 SkyFog(half3 color, float worldPosY, float3 ray, float distance)
    {
        float3 beta_t = _BetaRsMs.xyz + _BetaRsMs.w + _BetaMa;

        float offsetYPos = (worldPosY - _HeightFogParams.z);
        float range = _HeightFogParams.y;//* 30 / SKY_GROUND_THRESHOLD;
        float t = max(1e-4, (_WorldSpaceCameraPos.y - offsetYPos) / max(range, 1e-6));
        t = (1 - exp(-t)) / t * exp(-offsetYPos / max(range, 1e-6));

        float3 extinction = exp(-distance * _HeightFogParams.x * 1e-5 * beta_t * t);

        float inSov = dot(ray, _MainLightPosition.xyz);
        float3 single_r = mAlbedoR * _BetaRsMs.xyz * Rayleigh(inSov);
        float3 single_m = mAlbedoM * _BetaRsMs.w * Mie(inSov, mMieAsymmetry);
        float3 inscatter = mSunColor * (single_r + single_m) / (4.0 * 3.14159);
        inscatter += mAmbColor * (_BetaRsMs.xyz + _BetaRsMs.w);
        inscatter /= beta_t;

        return color * extinction + inscatter * (1 - extinction);
    }

    void MixAtmosFog(inout half3 color, float3 worldPos)
    {
        float3 dist = _WorldSpaceCameraPos.xyz - worldPos.xyz;
        float len = length(dist);

        color = SkyFog(color, worldPos.y, normalize(-dist), len);
    }
    
    void MixAtmosFog_float(half4 color, float3 worldPos, out half4 Out)
    {
        float3 dist = _WorldSpaceCameraPos.xyz - worldPos.xyz;
        float len = length(dist);

        Out.rgb = SkyFog(color.rgb, worldPos.y, normalize(-dist), len);
        Out.a = color.a;
    }

    half3 SkyFogColor(half3 color, float worldPosY, float3 ray, float distance, float3 inscatter)
    {
        float3 beta_t = _BetaRsMs.xyz + _BetaRsMs.w + _BetaMa;

        float offsetYPos = (worldPosY - _HeightFogParams.z);
        float range = _HeightFogParams.y;//* 30 / SKY_GROUND_THRESHOLD;
        float t = max(1e-4, (_WorldSpaceCameraPos.y - offsetYPos) / max(range, 1e-6));
        t = (1 - exp(-t)) / t * exp(-offsetYPos / max(range, 1e-6));

        float3 extinction = exp(-distance * _HeightFogParams.x * 1e-5 * beta_t * t);

        return color * extinction + inscatter * (1 - extinction);
    }

    void MixAtmosFogColor(inout half3 color, float3 worldPos, float3 inscatter)
    {
        float3 dist = _WorldSpaceCameraPos.xyz - worldPos.xyz;
        float len = length(dist);

        color = SkyFogColor(color, worldPos.y, normalize(-dist), len, inscatter);
    }

#else
    void MixAtmosFog(inout half3 color, float3 worldPos) {}
    void MixAtmosFog_float(half4 color, float3 worldPos, out half4 Out) 
    {
        Out = color;
    }
    void MixAtmosFogColor(inout half3 color, float3 worldPos, float3 inscatter) {}
#endif

#endif
