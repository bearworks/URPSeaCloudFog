# URPSeaCloudFog

This is an sea&cloud&sky integrated project for height based & atomspheric fog rendering for URP (Unity 2020.3+ and Custom URP 10.6).

![](./Image/URPSeaCloudFog.png)
<p align="center">Atomspheric Fog (Height based)</p>

![](./Image/URPSeaCloudFog1.png)
![](./Image/URPSeaCloudFog2.png)

<p align="center">Height based & Sky Fog</p>

![](./Image/URPSeaCloudFog3.png)

<p align="center">Now supports Unity 2021.3 or newer</p>

## Useage
1.To use AtmosFogNode Shadersubgraph in the object's Shadergraph as follow:

![](./Image/URPSeaCloudFog5.png)

2.To use MixAtmosFog(inout half3 color, float3 worldPos) to object's shader instead of URP's fog builtin-shader code.

For example, in the file TerrainLitPasses.hlsl, to change code as following:  

color.rgb = MixFog(color.rgb, fogCoord); -> MixAtmosFog(color.rgb, worldPos); // need worldPos

![](./Image/URPSeaCloudFog4.png)

There is a Unlit shadergraph currently that uses AtmosFogNode (as the cube material in above figure) in this repository.

Reference:  
http://advances.realtimerendering.com/s2017/DecimaSiggraph2017.pdf  
https://github.com/bearworks/URPSeaCloud
