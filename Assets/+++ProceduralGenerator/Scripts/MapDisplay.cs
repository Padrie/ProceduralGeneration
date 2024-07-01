using System;
using UnityEngine;
using static UnityEngine.Mathf;

public class MapDisplay : MonoBehaviour
{
    public static MapDisplay Instance { get; private set; }

    public int width = 150; // width used for mesh and noise generation
    public int height = 150; // height used for mesh and noise generation
    public Vector2Int heightClamp = new Vector2Int(-1000, 1000); // heightClamp is to limit the max and min value of noise map
    [Min(0.1f)] public float noiseHeightMultiplier = 75; // Used for height of terrain
    [Space(10)] public NoiseTypeStruct[] noiseType = { new NoiseTypeStruct() }; // Add Scriptable Object/Preset to it

    [Space(10)] public bool updateMaterial; // If you want to see a noisemap, or terrain textures
    public Material material; // terrain texture
    [SerializeField] private string pngName = "Name"; // name of png if you save one
    [SerializeField] bool png = false;

    Renderer textureRender;

    private void Awake()
    {
        textureRender = GetComponent<Renderer>();
    }

    public void OnValidate()
    {
        width = 150;
        height = 150;

        textureRender = GetComponent<Renderer>();
        Instance = this;

        if (updateMaterial)
        {
            textureRender.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit")); // searches for a material to act as a visualisation for the noise map
        }
        else
        {
            if (material != null)
                textureRender.sharedMaterial = new Material(material);
        }

        // this if statement segment is for the noise Map and scriptable objects.
        if (noiseType[0].noisePreset != null)
        {
            DomainWarpedNoise[] domainNoise = new DomainWarpedNoise[noiseType.Length]; // creates a new reference of DomainWarpedNoise as an array

            for (int i = 0; i < noiseType.Length; i++) // goes through every element in noiseType
            {
                // Passes the information from the scriptable object to the function
                domainNoise[i] = new DomainWarpedNoise(noiseType[i].noisePreset.noiseSettings, noiseType[i].strength, width, height);
            }

            float[,] mixMap = PadrieExtension.DomainWarping(domainNoise); // Does domain warping if necessary and applies it to a 2d Float array

            NoiseMapVisualisation(mixMap); // That float array is then used to visualize the noise Map, if needed
            DrawMesh(MeshGenerator.GenerateTerrainMesh(mixMap, width, height)); // And to create the mesh, with width and height
        }
    }

    public void DrawMesh(MeshData meshData)
    {
        if (GetComponent<MeshFilter>() == null) return;
        GetComponent<MeshFilter>().sharedMesh = meshData.CreateMesh(); // this creates the mesh
        if (GetComponent<MeshCollider>() == null) return;
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh; // applies the mesh we created to the mesh collider
    }

    public void NoiseMapVisualisation(float[,] map)
    {
        Texture2D texture2D = new Texture2D(width, height); // Creates a texture with the dimension of width and height
        Color[] colorMap = new Color[width * height]; // A Color array that is width(150) * height(150) big
        texture2D.filterMode = FilterMode.Point; // This prevents the texture from being blurry
        texture2D.wrapMode = TextureWrapMode.Clamp;

        float maxDistance = float.MinValue;
        float minDistance = float.MaxValue; // smallest value in the noise map

        foreach (float value in map)
        {
            maxDistance = Max(maxDistance, value); // biggest value in the noise map
            minDistance = Min(minDistance, value); // smallest value in the noise map
        }

        float isolines = 0;

        // goes through every pixel in the texture by using a nested for loop
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedDistance = InverseLerp(minDistance, maxDistance, map[x, y]); // InverseLerp always return a value from 0.1
                colorMap[y * width + x] = Color.Lerp(
                    new Color(
                        -1 * SmoothStep(0, Abs(Sin(isolines * normalizedDistance)), 1),
                        -1 * SmoothStep(0, Abs(Sin(isolines * normalizedDistance)), 1),
                        -1 * SmoothStep(0, Abs(Sin(isolines * normalizedDistance)), 1)),
                    Color.white,
                    normalizedDistance); // this then creates the values to the colorMap
            }
        }

        texture2D.SetPixels(colorMap); // we then need to apply it to the texture
        texture2D.Apply(); // we then need to apply it to the texture

        textureRender.sharedMaterial.mainTexture = texture2D;
        transform.localScale = new Vector3(width, 150, height) / 150f;

        string path = null;

        if (png)
        {
            byte[] textureBytes = texture2D.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath + "/../" + pngName + ".png", textureBytes);
            png = false;
            print("Saved Image at: " + Application.dataPath);
        }
    }
}

[Serializable]
public class NoiseTypeStruct
{
    [Tooltip("Add a Preset from the Project window +++ProceduralGenerator/Presets")]public NoisePreset noisePreset; // Scriptable Object
    [Range(0f, 1f), Tooltip("strength of the noise map")] public float strength = 1f; // strength of the noise map
}

public class DomainWarpedNoise
{
    private float[,] noise; // this is the Noise Map
    private NoiseSettings noiseSetting; // every Setting applicable to the noise map
    private float strength; // strength of the noise map
    public int width;
    public int height;

    // Overloaded Method to store these parameters in a new Class
    public DomainWarpedNoise(NoiseSettings noiseSettings, float noiseStrength, int noiseWidth, int noiseHeight)
    {
        noiseSetting = noiseSettings;
        strength = noiseStrength;
        width = noiseWidth;
        height = noiseHeight;
        noise = MakeNoise();
    }

    // Used to make the Noise Map and add it to the 2D Float Array
    public float[,] MakeNoise()
    {
        return NoiseVariants.Start(noiseSetting, strength, width, height);
    }

    // Used to retrieve the Noise Map
    public float RetrieveNoiseMap(int y, int x)
    {
        return noise[y, x];
    }
}