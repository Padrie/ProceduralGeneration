using System;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.Mathf;

public class MapDisplay : MonoBehaviour
{
    public enum AddType
    {
        Additive,
        Subtract,
        Multiply,
        Convolve,
    }

    public static MapDisplay Instance { get; private set; }

    public int width = 150;
    public int height = 150;
    [Space(5)] [Min(0.0001f)] public float heightMultiplier = 40f;
    [Space(5)] public AddType addType;
    public NoiseTypeStruct[] noiseTypeStruct;
    public TestList[] testList;

    [Space(10)] public bool updateMaterial;
    public Material material;
    public int isolines;
    [SerializeField] private string pngName;
    [SerializeField] bool png = false;

    Renderer textureRender;

    private int pngNumber;

    private void Awake()
    {
        textureRender = GetComponent<Renderer>();
    }

    public void OnValidate()
    {
        textureRender = GetComponent<Renderer>();
        Instance = this;

        if (updateMaterial) textureRender.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        else textureRender.sharedMaterial = new Material(material);

        if (noiseTypeStruct.Length == 1)
        {
            float[,] valueMap = NoiseVariants.Start(noiseTypeStruct[0].noisePreset.noiseSettings,
                noiseTypeStruct[0].strength, width, height);
            NoiseMapVisualisation(valueMap);
            DrawMesh(MeshGenerator.GenerateTerrainMesh(valueMap, noiseTypeStruct[0].noisePreset.noiseSettings, width,
                height, heightMultiplier));
        }
        else
        {
            //TestNoise test = new TestNoise(noiseTypeStruct[0].noisePreset.noiseSettings, noiseTypeStruct[0].strength, width,
            // height);

            TestNoise[] test = new TestNoise[noiseTypeStruct.Length];
            
            #region old
            //     
            // float[,] mixMap = PadrieExtension.DomainWarping(
            //     NoiseVariants.Start(noiseTypeStruct[0].noisePreset.noiseSettings, noiseTypeStruct[0].strength,
            //         width,
            //         height),
            //     NoiseVariants.Start(noiseTypeStruct[1].noisePreset.noiseSettings, noiseTypeStruct[1].strength,
            //         width,
            //         height), noiseTypeStruct[0].@override);
            //     
            #endregion

            for (int i = 0; i < noiseTypeStruct.Length; i++)
            {
                test[i] = new TestNoise(noiseTypeStruct[i].noisePreset.noiseSettings, noiseTypeStruct[i].strength,
                    width, height, noiseTypeStruct[i].@override);
            }
            
            float[,] mixMap = PadrieExtension.DomainWarping(test);

            NoiseMapVisualisation(mixMap);
            DrawMesh(MeshGenerator.GenerateTerrainMesh(mixMap, noiseTypeStruct[0].noisePreset.noiseSettings, width,
                height, heightMultiplier));
        }
    }

    public void DrawMesh(MeshData meshData)
    {
        GetComponent<MeshFilter>().sharedMesh = meshData.CreateMesh();
        if (GetComponent<MeshCollider>() == null) return;
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
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
        transform.localScale = new Vector3(width, 150, height) / 150f;

        if (png)
        {
            System.IO.File.WriteAllBytes(pngName + pngNumber++ + ".png", texture2D.EncodeToPNG());
            png = false;
            print("bum diggah");
        }
    }
}

[Serializable]
public class NoiseTypeStruct
{
    public NoisePreset noisePreset;
    [Range(0f, 1f)] public float strength = 1f;
    public bool @override;
}

public class TestNoise
{
    public float[,] noise;
    public NoiseSettings noiseSetting;
    public float strength;
    public int width;
    public int height;
    public bool @override;

    public TestNoise(NoiseSettings noiseSettings, float noiseStrength, int noiseWidth, int noiseHeight, bool _override)
    {
        noiseSetting = noiseSettings;
        strength = noiseStrength;
        width = noiseWidth;
        height = noiseHeight;
        @override = _override;
        noise = RetrieveNoise();
    }

    public void RetrieveValues(out NoiseSettings a, out float b, out int c, out int d)
    {
        a = noiseSetting;
        b = strength;
        c = width;
        d = height;
    }

    public float[,] RetrieveNoise()
    {
        return NoiseVariants.Start(noiseSetting, strength, width, height);
    }

    public float RetrieveHeightValue(int y, int x)
    {
        return noise[y, x];
    }
}

[Serializable]
public class TestList
{
    public string name;

    public enum AddType
    {
        Additive,
        Subtract,
        Multiply,
        Convolve
    }

    public NoisePreset[] noisePreset1;
    [Space(5)] public AddType addType;
    [Space(5)] public NoisePreset[] noisePreset2;
}

[Serializable]
public class NoiseSettings
{
    public NoiseVariants.NormalizeMode normalizeMode;

    public enum NoiseType
    {
        ValueNoise2D,
        PerlinNoise2D,
        VoronoiNoise2D
    }

    public NoiseType noiseType;

    public int seed = 0;
    public Vector2 offset;
    [Range(1, 30f)] public float randomness = 1f;
    [Min(0.0001f)] public float scale = 25f;
    [Range(1, 6)] public int octaves = 4;
    [Range(1, 5)] public float lacunarity = 2f;
    [Range(0f, 1f)] public float persistence = 0.4f;

    [Space(5), Range(1, 2)] public int turbulence = 1;
    [Range(0, 5)] public float crease = 1f;
    public bool invert = false;
    [Space(10), Min(.1f)] public Vector2 xyScale = new Vector2(1f, 1f);
    [Space(10)] public bool roundUp;
    [Range(1, 100)] public int roundTo = 10;
    [Range(0.01f, 1f)] public float roundStrength = 0.1f;
    [Space(5)] public bool curve;
    public AnimationCurve animationCurve;
}