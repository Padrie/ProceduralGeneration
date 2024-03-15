using System;
using UnityEngine;
using static UnityEngine.Mathf;

public class MapDisplay : MonoBehaviour
{
    public enum NoiseType
    {
        ValueNoise2D,
        PerlinNoise2D,
        VoronoiNoise2D
    }

    [SerializeField] NoiseType noiseType;
    [SerializeField] int width = 256;
    [SerializeField] int height = 256;
    [SerializeField] int seed;
    [SerializeField] Vector2 offset;
    [Min(.1f), SerializeField] Vector2 xyScale = new Vector2(1f, 1f);

    [Space(5)] [Range(1, 30f), SerializeField]
    float randomness;

    [Min(0.0001f), SerializeField] float scale;
    [Range(1, 5), SerializeField] int octaves;
    [Range(1, 5), SerializeField] float lacunarity;
    [Range(0f, 1f), SerializeField] float persistence;

    [Space(5)] [Range(1, 2), SerializeField]
    int turbulence;

    [Min(0.01f), SerializeField] float brightness = 1f;
    [Range(0, 5), SerializeField] float crease = 1f;

    [SerializeField] bool invert = false;

    [Space(10)] 
    
    public int isolines;
    [SerializeField] private string pngName;
    [SerializeField] bool png = false;

    

    Renderer textureRender;

    private int pngNumber;

    private void Awake()
    {
        textureRender = GetComponent<Renderer>();
    }

    private void OnValidate()
    {
        textureRender = GetComponent<Renderer>();

        switch (noiseType)
        {
            case NoiseType.ValueNoise2D:
                float[,] valueMap =
                    Noise.ValueNoise2D(width, height, seed, scale, octaves, persistence, lacunarity, offset, xyScale,
                        turbulence, brightness, crease, invert);
                NoiseMapVisualisation(valueMap);
                break;

            case NoiseType.PerlinNoise2D:
                float[,] perlinMap =
                    Noise.PerlinNoise2D(width, height, seed, scale, octaves, persistence, lacunarity, offset, xyScale,
                        turbulence, brightness, crease, invert);
                NoiseMapVisualisation(perlinMap);
                break;

            case NoiseType.VoronoiNoise2D:
                float[,] voronoiMap =
                    Noise.VoronoiNoise2D(width, height, seed, scale, octaves, persistence, lacunarity, offset, xyScale,
                        turbulence, brightness, crease, invert, randomness);
                NoiseMapVisualisation(voronoiMap);
                break;
        }
    }

    public void NoiseMapVisualisation(float[,] map)
    {
        Texture2D texture2D = new Texture2D(width, height);
        Color[] colorMap = new Color[width * height];

        float maxDistance = float.MinValue;
        float minDistance = float.MaxValue;

        foreach (float value in map)
        {
            maxDistance = Max(maxDistance, value);
            minDistance = Min(minDistance, value);
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedDistance = InverseLerp(minDistance, maxDistance, map[x, y]);
                colorMap[y * width + x] = Color.Lerp(
                    new Color(
                        -1 * SmoothStep(0, Abs(Sin(isolines * normalizedDistance)), 1),
                        -1 * SmoothStep(0, Abs(Sin(isolines * normalizedDistance)), 1),
                        -1 * SmoothStep(0, Abs(Sin(isolines * normalizedDistance)), 1)),
                    Color.white,
                    normalizedDistance);
            }
        }

        texture2D.SetPixels(colorMap);
        texture2D.Apply();

        textureRender.sharedMaterial.mainTexture = texture2D;
        textureRender.transform.localScale = new Vector3(width, 1, height) / 10f;

        
        
        if (png)
        {
            System.IO.File.WriteAllBytes(pngName + pngNumber++ + ".png", texture2D.EncodeToPNG());
            png = false;
            print("bum diggah");
        }
    }
}

[Serializable]
public class NoiseSettings
{
    
}