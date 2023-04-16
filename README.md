# URPSeaCloudFog

This is an sea&cloud&sky integrated project for height based & atomspheric fog rendering for URP (Unity 2020.3+ and Custom URP 10.6).

![](./Image/URPSeaCloudFog.png)
<p align="center">Atomspheric Fog (Height based)</p>

![](./Image/URPSeaCloudFog1.png)
![](./Image/URPSeaCloudFog2.png)

<p align="center">Height based & Sky Fog</p>

![](./Image/URPSeaCloudFog3.png)

<p align="center">Now supports Unity 2021.3 or newer</p>

![](./Image/URPSeaCloudFog4.png)

## Useage

![](./Image/URPSeaCloudFog5.png)

To use MixAtmosFog(inout half3 color, float3 worldPos) to object's shader instead of URP's fog builtin-shader code.

For example, in TerrainLitPasses.hlsl change code as following:  

color.rgb = MixFog(color.rgb, fogCoord); -> MixAtmosFog(color.rgb, worldPos); // need worldPos

There is currently No AtmosFog Shadergraph node in this repository.

Reference:  
http://advances.realtimerendering.com/s2017/DecimaSiggraph2017.pdf  
https://github.com/bearworks/URPSeaCloud
