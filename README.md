# ProceduralGeneration

![Header](https://cdn.discordapp.com/attachments/1051801782818451479/1481527589435867216/Scatter.PNG?ex=69b3a396&is=69b25216&hm=0b5514dbd767b63af8e91b6858b184a356b52c3d09fa1baa04e4761e5b671227)

I've always wanted to make an endless World, with interesting landmarks to explore and complex environments, and I didn't quite reach it with this project, as it's the first time I've even tackled any kind of procedural generation, but it was a great learning experience that wouldn't have been possible without [Sebastian Lague's](https://www.youtube.com/@SebastianLague) Procedural Terrain Generation youtube series and [Catlike Codings Pseudorandom Noise](https://catlikecoding.com/unity/tutorials/pseudorandom-noise/) blog. This was a very fun and enjoyable project that I definetly want to revisit some day.

<h2 align="left">Features</h2>

<h3 align="left">Terrain Generation and Noise</h3>

- 3 different Noise types (Perlin, Voronoi and Value) that can yield different kinds of terrain
- typical noise settings like Octaves, Lacunarity, Persistence and Turbulence, but also Crease that makes terrain sharper
- Noise Settings are saveable as Scriptable Objects
- limited Chunk generation
- terrain adjusts real time if settings are changed

![Header](https://cdn.discordapp.com/attachments/1051801782818451479/1481529133862354994/1.gif?ex=69b3a506&is=69b25386&hm=a6754c64f94a445b2fb728b23e372cc52fb76784a6d2708136e77d72cf3c5e87)

<h3 align="left">Asset Placement tool</h3>

- A tool to place assets on your procedural generated terrain
- Ability to spawn assets randomly or with a noise map
- Randomly selects asset in an array if multiple are present
- Adjustable alignment of asset on the normals of the terrain
- Adjustable slider to spawn assets on flat or steep terrain
- Gizmo visualization to roughly see the result

![Header](https://cdn.discordapp.com/attachments/1051801782818451479/1481528863812223066/TerrainAssetPlacement.png?ex=69b3a4c6&is=69b25346&hm=377cf0a980cc39cef8da4647b964617ee4c4d884fb5c662e35ae5f2f5c862cf8)
