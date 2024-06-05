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
    Dictionary<GameObject, string> spawnedObject = new Dictionary<GameObject, string>();

    List<StructureList> noiseStructureLists = new List<StructureList>();
    List<StructureList> randomStructureLists = new List<StructureList>();

    System.Random prng;

    private void OnValidate()
    {
        spawnableGizmoPositions.Clear();
        prng = new System.Random(seed);

        for (int i = 0; i < structureLists.Length; i++)
        {
            //structureLists[i].name = structureLists[i].assets[0].name;

            transform.position = offset;

            if (gizmoAssetIndex >= structureLists.Length)
                gizmoAssetIndex = structureLists.Length - 1;

            float topLeftX = (width - 1) / -2f;
            float topLeftZ = (height - 1) / 2f;

            for (int x = 0; x < width; x += 3)
            {
                for (int y = 0; y < height; y += 3)
                {
                    float perlin =
                        Round(Perlin(x, y, structureLists[gizmoAssetIndex].invertNoise) * 100) / 100;

                    if (ShouldSpawnGizmo(perlin))
                    {
                        spawnableGizmoPositions.Add(new Vector3(topLeftX + offset.x + x, 0,
                            topLeftZ + offset.z - y));
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

    public void CreateMap()
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

    public void SpawnMapObjects()
    {
        for (int i = 0; i < noiseStructureLists.Count; i++)
        {
            for (int k = 0; k < noiseStructureLists[i].assets.Length; k++)
            {
                var positions = noiseStructureLists[i].spawnablePositions;
                float radius = noiseStructureLists[i].radius;

                for (int j = 0;
                     j < (positions.Count * noiseStructureLists[i].density / 10) / noiseStructureLists[i].assets.Length;
                     j++)
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
                                var obj = Instantiate(noiseStructureLists[i].assets[k]);
                                obj.transform.position = new Vector3((hit.point.x + spawnPos.x) / 2, hit.point.y + spawnPos.y / 2, (hit.point.z + spawnPos.z) / 2);
                                obj.transform.parent = hit.transform;
                                spawnedObject.Add(obj, obj.name);
                                obj.isStatic = true;

                                if (noiseStructureLists[i].canRotate)
                                {
                                    var normal = new Vector3(hit.normal.x * noiseStructureLists[i].rotate, 1,
                                        hit.normal.z * noiseStructureLists[i].rotate);

                                    obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
                                }
                                else
                                    obj.transform.rotation = Quaternion.identity;
                            }
                        }
                    }
                }

                positions.Clear();
            }
        }
    }


    public void RandomSpawn()
    {
        List<GameObject> meshObjects = new List<GameObject>();
        meshObjects.Clear();
        Vector3 combinedBounds = new Vector3();
        System.Random prng = new System.Random(seed);

        foreach (Transform child in transform)
        {
            meshObjects.Add(child.gameObject);
        }

        var renderer = meshObjects[0].gameObject.GetComponent<Renderer>();
        Vector3 bounds = renderer.bounds.size;
        combinedBounds = Vector3Additive(bounds, bounds);

        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        gameObject.transform.localScale = combinedBounds / (20f / Sqrt(meshObjects.Count));
        gameObject.transform.position = new Vector3(0, 500, 0);

        for (int i = 0; i < randomStructureLists.Count; i++)
        {
            for (int k = 0; k < randomStructureLists[i].assets.Length; k++)
            {
                print("yur");
                for (int j = 0;
                     j < (meshObjects.Count * randomStructureLists[i].density * 5000) /
                     randomStructureLists[i].assets.Length;
                     j++)
                {
                    Vector3 rng = new Vector3((float)prng.NextDouble() * combinedBounds.x,
                        (float)prng.NextDouble() * combinedBounds.y,
                        (float)prng.NextDouble() * combinedBounds.z) - combinedBounds * 0.5f;
                    Ray ray = new Ray(new Vector3(
                        gameObject.transform.position.x + rng.x * (Sqrt(meshObjects.Count) / 2),
                        gameObject.transform.position.y,
                        gameObject.transform.position.z + rng.z * (Sqrt(meshObjects.Count) / 2)), Vector3.down);

                    if (Physics.Raycast(ray, out RaycastHit hit, 10000, layerToHit))
                    {
                        randomStructureLists[i].name = randomStructureLists[i].assets[k].name;

                        if (randomStructureLists[i].invert
                                ? Abs(hit.normal.y) < randomStructureLists[i].slopeAngle
                                : Abs(hit.normal.y) > randomStructureLists[i].slopeAngle)
                        {
                            var asset = Instantiate(randomStructureLists[i].assets[k]);
                            asset.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                            asset.transform.parent = hit.transform;
                            asset.isStatic = true;

                            spawnedObject.Add(asset, asset.name);

                            if (randomStructureLists[i].canRotate)
                                asset.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                            else
                                asset.transform.rotation = Quaternion.identity;
                        }
                    }
                }
            }
        }

        DestroyImmediate(gameObject);
    }

    public float Perlin(int x, int y, bool invert)
    {
        float sampleX = (x + offset.x) / width * scale + seed;
        float sampleY = (y - offset.z) / height * scale + seed;
        float perlinValue = PerlinNoise(sampleX, sampleY);
        return invert ? 1f - perlinValue : perlinValue;
    }

    private bool IsPositionOccupied(Vector3 position, float radius)
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

    private bool ShouldSpawn(float perlin, int index)
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

    private bool ShouldSpawnGizmo(float perlin)
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

    private void OnDrawGizmosSelected()
    {
        if (drawGizmo)
        {
            foreach (Vector3 v3 in spawnableGizmoPositions)
            {
                DrawPlane(v3);
            }
        }
    }

    public void DrawPlane(Vector3 position)
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
    [Space(5)] public bool invert;
    public bool canRotate;
    [Range(0.01f, 1f)] public float rotate;

    [Header("Noise Map Settings")] [Range(0f, 1f)]
    public float spawnRangeValue;

    [Range(0f, 1f)] public float secondarySpawnRangeValue = 0.5f;
    [Range(0f, 1f)] public float secondarySpawnProbability = 0.2f;
    public bool invertNoise;

    [HideInInspector] public List<Vector3> spawnablePositions = new List<Vector3>();
}