using UnityEngine;
using static UnityEngine.Mathf;
using Random = UnityEngine.Random;

public class PerlinNoise
{
    public static float[,] PerlinNoise2D(NoiseSettings noiseSettings)
    {
        float[,] map = new float[noiseSettings.width, noiseSettings.height];

        map = NoiseVariants.Fbm(noiseSettings);

        return map;
    }
}