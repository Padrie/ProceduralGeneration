using System;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mathf;
using static PadrieExtension;

public class SpawnWithNoise : MonoBehaviour
{
    [Space(10)] public LayerMask layerToHit;

    [Header("Noise Settings")] public int seed;
    public Vector3 offset;
    public int width = 150;
    public int height = 150;
    public float scale = 2.5f;
    public bool drawGizmo = false;
    [Range(0, 10)] public int gizmoAssetIndex;
    [Header("Asset Settings")] public StructureList[] structureLists;

    List<Vector3> spawnableGizmoPositions = new List<Vector3>();
    public SerializedDictionary<GameObject, string> spawnedObject = new SerializedDictionary<GameObject, string>();

    List<StructureList> noiseStructureLists = new List<StructureList>();
    List<StructureList> randomStructureLists = new List<StructureList>();

    System.Random prng;

    void OnValidate()
    {
        spawnableGizmoPositions.Clear();
        prng = new System.Random(seed);

        width = MapDisplay.Instance.width;
        height = MapDisplay.Instance.height;

        foreach (var structure in structureLists)
        {
            structure.name = structure.assets[0].name;

            transform.position = offset;

            if (gizmoAssetIndex >= structureLists.Length)
                gizmoAssetIndex = structureLists.Length - 1;

            float topLeftX = (width - 1) / -2f;
            float topLeftZ = (height - 1) / 2f;

            for (int x = 0; x < width; x += 3)
            {
                for (int y = 0; y < height; y += 3)
                {
                    float perlin = Round(Perlin(x, y, structureLists[gizmoAssetIndex].invertNoise) * 100) / 100;

                    if (ShouldSpawnGizmo(perlin))
                    {
                        spawnableGizmoPositions.Add(new Vector3(topLeftX + offset.x + x, 0, topLeftZ + offset.z - y));
                    }
                }
            }
        }
    }

    [ButtonMethod]
    public void StartGeneration()
    {
        spawnedObject.Clear();
        noiseStructureLists.Clear();
        randomStructureLists.Clear();

        for (int i = 0; i < structureLists.Length; i++)
        {
            if (structureLists[i].spawnType == StructureList.SpawnType.Noise)
            {
                noiseStructureLists.Add(structureLists[i]);

                List<GameObject> meshObjects = new List<GameObject>();
                meshObjects.Clear();

                foreach (Transform child in transform)
                {
                    meshObjects.Add(child.gameObject);
                }

                Vector3 saved = offset;

                for (int j = 0; j < meshObjects.Count; j++)
                {
                    offset = meshObjects[j].transform.position;
                    CreateMap();
                }

                offset = saved;
            }
            else
            {
                randomStructureLists.Add(structureLists[i]);

                RandomSpawn();
            }
        }
    }

