using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Mathf;
using Random = UnityEngine.Random;

public static class OldNoise
{
    #region Value Noise

    public static float[,] ValueNoise2D(NoiseSettings noiseSettings)
    {
        float[,] valueMap = new float[noiseSettings.width, noiseSettings.height];

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

        for (int y = 0; y < noiseSettings.height; y++)
        {
            for (int x = 0; x < noiseSettings.width; x++)
            {
                float noiseHeight = 0f;
                float amplitude = 0.5f;
                float frequency = 1f;

                for (int i = 0; i < noiseSettings.octaves; i++)
                {
                    float sampleX = ((x - halfWidth) / noiseSettings.scale + octaveOffsets[i].x) * frequency;
                    float sampleY = ((y - halfHeight) / noiseSettings.scale - octaveOffsets[i].y) * frequency;

                    float value = Noise.ValueNoise(sampleX, sampleY, noiseSettings.randomness) * 2 - 1;

                    if (noiseSettings.turbulence == 1)
                    {
                        value = value;
                    }
                    else
                    {
                        //TURBULENCE
                        value = Abs(value);

                        value = Pow(value, 1f / (noiseSettings.crease + 1));
                    }

                    noiseHeight += value * amplitude;

                    amplitude *= noiseSettings.persistence;
                    frequency *= noiseSettings.lacunarity;
                }

                if (noiseHeight > maxNoise)
                    maxNoise = noiseHeight;
                if (noiseHeight < minNoise)
                    minNoise = noiseHeight;

                valueMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < noiseSettings.height; y++)
        {
            for (int x = 0; x < noiseSettings.width; x++)
            {
                valueMap[x, y] = noiseSettings.invert
                    ? 1 - InverseLerp(minNoise, maxNoise, valueMap[x, y])
                    : InverseLerp(minNoise, maxNoise, valueMap[x, y]);
            }
        }

        return valueMap;
    }

    #endregion

    #region Perlin Noise

    public static float[,] PerlinNoise2D(NoiseSettings noiseSettings)
    {
        float[,] noiseMap = new float[noiseSettings.width, noiseSettings.height];

        System.Random prng = new System.Random(noiseSettings.seed);
        Vector2[] octaveOffsets = new Vector2[noiseSettings.octaves];
        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + noiseSettings.offset.x;
            float offsetY = prng.Next(-100000, 100000) - noiseSettings.offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = noiseSettings.width / 2f;
        float halfHeight = noiseSettings.height / 2f;


        for (int y = 0; y < noiseSettings.height; y++)
        {
            for (int x = 0; x < noiseSettings.width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;


                for (int i = 0; i < noiseSettings.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / noiseSettings.scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / noiseSettings.scale * frequency;

                    float perlinValue = PerlinNoise(sampleX, sampleY) * 2 - 1;

                    if (noiseSettings.turbulence == 1)
                    {
                        perlinValue = perlinValue;

                        perlinValue = Pow(perlinValue, 1f / (noiseSettings.crease + 1));
                    }
                    else
                    {
                        //TURBULENCE
                        perlinValue = Abs(perlinValue);

                        perlinValue = Pow(perlinValue, 1f / (noiseSettings.crease + 1));
                    }

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= noiseSettings.persistence;
                    frequency *= noiseSettings.lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }

                if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < noiseSettings.height; y++)
        {
            for (int x = 0; x < noiseSettings.width; x++)
            {
                noiseMap[x, y] = noiseSettings.invert
                    ? 1 - InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y])
                    : InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    #endregion

    #region Voronoi Noise

    public static float[,] VoronoiNoise2D(NoiseSettings noiseSettings)
    {
        float[,] voronoiMap = new float[noiseSettings.width, noiseSettings.height];

        System.Random prng = new System.Random(noiseSettings.seed);
        Vector2[] octaveOffsets = new Vector2[noiseSettings.octaves];
        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + noiseSettings.offset.x;
            float offsetY = prng.Next(-100000, 100000) - noiseSettings.offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoise = float.MinValue;
        float minNoise = float.MaxValue;

        float halfHeight = noiseSettings.height / 2f;
        float halfWidth = noiseSettings.width / 2f;

        for (int y = 0; y < noiseSettings.height; y++)
        {
            for (int x = 0; x < noiseSettings.width; x++)
            {
                float noiseHeight = 0f;
                float amplitude = 0.5f;
                float frequency = 1f;

                for (int i = 0; i < noiseSettings.octaves; i++)
                {
                    float sampleX = ((x - halfWidth) / noiseSettings.scale - octaveOffsets[i].x) * frequency;
                    float sampleY = ((y - halfHeight) / noiseSettings.scale - octaveOffsets[i].y) * frequency;

                    float voronoiValue =
                        Noise.VoronoiNoise(sampleX * noiseSettings.xyScale.x, sampleY * noiseSettings.xyScale.y, noiseSettings.randomness) * 2 - 1;

                    if (noiseSettings.turbulence == 1)
                    {
                        voronoiValue = voronoiValue;
                    }
                    else if (noiseSettings.turbulence == 2)
                    {
                        //TURBULENCE
                        voronoiValue = Abs(voronoiValue);

                        voronoiValue = Pow(voronoiValue, 1f / (noiseSettings.crease + 1));
                    }

                    noiseHeight += voronoiValue * amplitude;

                    amplitude *= noiseSettings.persistence;
                    frequency *= noiseSettings.lacunarity;
                }

                if (noiseHeight > maxNoise)
                    maxNoise = noiseHeight;
                if (noiseHeight < minNoise)
                    minNoise = noiseHeight;

                voronoiMap[x, y] = noiseSettings.invert
                    ? 1 - noiseHeight
                    : noiseHeight;
            }
        }

        return voronoiMap;
    }

    #endregion
}