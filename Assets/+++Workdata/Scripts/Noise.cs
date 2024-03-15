using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Mathf;
using Random = UnityEngine.Random;


public static class Noise
{
    static float WhiteNoise(Vector2 p)
    {
        float value = Sin(Vector2.Dot(p, new Vector2(12.9898f, 78.233f))) * 43758.5453f;
        return value - Floor(value);
    }

    #region Value Noise

    static float ValueNoise(float x, float y)
    {
        Vector2 lv = new Vector2(x - Floor(x), y - Floor(y));
        Vector2 id = new Vector2(Floor(x), Floor(y));

        lv = lv * lv * (Vector2.one * 3f - 2f * lv);

        float botLeft = WhiteNoise(id);
        float botRight = WhiteNoise(id + new Vector2(1, 0));
        float b = Lerp(botLeft, botRight, lv.x);

        float topLeft = WhiteNoise(id + new Vector2(0, 1));
        float topRight = WhiteNoise(id + new Vector2(1, 1));
        float t = Lerp(topLeft, topRight, lv.x);

        return Lerp(b, t, lv.y);
    }


    public static float[,] ValueNoise2D(int width, int height, int seed, Vector2 offset, Vector2 XYScale,
        NoiseSettings noiseSettings)
    {
        float[,] valueMap = new float[width, height];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[noiseSettings.octaves];
        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoise = float.MinValue;
        float minNoise = float.MaxValue;

        float halfHeight = height / 2f;
        float halfWidth = width / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noiseHeight = 0f;
                float amplitude = 0.5f;
                float frequency = 1f;

                for (int i = 0; i < noiseSettings.octaves; i++)
                {
                    float sampleX = ((x - halfWidth) / noiseSettings.scale - octaveOffsets[i].x) * frequency;
                    float sampleY = ((y - halfHeight) / noiseSettings.scale - octaveOffsets[i].y) * frequency;

                    float value = ValueNoise(sampleX * XYScale.x, sampleY * XYScale.y) * 2 - 1;

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

                valueMap[x, y] = noiseHeight * noiseSettings.brightness;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
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

    public static float[,] PerlinNoise2D(int width, int height, int seed, Vector2 offset, Vector2 XYScale,
        NoiseSettings noiseSettings)
    {
        float[,] noiseMap = new float[width, height];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[noiseSettings.octaves];
        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < noiseSettings.octaves; i++)
                {
                    float sampleX = ((x - halfWidth) / noiseSettings.scale * XYScale.x - octaveOffsets[i].x) *
                                    frequency;
                    float sampleY = ((y - halfHeight) / noiseSettings.scale * XYScale.y - octaveOffsets[i].y) *
                                    frequency;

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

                noiseMap[x, y] = noiseHeight * noiseSettings.brightness;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
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

    public static float VoronoiNoise(float width, float height, float randomness)
    {
        float dis = 1f;

        Vector2 n = new Vector2(Floor(width), Floor(height));
        Vector2 f = new Vector2(width - n.x, height - n.y);

        for (int j = -1; j <= 1; j++)
        {
            for (int i = -1; i <= 1; i++)
            {
                Vector2 o = new Vector2(WhiteNoise(n + new Vector2(i, j)), WhiteNoise(n + new Vector2(i, j)));
                o = new Vector2(0.5f + 0.5f * Sin(6.2831f * o.x), 0.5f + 0.5f * Sin(6.2831f * o.y)) / randomness;
                Vector2 diff = new Vector2(i, j) + o - f;
                float d = Sqrt(diff.x * diff.x + diff.y * diff.y);
                dis = Min(dis, d);
            }
        }

        return dis;
    }


    public static float[,] VoronoiNoise2D(int width, int height, int seed, Vector2 offset, Vector2 XYScale,
        NoiseSettings noiseSettings)
    {
        float[,] voronoiMap = new float[width, height];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[noiseSettings.octaves];
        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoise = float.MinValue;
        float minNoise = float.MaxValue;

        float halfHeight = height / 2f;
        float halfWidth = width / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noiseHeight = 0f;
                float amplitude = 0.5f;
                float frequency = 1f;

                for (int i = 0; i < noiseSettings.octaves; i++)
                {
                    float sampleX = ((x - halfWidth) / noiseSettings.scale - octaveOffsets[i].x) * frequency;
                    float sampleY = ((y - halfHeight) / noiseSettings.scale - octaveOffsets[i].y) * frequency;

                    float voronoiValue =
                        VoronoiNoise(sampleX * XYScale.x, sampleY * XYScale.y, noiseSettings.randomness) * 2 - 1;

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
                    ? 1 - noiseHeight * noiseSettings.brightness
                    : noiseHeight * noiseSettings.brightness;
            }
        }

        return voronoiMap;
    }

    #endregion
}