    void CreateMap()
    {
        prng = new System.Random(seed);

        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        for (int i = 0; i < noiseStructureLists.Count; i++)
        {
            noiseStructureLists[i].spawnablePositions.Clear();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float perlin = Round(Perlin(x, y, noiseStructureLists[i].invertNoise) * 100) / 100;

                    if (ShouldSpawn(perlin, i))
                    {
                        noiseStructureLists[i].spawnablePositions
                            .Add(new Vector3(topLeftX + offset.x + x, 0, topLeftZ + offset.z - y));
                    }
                }
            }
        }

        SpawnMapObjects();
    }

   void SpawnMapObjects()
    {
        for (int i = 0; i < noiseStructureLists.Count; i++)
        {
            var positions = noiseStructureLists[i].spawnablePositions;
            float radius = noiseStructureLists[i].radius;
            
            int totalObjectsToSpawn = (int)(positions.Count * noiseStructureLists[i].density);
            int objectsPerAsset = totalObjectsToSpawn / noiseStructureLists[i].assets.Length / 5;

            for (int k = 0; k < noiseStructureLists[i].assets.Length; k++)
            {
                for (int j = 0; j < objectsPerAsset; j++)
                {
                    int random = prng.Next(0, positions.Count);
                    Vector3 spawnPos = positions[random];

                    if (!IsPositionOccupied(spawnPos, radius))
                    {
                        Vector3 rayOrigin = new Vector3(spawnPos.x, spawnPos.y + 500, spawnPos.z);
                        Ray ray = new Ray(rayOrigin, Vector3.down);

                        if (Physics.Raycast(ray, out RaycastHit hit, 10000, layerToHit))
                        {
                            if (noiseStructureLists[i].invert
                                    ? Abs(hit.normal.y) < noiseStructureLists[i].slopeAngle
                                    : Abs(hit.normal.y) > noiseStructureLists[i].slopeAngle)
                            {
                                var asset = Instantiate(noiseStructureLists[i].assets[k], hit.transform, true);
                                asset.transform.position = new Vector3((hit.point.x + spawnPos.x) / 2,
                                    hit.point.y + spawnPos.y / 2, (hit.point.z + spawnPos.z) / 2);
                                spawnedObject.Add(asset, asset.name);
                                asset.isStatic = true;

                                if (noiseStructureLists[i].canRotate)
                                {
                                    var normal = new Vector3(hit.normal.x * noiseStructureLists[i].rotate, 1,
                                        hit.normal.z * noiseStructureLists[i].rotate);

                                    asset.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
                                }
                                else
                                    asset.transform.rotation = Quaternion.identity;
                            }
                        }
                    }
                }
            }

            positions.Clear();
        }
    }

    void RandomSpawn()
    {
        List<GameObject> meshObjects = new List<GameObject>();
        meshObjects.Clear();
        System.Random random = new System.Random(seed);

        foreach (Transform child in transform)
        {
            meshObjects.Add(child.gameObject);
        }

        var renderer = meshObjects[0].gameObject.GetComponent<Renderer>();
        Vector3 bounds = renderer.bounds.size;
        var combinedBounds = Vector3Additive(bounds, bounds);

        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
        primitive.transform.localScale = combinedBounds / (20f / Sqrt(meshObjects.Count));
        primitive.transform.position = new Vector3(0, 500, 0);

        for (int i = 0; i < randomStructureLists.Count; i++)
        {
            for (int k = 0; k < randomStructureLists[i].assets.Length; k++)
            {
                for (int j = 0; j < (meshObjects.Count * randomStructureLists[i].density * 5000) / randomStructureLists[i].assets.Length; j++)
                {
                    Vector3 rng = new Vector3((float)random.NextDouble() * combinedBounds.x,
                        (float)random.NextDouble() * combinedBounds.y,
                        (float)random.NextDouble() * combinedBounds.z) - combinedBounds * 0.5f;
                    var position = primitive.transform.position;
                    Ray ray = new Ray(new Vector3(position.x + rng.x * (Sqrt(meshObjects.Count) / 2), position.y, position.z + rng.z * (Sqrt(meshObjects.Count) / 2)), Vector3.down);

                    if (Physics.Raycast(ray, out RaycastHit hit, 10000, layerToHit))
                    {
                        randomStructureLists[i].name = randomStructureLists[i].assets[k].name;

                        if (randomStructureLists[i].invert
                                ? Abs(hit.normal.y) < randomStructureLists[i].slopeAngle
                                : Abs(hit.normal.y) > randomStructureLists[i].slopeAngle)
                        {
                            var asset = Instantiate(randomStructureLists[i].assets[k], hit.transform, true);
                            asset.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                            asset.isStatic = true;

                            spawnedObject.Add(asset, asset.name);

                            if (randomStructureLists[i].canRotate)
                            {
                                var normal = new Vector3(hit.normal.x * randomStructureLists[i].rotate, 1,
                                    hit.normal.z * randomStructureLists[i].rotate);

                                asset.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
                            }
                            else
                                asset.transform.rotation = Quaternion.identity;
                        }
                    }
                }
            }
        }

        DestroyImmediate(primitive);
    }

    float Perlin(int x, int y, bool invert)
    {
        float sampleX = (x + offset.x) / width * scale + seed;
        float sampleY = (y - offset.z) / height * scale + seed;
        float perlinValue = PerlinNoise(sampleX, sampleY);
        return invert ? 1f - perlinValue : perlinValue;
    }

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

    void OnDrawGizmosSelected()
    {
        if (drawGizmo)
        {
            foreach (Vector3 v3 in spawnableGizmoPositions)
            {
                DrawPlane(v3);
            }
        }
    }

    void DrawPlane(Vector3 position)
    {
        Vector3 corner0 = position + Vector3.forward / 2 + Vector3.down * 5;
        Vector3 corner2 = position - Vector3.forward / 2 + Vector3.down * 5;
        Vector3 corner1 = position + Vector3.left / 2 + Vector3.down * 5;
        Vector3 corner3 = position - Vector3.left / 2 + Vector3.down * 5;

        Debug.DrawLine(corner0, corner2, Color.green);
        Debug.DrawLine(corner1, corner3, Color.green);
    }

    [ButtonMethod]
    public void Clear()
    {
        noiseStructureLists.Clear();
        randomStructureLists.Clear();

        foreach (KeyValuePair<GameObject, string> obj in spawnedObject)
        {
            DestroyImmediate(obj.Key);
        }

        spawnedObject.Clear();
    }
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

    public SpawnType spawnType;
    public GameObject[] assets;
    [Range(0f, 1f)] public float density = 0.2f;
    [Range(0f, 100f)] public float radius = 1f;
    [Space(5), Range(0.01f, 1f)] public float slopeAngle;
    public bool invert;
    [Space(5)] public bool canRotate;
    [Range(0.01f, 1f)] public float rotate;

    [Header("Noise Map Settings")] [Range(0f, 1f)]
    public float spawnRangeValue;

    [Range(0f, 1f)] public float secondarySpawnRangeValue = 0.5f;
    [Range(0f, 1f)] public float secondarySpawnProbability = 0.2f;
    public bool invertNoise;

    [HideInInspector] public List<Vector3> spawnablePositions = new List<Vector3>();
}
