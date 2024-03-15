using System;
using UnityEngine;
using static UnityEngine.Mathf;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
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
    [Space(10)] [SerializeField] NoiseSettings noiseSettings;
    [Space(10)] [Min(.1f), SerializeField] Vector2 xyScale = new Vector2(1f, 1f);
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
                float[,] valueMap = Noise.ValueNoise2D(width, height, seed, offset, xyScale, noiseSettings);
                NoiseMapVisualisation(valueMap);
                DrawMesh(MeshGenerator.GenerateTerrainMesh(valueMap, width, height, noiseSettings));
                break;

            case NoiseType.PerlinNoise2D:
                float[,] perlinMap =
                    Noise.PerlinNoise2D(width, height, seed, offset, xyScale, noiseSettings);
                NoiseMapVisualisation(perlinMap);
                DrawMesh(MeshGenerator.GenerateTerrainMesh(perlinMap, width, height, noiseSettings));
                break;

            case NoiseType.VoronoiNoise2D:
                float[,] voronoiMap =
                    Noise.VoronoiNoise2D(width, height, seed, offset, xyScale, noiseSettings);
                NoiseMapVisualisation(voronoiMap);
                DrawMesh(MeshGenerator.GenerateTerrainMesh(voronoiMap, width, height, noiseSettings));
                break;
        }
    }

    public void DrawMesh(MeshData meshData)
    {
        FindObjectOfType<MeshFilter>().mesh = meshData.CreateMesh();
    }

    public void NoiseMapVisualisation(float[,] map)
    {
        Texture2D texture2D = new Texture2D(width, height);
        Color[] colorMap = new Color[width * height];
        texture2D.filterMode = FilterMode.Point;
        texture2D.wrapMode = TextureWrapMode.Clamp;

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
        transform.localScale = new Vector3(width, 1, height) / 150f;


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
    [Min(0.1f)] public float heightMultiplier;
    [Range(1, 30f)] public float randomness;
    [Min(0.0001f)] public float scale;
    [Range(1, 5)] public int octaves;
    [Range(1, 5)] public float lacunarity;
    [Range(0f, 1f)] public float persistence;

    [Space(5)] [Range(1, 2)] public int turbulence;
    [Min(0.01f)] public float brightness = 1f;
    [Range(0, 5)] public float crease = 1f;
    public bool invert = false;
}