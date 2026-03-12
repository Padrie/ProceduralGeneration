# ProceduralGeneration

![Header](https://img.itch.zone/aW1nLzIwMjE1MzI4LnBuZw==/original/leuAwS.png)

- I've always wanted to make an endless World, with interesting landmarks to explore and complex environments, and I didn't quite reach it with this project, as it's the first time I've even tackled any kind of procedural generation, but it was a great learning experience that wouldn't have been possible without [Sebastian Lague's](https://www.youtube.com/@SebastianLague) Procedural Terrain Generation youtube series and [Catlike Codings Pseudorandom Noise](https://catlikecoding.com/unity/tutorials/pseudorandom-noise/) blog. This was a very fun and enjoyable project that I definetly want to revisit some day.

<h2 align="left">Features</h2>

<h3 align="left">Terrain Generation and Noise</h3>

- 3 different Noise types (Perlin, Voronoi and Value) that can yield different kinds of terrain
- typical noise settings like Octaves, Lacunarity, Persistence and Turbulence, but also Crease that makes terrain sharper
- Noise Settings are saveable as Scriptable Objects
- limited Chunk generation
- terrain adjusts real time if settings are changed

<h3 align="left">Asset Placement tool</h3>

- A tool to place assets on your procedural generated terrain
- Ability to spawn assets randomly or with a noise map
- Randomly selects asset in an array if multiple are present
- Adjustable alignment of asset on the normals of the terrain
- Gizmo visualization to roughly see the result

<h2 align="left">Contributions</h2>

This project was made in a team of 8, while on the engineering side we had 2 people, my teammate [Tom](https://github.com/Tom-Schueler), who was resposible checkpoint and saving logic, and general gameplay, and me, for gravity manipulation, player movement and special obstacles.

<h2 align="left">My Responsibility</h2>

<h3 align="left">Gravity Manipulation</h3>
