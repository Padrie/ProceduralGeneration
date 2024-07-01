using System;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using static UnityEngine.Mathf;
using static PadrieExtension;

public class SpawnWithNoise : MonoBehaviour
{
    [Space(10)] public LayerMask layerToHit = 1;

    [Header("Noise Settings"), Space(5)]
    public int seed;

    public int width = 150;
    public int height = 150;
    [Min(0.001f)] public float scale = 1.5f;
    [Header("Gizmos"), Space(5)]
    public bool drawGizmo = true;
    public bool fill;
    [Range(1f, 5f)] public int quality = 2;
    [Range(0, 10)] public int gizmoAssetIndex;
    [SerializeField] Color32 planeColor = new Color32(255, 255, 0, 255);
    [SerializeField] Color32 noiseMapColor = new Color32(0, 0, 255, 255);

    [Header("Asset Settings")]
    public List<StructureList> structureLists = new List<StructureList> { new StructureList() };

    Dictionary<GameObject, string> spawnedObject = new Dictionary<GameObject, string>();
    Vector3 offset;
    List<Vector3> spawnableGizmoPositions = new List<Vector3>();
    List<StructureList> noiseStructureLists = new List<StructureList>();
    List<StructureList> randomStructureLists = new List<StructureList>();

    System.Random prng;

    private int a = 0;

    public void OnValidate()
    {
        // ensures to always have an element in structureLists
        if (structureLists.Count == 0)
        {
            a = 0;
            structureLists = new List<StructureList> { new StructureList() };
        }
        
        if (a == 0 && structureLists.Count <= 0)
        {
            structureLists[0] = new StructureList();
            a++;
        }

        if (seed >= 2000)
            seed = 2000;
        if (seed <= -2000)
            seed = -2000;

        spawnableGizmoPositions.Clear();
        prng = new System.Random(seed); // creates seed

        width = MapDisplay.Instance.width;
        height = MapDisplay.Instance.height;

        for (int i = 0; i < structureLists.Count; i++)
        {
            if (structureLists[i].upperLimit < 0)
                structureLists[i].upperLimit = 0;
            if (structureLists[i].lowerLimit > 0)
                structureLists[i].lowerLimit = 0;
        }

        
        // used to visualize the noise map as gizmos
        foreach (var structure in structureLists)
        {
            if (structure.assets.Length != 0)
                if (structure.assets[0] != null)
                    structure.name = structure.assets[0].name;

            if (gizmoAssetIndex >= structureLists.Count)
                gizmoAssetIndex = structureLists.Count - 1;

            int childCount = (int)Sqrt(gameObject.transform.childCount) == 0 ? 1 : (int)Sqrt(gameObject.transform.childCount);

            float topLeftX = (width - 1) / -2f;
            float topLeftZ = (height - 1) / 2f;

            for (int x = 0; x < width * childCount; x += quality)
            {
                for (int y = 0; y < height * childCount; y += quality)
                {
                    float perlin = Round(Perlin(x, y, structureLists[gizmoAssetIndex].invertNoise) * 100) / 100; // creates a perlin noise

                    if (ShouldSpawnGizmo(perlin)) // checks if a value is high enough to pass the if statement
                    {
                        spawnableGizmoPositions.Add(new Vector3(topLeftX + offset.x + x, 0, topLeftZ + offset.z - y)); // if yes, it adds a position to the available space
                    }
                }
            }
        }
    }

    // Is the start of the Asset generation and determines if it's random or noise generated
    [ButtonMethod]
    public void StartGeneration()
    {
        spawnedObject.Clear();
        noiseStructureLists.Clear();
        randomStructureLists.Clear();

        for (int i = 0; i < structureLists.Count; i++) // checks every element in structureLists, and sorts it by the SpawnType
        {
            if (structureLists[i].spawnType == StructureList.SpawnType.Noise) // if the SpawnType is Noise
            {
                noiseStructureLists.Add(structureLists[i]); // it gets added to it's own list

                List<GameObject> meshObjects = new List<GameObject>(); // then a new List of GameObjects
                meshObjects.Clear();

                foreach (Transform child in transform)
                {
                    meshObjects.Add(child.gameObject); // every child gets added to meshObjects
                }

                for (int j = 0; j < meshObjects.Count; j++) // and for every meshObject
                {
                    offset = meshObjects[j].transform.position; // we get the position of that meshObject
                    CreateMap(meshObjects.Count); // and go to the next Method
                }

                offset = Vector3.zero;
            }
            else // if it's anything else than Spawntype Noise, this gets called
            {
                randomStructureLists.Add(structureLists[i]); // gets added to it's own List
                RandomSpawn(); // and goes to the next Method
            }
        }
    }

