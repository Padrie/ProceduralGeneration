using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.Mathf;
using Random = UnityEngine.Random;

public class NoiseVariants
{

    static float Turbulence(float value, NoiseSettings noiseSettings)
    {
        if (noiseSettings.turbulence == 1)
            value = value;
        else
        {
            value = Abs(value);
            value = Pow(value, 1f / (noiseSettings.crease + 1));
        }

        return value;
    }

    public enum NormalizeMode
    {
        Local,
        Global
    }

    public static float[,] Fbm(NoiseSettings noiseSettings)
    {
        float[,] map = new float[noiseSettings.width, noiseSettings.height];
        
        System.Random prng = new System.Random(noiseSettings.seed);
        Vector2[] octaveOffsets = new Vector2[noiseSettings.octaves];
        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + noiseSettings.offset.x;
            float offsetY = prng.Next(-100000, 100000) + noiseSettings.offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoise = float.MinValue;
        float minNoise = float.MaxValue;

        float halfHeight = noiseSettings.height / 2f;
        float halfWidth = noiseSettings.width / 2f;

        float maxPossibleHeight = 0f;

        for (int y = 0; y < noiseSettings.height; y++)
        {
            for (int x = 0; x < noiseSettings.width; x++)
            {
                float noiseHeight = 0f;
                float amplitude = 0.5f;
                float frequency = 1f;

                for (int i = 0; i < noiseSettings.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) * noiseSettings.xyScale.x /
                        noiseSettings.scale * frequency;
                    float sampleY = ((y - halfHeight - octaveOffsets[i].y) * noiseSettings.xyScale.y /
                                     noiseSettings.scale) * frequency;

                    float value = 0f;

                    if (noiseSettings.noiseType == NoiseSettings.NoiseType.ValueNoise2D)
                        value = Noise.ValueNoise(sampleX, sampleY, noiseSettings.randomness) * 2 - 1;

                    if (noiseSettings.noiseType == NoiseSettings.NoiseType.PerlinNoise2D)
                        value = PerlinNoise(sampleX, sampleY) * 2 - 1;

                    if (noiseSettings.noiseType == NoiseSettings.NoiseType.VoronoiNoise2D)
                        value = Noise.VoronoiNoise(sampleX, sampleY, noiseSettings.randomness) * 2 - 1;

                    if (noiseSettings.noiseType == NoiseSettings.NoiseType.Mix)
                    {
                        var a = Noise.ValueNoise(sampleX, sampleY, noiseSettings.randomness) * 2 - 1;
                        value = (PerlinNoise(sampleX, sampleY) * 2 - 1) - a - a;
                    }


                    value = Turbulence(value, noiseSettings);

                    noiseHeight += value * amplitude;
                    maxPossibleHeight += amplitude;

                    amplitude *= noiseSettings.persistence;
                    frequency *= noiseSettings.lacunarity;
                }

                if (noiseHeight > maxNoise)
                    maxNoise = noiseHeight;
                if (noiseHeight < minNoise)
                    minNoise = noiseHeight;

                map[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < noiseSettings.height; y++)
        {
            for (int x = 0; x < noiseSettings.width; x++)
            {
                if (noiseSettings.normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (map[x, y] + 1) / (maxPossibleHeight / 0.9f) * 10000;
                    map[x, y] = noiseSettings.invert
                        ? 1 - Clamp(normalizedHeight, 0, int.MaxValue)
                        : Clamp(normalizedHeight, 0, int.MaxValue);
                }
                else
                {
                    map[x, y] = noiseSettings.invert
                        ? 1 - InverseLerp(minNoise, maxNoise, map[x, y])
                        : InverseLerp(minNoise, maxNoise, map[x, y]);
                }
            }
        }

        return map;
    }
}