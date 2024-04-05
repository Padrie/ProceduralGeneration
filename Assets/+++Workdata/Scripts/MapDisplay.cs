using System;
using UnityEngine;
using static UnityEngine.Mathf;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MapDisplay : MonoBehaviour
{
    [Space(10)] [SerializeField] NoiseSettings noiseSettings;
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

        switch (noiseSettings.noiseType)
        {
            case NoiseSettings.NoiseType.ValueNoise2D:
                float[,] valueMap = ValueNoise.ValueNoise2D(noiseSettings);
                NoiseMapVisualisation(valueMap);
                DrawMesh(MeshGenerator.GenerateTerrainMesh(valueMap, noiseSettings));
                break;

            case NoiseSettings.NoiseType.PerlinNoise2D:
                float[,] perlinMap =
                    PerlinNoise.PerlinNoise2D(noiseSettings);
                NoiseMapVisualisation(perlinMap);
                DrawMesh(MeshGenerator.GenerateTerrainMesh(perlinMap, noiseSettings));
                break;

            case NoiseSettings.NoiseType.VoronoiNoise2D:
                float[,] voronoiMap =
                    VoronoiNoise.VoronoiNoise2D(noiseSettings);
                NoiseMapVisualisation(voronoiMap);
                DrawMesh(MeshGenerator.GenerateTerrainMesh(voronoiMap, noiseSettings));
                break;
        }
    }

    public void DrawMesh(MeshData meshData)
    {
        GetComponent<MeshFilter>().sharedMesh = meshData.CreateMesh();
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }

    public void NoiseMapVisualisation(float[,] map)
    {
        Texture2D texture2D = new Texture2D(noiseSettings.width, noiseSettings.height);
        Color[] colorMap = new Color[noiseSettings.width * noiseSettings.height];
        texture2D.filterMode = FilterMode.Point;
        texture2D.wrapMode = TextureWrapMode.Clamp;

        float maxDistance = float.MinValue;
        float minDistance = float.MaxValue;

        foreach (float value in map)
        {
            maxDistance = Max(maxDistance, value);
            minDistance = Min(minDistance, value);
        }

        for (int y = 0; y < noiseSettings.height; y++)
        {
            for (int x = 0; x < noiseSettings.width; x++)
            {
                float normalizedDistance = InverseLerp(minDistance, maxDistance, map[x, y]);
                colorMap[y * noiseSettings.width + x] = Color.Lerp(
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
        transform.localScale = new Vector3(noiseSettings.width, 1, noiseSettings.height) / 150f;


        if (png)
        {
            System.IO.File.WriteAllBytes(pngName + pngNumber++ + ".png", texture2D.EncodeToPNG());
            png = false;
            print("bum diggah");
        }
    }
}

[Serializable]
public struct NoiseSettings
{
    public enum NoiseType
    {
        ValueNoise2D,
        PerlinNoise2D,
        VoronoiNoise2D
    }

    public NoiseType noiseType;
    
    public int width;
    public int height;
    public int seed;
    public Vector2 offset;
    
    [Min(0.0001f)] public float heightMultiplier;
    [Range(1, 30f)] public float randomness;
    [Min(0.0001f)] public float scale;
    [Range(1, 5)] public int octaves;
    [Range(1, 5)] public float lacunarity;
    [Range(0f, 1f)] public float persistence;

    [Space(5)] [Range(1, 2)] public int turbulence;
    [Range(0, 5)] public float crease;
    public bool invert;
    [Space(10)] [Min(.1f)] public Vector2 xyScale;

    public static NoiseSettings Default => new NoiseSettings
    {
        width = 241,
        height = 241,
        heightMultiplier = 40f,
        randomness = 1f,
        scale = 25f,
        octaves = 4,
        lacunarity = 2f,
        persistence = 0.4f,
        turbulence = 1,
        crease = 1f,
        invert = false,
        xyScale = new Vector2(1f, 1f)
    };
}