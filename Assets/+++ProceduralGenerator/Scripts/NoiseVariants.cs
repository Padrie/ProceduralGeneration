using System;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.Mathf;
using static PadrieExtension;

public class NoiseVariants
{
    // returns the Abs of value, and the power of crease, giving control over sharp mountain ridges
    static float Turbulence(float value, NoiseSettings noiseSettings)
    {
        return IfSwitch(noiseSettings.turbulence - 1, value, Pow(Abs(value), 1f / (noiseSettings.crease + 1)));
    }

    // Seed of the noise, always being the same
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

    // Normalizes the Noise Map to have seamless chunks
    public static float[,] Normalize(float[,] noiseMap, NoiseSettings noiseSettings, int width, int height)
    {
        float minHeight = MapDisplay.Instance.heightClamp.x; 
        float maxHeight = MapDisplay.Instance.heightClamp.y;

        float[,] remappedMap = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedHeight = (noiseMap[y, x] + 1) / 2f; // centralizes Noise Map
                // Clamps the Noise maps values to never be above or below minHeight or maxHeight
                remappedMap[y, x] = noiseSettings.invert ? 1 - Clamp(normalizedHeight, minHeight, maxHeight) : Clamp(normalizedHeight, minHeight, maxHeight);
            }
        }

        return remappedMap;
    }

    public static float Fbm(int x, int y, int width, int height, NoiseSettings noiseSettings)
    {
        Vector2[] seed = Seed(noiseSettings);

        float noiseHeight = 0f;
        float amplitude = 1f;
        float frequency = 1f;

        for (int i = 0; i < noiseSettings.octaves; i++) // Octaves is the number of noise maps generated
        {
            float sampleX = (x - width / 2f + seed[i].x) * noiseSettings.xyScale.x / noiseSettings.scale * frequency; // Frequency makes the noise map smaller, i.e. more noisy
            float sampleY = (y - height / 2f - seed[i].y) * noiseSettings.xyScale.y / noiseSettings.scale * frequency;

            float value = 0f;

            if (noiseSettings.noiseType == NoiseSettings.NoiseType.ValueNoise2D)
                value = Noise.ValueNoise(sampleX, sampleY) * 2 - 1; // Noise Function

            if (noiseSettings.noiseType == NoiseSettings.NoiseType.PerlinNoise2D)
                value = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // Noise Function

            if (noiseSettings.noiseType == NoiseSettings.NoiseType.VoronoiNoise2D)
                value = Noise.VoronoiNoise(sampleX, sampleY, noiseSettings.randomness) * 2 - 1; // Noise Function

            value = Turbulence(value, noiseSettings);

            noiseHeight += value * amplitude; // Amplitude does the opposite of frequency, i.e. less noisy

            amplitude *= noiseSettings.persistence;
            frequency *= noiseSettings.lacunarity;
        }

        return noiseHeight;
    }

    // Start is used to combine the methods and finally make the noise Map
    public static float[,] Start(NoiseSettings noiseSettings, float strength, int width, int height)
    {
        float[,] map = new float[width, height]; // Create 2D float array with size of width and height

        // Nested for loop of width and height
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noiseValue = Fbm(x, y, width, height, noiseSettings) * strength; // Creating the Noise Value through the FBM above, and multiplying it by strength
                if (noiseSettings.roundUp)
                    noiseValue = Round(noiseValue * noiseSettings.roundTo) * noiseSettings.roundStrength; // This rounds the values to a specified value
                map[x, y] = noiseValue;
                map[x, y] *= MapDisplay.Instance.noiseHeightMultiplier; // This controls the height of the terrain

                if (noiseSettings.curve)
                    map[x, y] = noiseSettings.animationCurve.Evaluate(map[x, y]);
            }
        }

        return Normalize(map, noiseSettings, width, height); // uses the Normalize method above
    }
}