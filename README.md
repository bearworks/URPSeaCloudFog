# URPSeaCloudFog

This is an sea&cloud&sky integrated project for height based & atomspheric fog rendering for URP (Unity 2021.3+ and Custom URP 12.1.10).

![](./Image/URPSeaCloudFog.png)
<p align="center">Atomspheric Fog (Height based)</p>

![](./Image/URPSeaCloudFog1.png)
![](./Image/URPSeaCloudFog2.png)

<p align="center">Height based & Sky Fog</p>

![](./Image/URPSeaCloudFog3.png)

<p align="center">Now supports Unity 2021.3</p>

## Useage
1.To use AtmosFogNode shadersubgraph in the object's shadergraph as follow:

![](./Image/URPSeaCloudFog5.png)

2.To use MixAtmosFog(inout half3 color, float3 worldPos) to object's shader instead of URP's fog builtin-shader code.

For example, in the file TerrainLitPasses.hlsl, to change code as following:  

color.rgb = MixFog(color.rgb, fogCoord); -> MixAtmosFog(color.rgb, worldPos); // need worldPos

![](./Image/URPSeaCloudFog4.png)

Currently, there is an unlit shadergraph in this repository that uses the AtmosFogNode (as shown in the above figure) as the material for the cube.

Reference:  
http://advances.realtimerendering.com/s2017/DecimaSiggraph2017.pdf  
https://github.com/bearworks/URPSeaCloud
