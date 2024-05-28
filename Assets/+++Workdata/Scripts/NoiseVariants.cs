using System;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.Mathf;
using Random = UnityEngine.Random;

public class NoiseVariants
{
    static float Turbulence(float value, NoiseSettings noiseSettings)
    {
        if (noiseSettings.turbulence == 1)
            return value;

        value = Abs(value);
        return Pow(value, 1f / (noiseSettings.crease + 1));
    }

    static Vector2[] Seed(NoiseSettings noiseSettings)
    {
        System.Random prng = new System.Random(noiseSettings.seed);
        Vector2[] seed = new Vector2[noiseSettings.octaves];
        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + noiseSettings.offset.x;
            float offsetY = prng.Next(-100000, 100000) + noiseSettings.offset.y;
            seed[i] = new Vector2(offsetX, offsetY);
        }

        return seed;
    }

    public enum NormalizeMode
    {
        Local,
        Global
    }

    public static float[,] Normalize(float[,] noiseMap, NoiseSettings noiseSettings, int width, int height)
    {
        float maxNoise = float.MinValue;
        float minNoise = float.MaxValue;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (noiseMap[x, y] > maxNoise) maxNoise = noiseMap[x, y];
                if (noiseMap[x, y] < minNoise) minNoise = noiseMap[x, y];
            }
        }

        float[,] remappedMap = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (noiseSettings.normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / 2f;
                    remappedMap[x, y] = noiseSettings.invert ? 1 - Clamp(normalizedHeight, 0, int.MaxValue) : Clamp(normalizedHeight, 0, int.MaxValue);
                    //Debug.Log(remappedMap[x,y]);
                }
                else
                {
                    float normalizedHeight = InverseLerp(minNoise, maxNoise, noiseMap[x, y]);
                    remappedMap[x, y] = noiseSettings.invert ? 1 - normalizedHeight : normalizedHeight;
                    //Debug.Log(remappedMap[x,y]);
                }
            }
        }

        return remappedMap;
    }

    public static float Fbm(int x, int y, NoiseSettings noiseSettings, int width, int height)
    {
        Vector2[] seed = Seed(noiseSettings);
        float noiseHeight = 0f;

        noiseHeight = 0f;
        float amplitude = 0.5f;
        float frequency = 1f;

        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            float sampleX = (x - (width / 2f) + seed[i].x) * noiseSettings.xyScale.x /
                noiseSettings.scale * frequency;
            float sampleY = (y - (height / 2f) - seed[i].y) * noiseSettings.xyScale.y /
                noiseSettings.scale * frequency;

            float value = 0f;

            if (noiseSettings.noiseType == NoiseSettings.NoiseType.ValueNoise2D)
                value = Noise.ValueNoise(sampleX, sampleY, noiseSettings.randomness) * 2 - 1;

            if (noiseSettings.noiseType == NoiseSettings.NoiseType.PerlinNoise2D)
                value = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

            if (noiseSettings.noiseType == NoiseSettings.NoiseType.VoronoiNoise2D)
                value = Noise.VoronoiNoise(sampleX, sampleY, noiseSettings.randomness) * 2 - 1;

            value = Turbulence(value, noiseSettings);

            noiseHeight += value * amplitude;

            amplitude *= noiseSettings.persistence;
            frequency *= noiseSettings.lacunarity;
        }

        return noiseHeight;
    }

    public static float[,] Start(NoiseSettings noiseSettings, float strength, int width, int height)
    {
        float[,] map = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noiseValue = Fbm(x, y, noiseSettings, width, height);
                if(noiseSettings.roundUp) 
                    noiseValue = Round(noiseValue * noiseSettings.roundTo) * noiseSettings.roundStrength;
                map[x, y] = noiseValue * strength;
                if (noiseSettings.curve)
                    map[x, y] = noiseSettings.animationCurve.Evaluate(map[x, y]);
            }
        }

        return Normalize(map, noiseSettings, width, height);
    }
}