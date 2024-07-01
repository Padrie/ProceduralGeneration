using UnityEngine;
using System;

[Serializable]
public class NoiseSettings
{
    public enum NoiseType
    {
        ValueNoise2D,
        PerlinNoise2D,
        VoronoiNoise2D
    }

    public NoiseType noiseType = NoiseType.PerlinNoise2D; // Used to select different Noise Type

    public int seed = 0;
    [Range(1, 30f)] public float randomness = 1f; // Only used by Voronoi Noise. Controls the randomness of the "Balls"
    [Min(0.0001f)] public float scale = 100f; // Scale of the noise
    [Range(1, 6)] public int octaves = 5; // Detail of the Noise
    [Range(1, 5)] public float lacunarity = 2f; // Frequency of Noise
    [Range(0f, 1f)] public float persistence = 0.4f; // Mix of Octaves and lacunarity

    [Space(5), Range(1, 2)] public int turbulence = 1; // Absolute value of the noise, does sharp mountain ridges
    [Range(0, 5)] public float crease = 1f; // Control over sharp mountain ridges
    public bool invert = false; // Inverts the noise
    [Space(10), Min(.1f)] public Vector2 xyScale = new Vector2(1f, 1f); // Scales the noise on the X and Y
    [HideInInspector] public Vector2 offset = new Vector2(149, -149); // Offset, used to make seamless chunks
    [Space(10)] public bool roundUp; // If true, uses roundTo
    [Range(1, 100)] public int roundTo = 10; // Rounds up every value
    [Range(0.01f, 1f)] public float roundStrength = 0.1f; // Controls the height of the rounded values
    [Space(5)] public bool curve; // If true, uses animaionCurve
    public AnimationCurve animationCurve; // Control over terrain with an Animation Curve
}