    void CreateMap(int meshObjects)
    {
        // centralizes
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        for (int i = 0; i < noiseStructureLists.Count; i++)
        {
            noiseStructureLists[i].spawnablePositions.Clear();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var a = Sqrt(meshObjects) / 2 - 0.5f; // Square Root of meshObjects divided by 2 and subtracting 0.5
                    float perlin = Round(Perlin(x + 149 * a, y + 149 * a, noiseStructureLists[i].invertNoise) * 100) / 100; // creating Perlin Noise

                    if (ShouldSpawn(perlin, i)) // If the value is high enough, it passed the if statement
                    {
                        noiseStructureLists[i].spawnablePositions.Add(new Vector3(topLeftX + offset.x + x, 0, topLeftZ + offset.z - y)); // and gets added to a List of possible
                                                                                                                                         // spawn locations
                    }
                }
            }
        }

        SpawnMapObjects(); // Method for generating Assets gets called
    }

    void SpawnMapObjects()
    {
        for (int i = 0; i < noiseStructureLists.Count; i++) // goes through every element in noiseStructureLists
        {
            var positions = noiseStructureLists[i].spawnablePositions; // saves the possible spawnablePosition
            float radius = noiseStructureLists[i].radius; // and radius

            int totalObjectsToSpawn = (int)(positions.Count * noiseStructureLists[i].density); // Predefined amount of Assets to spawn
            int objectsPerAsset = totalObjectsToSpawn / noiseStructureLists[i].assets.Length / 5;

            for (int k = 0; k < noiseStructureLists[i].assets.Length; k++)
            {
                for (int j = 0; j < objectsPerAsset; j++)
                {
                    int random = prng.Next(0, positions.Count); // always the same random numbers based on the seed
                    Vector3 spawnPos = positions[random]; // those predefined numbers are used to select a random position in the positions index 

                    if (!IsPositionOccupied(spawnPos, radius)) // if the positions it wants to spawn the object at isn't occupied, it goes through the if statement
                    {
                        Vector3 rayOrigin = new Vector3(spawnPos.x, spawnPos.y + 500, spawnPos.z); // position of the ray
                        Ray ray = new Ray(rayOrigin, Vector3.down); // spawn a ray pointing down the rayOrigin

                        if (Physics.Raycast(ray, out RaycastHit hit, 10000, layerToHit)) // If the ray hits the defined layer
                        {
                            if (hit.point.y < noiseStructureLists[i].upperLimit && hit.point.y > noiseStructureLists[i].lowerLimit) // and the ray hit withing the spawn limit
                            {
                                // AND if the normal is correct
                                if (noiseStructureLists[i].invert ? Abs(hit.normal.y) < noiseStructureLists[i].slopeAngle : Abs(hit.normal.y) > noiseStructureLists[i].slopeAngle)
                                {
                                    var asset = Instantiate(noiseStructureLists[i].assets[k], hit.transform, true); // it spawns the asset at the position of the hit
                                    // and applies the position
                                    asset.transform.position = new Vector3((hit.point.x + spawnPos.x) / 2, hit.point.y + spawnPos.y / 2, (hit.point.z + spawnPos.z) / 2);
                                    spawnedObject.Add(asset, asset.name); // then gets added to a dictionary with the asset and the name of said asset
                                    asset.isStatic = true;

                                    if (noiseStructureLists[i].canRotate) // if the asset can rotate
                                    {
                                        // it rotates based on the normal of the mesh face the ray hit
                                        var normal = new Vector3(hit.normal.x * noiseStructureLists[i].rotate, 1, hit.normal.z * noiseStructureLists[i].rotate);
                                        asset.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
                                    }
                                    else
                                        asset.transform.rotation = Quaternion.identity;
                                }
                            }
                        }
                    }
                }
            }

            positions.Clear();
        }
    }

    #region RandomSpawn

    void RandomSpawn()
    {
        // Creates a list of GameObject
        List<GameObject> meshObjects = new List<GameObject>();
        meshObjects.Clear();

        foreach (Transform child in transform)
        {
            meshObjects.Add(child.gameObject); // gets every child and add it to meshObjects
        }

        var renderer = meshObjects[0].gameObject.GetComponent<Renderer>();
        Vector3 bounds = renderer.bounds.size; // get the bounds of the first mesh
        var combinedBounds = Vector3Additive(bounds, bounds); // and adds them together

        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Plane); // creates a simple Plane
        primitive.transform.localScale = combinedBounds / (20f / Sqrt(meshObjects.Count)); // and adjusts it scale to cover all meshObjects
        primitive.transform.position = new Vector3(0, 500, 0); // and places it way above the meshes

        for (int i = 0; i < randomStructureLists.Count; i++)
        {
            for (int k = 0; k < randomStructureLists[i].assets.Length; k++)
            {
                for (int j = 0; j < meshObjects.Count * randomStructureLists[i].density * 5000 / randomStructureLists[i].assets.Length; j++)
                {
                    // this is the seed
                    Vector3 rng = new Vector3(
                        (float)prng.NextDouble() * combinedBounds.x,
                        (float)prng.NextDouble() * combinedBounds.y,
                        (float)prng.NextDouble() * combinedBounds.z) - combinedBounds * 0.5f;

                    var position = primitive.transform.position; // gets the position of the primitive plane
                    // and spawns rays in the bounds of that plane
                    Ray ray = new Ray(new Vector3(position.x + rng.x * (Sqrt(meshObjects.Count) / 2), position.y, position.z + rng.z * (Sqrt(meshObjects.Count) / 2)),
                        Vector3.down);

                    if (Physics.Raycast(ray, out RaycastHit hit, 10000, layerToHit)) // If the ray hits the defined layer
                    {
                        if (hit.point.y < randomStructureLists[i].upperLimit && hit.point.y > randomStructureLists[i].lowerLimit) // and the ray hit withing the spawn limit
                        {
                            // AND if the normal is correct
                            if (randomStructureLists[i].invert ? Abs(hit.normal.y) < randomStructureLists[i].slopeAngle : Abs(hit.normal.y) > randomStructureLists[i].slopeAngle)
                            {
                                var asset = Instantiate(randomStructureLists[i].assets[k], hit.transform, true); // it spawns the asset at the position of the hit
                                asset.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z); // and applies the position
                                asset.isStatic = true;

                                spawnedObject.Add(asset, asset.name); // then gets added to a dictionary with the asset and the name of said asset

                                if (randomStructureLists[i].canRotate) // if the asset can rotate
                                {
                                    // it rotates based on the normal of the mesh face the ray hit
                                    var normal = new Vector3(hit.normal.x * randomStructureLists[i].rotate, 1, hit.normal.z * randomStructureLists[i].rotate);
                                    asset.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
                                }
                                else
                                    asset.transform.rotation = Quaternion.identity;
                            }
                        }
                    }
                }
            }
        }

        DestroyImmediate(primitive);
    }

    #endregion

    // This is the Perlin method
    float Perlin(float x, float y, bool invert)
    {
        System.Random s = new System.Random(seed);

        float sampleX = (x + offset.x + seed * (float)s.Next(-100000, 100000)) / width * scale;  // X of perlinNoise created by getting the x of the method, adding the offset
                                                                                                 // and applying the scale and seed
        float sampleY = (y - offset.z + seed * (float)s.Next(-100000, 100000)) / height * scale; // Y of perlinNoise created by getting the y of the method, adding the offset
                                                                                                 // and applying the scale and seed

        float perlinValue = PerlinNoise(sampleX, sampleY); // makes perlin noise
        return invert ? 1f - perlinValue : perlinValue; // inverts perlin noise
    }

    // checks based on the distance of the GameObject in the dictionary to other GameObjects, and determines if it should spawn
    bool IsPositionOccupied(Vector3 position, float radius)
    {
        foreach (KeyValuePair<GameObject, string> obj in spawnedObject)
        {
            if (Vector3.Distance(obj.Key.transform.position, position) < radius)
            {
                return true;
            }
        }

        return false;
    }

    // If an asset should spawn by the value range
    bool ShouldSpawn(float perlin, int index)
    {
        if (perlin > noiseStructureLists[index].spawnRangeValue)
        {
            return true;
        }

        if (perlin > noiseStructureLists[index].secondarySpawnRangeValue)
        {
            return prng.NextDouble() < noiseStructureLists[index].secondarySpawnProbability;
        }

        return false;
    }

    // If a dot should spawn by the value range
    bool ShouldSpawnGizmo(float perlin)
    {
        if (perlin > structureLists[gizmoAssetIndex].spawnRangeValue)
        {
            return true;
        }

        if (perlin > structureLists[gizmoAssetIndex].secondarySpawnRangeValue)
        {
            return prng.NextDouble() < structureLists[gizmoAssetIndex].secondarySpawnProbability;
        }

        return false;
    }

    // Clears every asset in the dictionary
    [ButtonMethod]
    public void Clear()
    {
        noiseStructureLists.Clear();
        randomStructureLists.Clear();
        offset = Vector3.zero;

        OnValidate();

        foreach (KeyValuePair<GameObject, string> obj in spawnedObject)
        {
            DestroyImmediate(obj.Key);
        }

        spawnedObject.Clear();
    }

    #region Gizmo
    
    void OnDrawGizmosSelected()
    {
        if (structureLists.Count == 0) return;
        if (!drawGizmo) return;
        DrawWirePlane(structureLists[gizmoAssetIndex].upperLimit, fill);
        DrawWirePlane(structureLists[gizmoAssetIndex].lowerLimit, fill);

        foreach (Vector3 v3 in spawnableGizmoPositions)
        {
            DrawMarker(v3);
        }
    }

    // Used to display where an asset can spawn based on the Noise Map
    void DrawMarker(Vector3 position)
    {
        if (gameObject.transform.childCount == 0) return;
        if (structureLists[gizmoAssetIndex].spawnType == StructureList.SpawnType.Random) return;
        var a = Sqrt(gameObject.transform.childCount) / 2 - 0.5f;

        Gizmos.color = noiseMapColor;
        Vector3 corner0 = position + Vector3.forward / 2.5f + Vector3.up * structureLists[gizmoAssetIndex].upperLimit + new Vector3(-149, 0, 149) * a;
        Vector3 corner2 = position - Vector3.forward / 2.5f + Vector3.up * structureLists[gizmoAssetIndex].upperLimit + new Vector3(-149, 0, 149) * a;
        Vector3 corner1 = position + Vector3.left / 2.5f + Vector3.up * structureLists[gizmoAssetIndex].upperLimit + new Vector3(-149, 0, 149) * a;
        Vector3 corner3 = position - Vector3.left / 2.5f + Vector3.up * structureLists[gizmoAssetIndex].upperLimit + new Vector3(-149, 0, 149) * a;

        Gizmos.DrawLine(corner0, corner2);
        Gizmos.DrawLine(corner1, corner3);
    }

    void DrawWirePlane(float gizmoHeight, bool gizmoFill)
    {
        int childCount = (int)Sqrt(gameObject.transform.childCount);
        int w = width * childCount / 2;
        int h = height * childCount / 2;

        Gizmos.color = planeColor;

        Gizmos.DrawLine(new Vector3(-w, gizmoHeight, h), new Vector3(w, gizmoHeight, h));
        Gizmos.DrawLine(new Vector3(-w, gizmoHeight, h), new Vector3(-w, gizmoHeight, -h));
        Gizmos.DrawLine(new Vector3(-w, gizmoHeight, -h), new Vector3(w, gizmoHeight, -h));
        Gizmos.DrawLine(new Vector3(w, gizmoHeight, -h), new Vector3(w, gizmoHeight, h));

        if (gizmoFill)
        {
            Gizmos.color = new Color32(planeColor.r, planeColor.g, planeColor.b, 100);
            Gizmos.DrawCube(new Vector3(0, gizmoHeight, 0), new Vector3(w * 2, 0.0001f, h * 2));
        }
    }

    #endregion
}

[Serializable]
public class StructureList
{
    [HideInInspector] public string name;

    public enum SpawnType
    {
        Random,
        Noise
    };

    public SpawnType spawnType = SpawnType.Random;
    public GameObject[] assets = { null };
    public float upperLimit = 50f;
    public float lowerLimit = -50f;
    [Range(0f, 1f)] public float density = 0.2f;
    [Range(0f, 100f)] public float radius = 1f;
    [Space(5), Range(0.01f, 1f)] public float slopeAngle = 0.8f;
    public bool invert = false;
    [Space(5)] public bool canRotate = true;
    [Range(0.01f, 1f)] public float rotate = 1f;

    [Header("Noise Map Settings"), Space(5)]
    [Range(0f, 1f)] public float spawnRangeValue = 0.5f;

    [Range(0f, 1f)] public float secondarySpawnRangeValue = 0.4f;
    [Range(0f, 1f)] public float secondarySpawnProbability = 0.2f;
    public bool invertNoise = false;

    [HideInInspector] public List<Vector3> spawnablePositions = new List<Vector3>();
}