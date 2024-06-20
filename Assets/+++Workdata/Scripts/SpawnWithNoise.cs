using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mathf;
using static PadrieExtension;

public class SpawnWithNoise : MonoBehaviour
{
    [Space(10)] public LayerMask layerToHit;

    [Header("Noise Settings")] public int seed;
    public int width = 150;
    public int height = 150;
    public float scale = 2.5f;
    public bool drawGizmo = true;
    public bool fill = false;
    [Range(1f, 5f)] public int quality = 2;
    [Range(0, 10)] public int gizmoAssetIndex;
    public Vector3 offset;
    public Vector3 gizmoOffset = new Vector3(-149, 0, 149);

    [Header("Asset Settings")] public List<StructureList> structureLists = new List<StructureList> { StructureList };
    static StructureList StructureList { get; }

    public SerializedDictionary<GameObject, string> spawnedObject = new SerializedDictionary<GameObject, string>();

    List<Vector3> spawnableGizmoPositions = new List<Vector3>();
    List<StructureList> noiseStructureLists = new List<StructureList>();
    List<StructureList> randomStructureLists = new List<StructureList>();

    System.Random prng;

    int a = 0;

    public void OnValidate()
    {
        if (structureLists.Count == 0)
        {
            a = 0;
        }

        if (a == 0)
        {
            structureLists[0] = new StructureList();
            a++;
        }

        spawnableGizmoPositions.Clear();
        prng = new System.Random(seed);

        width = MapDisplay.Instance.width;
        height = MapDisplay.Instance.height;

        for (int i = 0; i < structureLists.Count; i++)
        {
            if (structureLists[i].upperLimit < 0)
                structureLists[i].upperLimit = 0;
            if (structureLists[i].lowerLimit > 0)
                structureLists[i].lowerLimit = 0;
        }

        foreach (var structure in structureLists)
        {
            if (structure.assets.Length != 0)
                if (structure.assets[0] != null)
                    structure.name = structure.assets[0].name;

            if (gizmoAssetIndex >= structureLists.Count)
                gizmoAssetIndex = structureLists.Count - 1;

            int childCount = (int)Sqrt(gameObject.transform.childCount) == 0
                ? 1
                : (int)Sqrt(gameObject.transform.childCount);
            print(childCount);

            float topLeftX = (width - 1) / -2f;
            float topLeftZ = (height - 1) / 2f;

            for (int x = 0; x < width * childCount; x += quality)
            {
                for (int y = 0; y < height * childCount; y += quality)
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

        for (int i = 0; i < structureLists.Count; i++)
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

                for (int j = 0; j < meshObjects.Count; j++)
                {
                    offset = meshObjects[j].transform.position;
                    CreateMap(meshObjects.Count);
                }

                offset = Vector3.zero;
            }
            else
            {
                randomStructureLists.Add(structureLists[i]);
                RandomSpawn();
            }
        }
    }

    void CreateMap(int meshObjects)
    {
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        for (int i = 0; i < noiseStructureLists.Count; i++)
        {
            noiseStructureLists[i].spawnablePositions.Clear();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var a = Sqrt(meshObjects) / 2 - 0.5f;
                    float perlin = Round(Perlin(x + 149 * a, y + 149 * a, noiseStructureLists[i].invertNoise) * 100) / 100;

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
                            if (hit.point.y < noiseStructureLists[i].upperLimit &&
                                hit.point.y > noiseStructureLists[i].lowerLimit)
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
            }

            positions.Clear();
        }
    }

    #region RandomSpawn

    void RandomSpawn()
    {
        List<GameObject> meshObjects = new List<GameObject>();
        meshObjects.Clear();

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
                for (int j = 0;
                     j < meshObjects.Count * randomStructureLists[i].density * 5000 /
                     randomStructureLists[i].assets.Length;
                     j++)
                {
                    Vector3 rng = new Vector3((float)prng.NextDouble() * combinedBounds.x,
                                      (float)prng.NextDouble() * combinedBounds.y,
                                      (float)prng.NextDouble() * combinedBounds.z) -
                                  combinedBounds * 0.5f;
                    var position = primitive.transform.position;
                    Ray ray = new Ray(
                        new Vector3(position.x + rng.x * (Sqrt(meshObjects.Count) / 2), position.y,
                            position.z + rng.z * (Sqrt(meshObjects.Count) / 2)), Vector3.down);

                    if (Physics.Raycast(ray, out RaycastHit hit, 10000, layerToHit))
                    {
                        if (hit.point.y < randomStructureLists[i].upperLimit &&
                            hit.point.y > randomStructureLists[i].lowerLimit)
                        {
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
        }

        DestroyImmediate(primitive);
    }

    #endregion

    float Perlin(float x, float y, bool invert)
    {
        System.Random s = new System.Random(seed);
        
        float sampleX = (x + offset.x) / width * scale + seed * (float)s.NextDouble();
        float sampleY = (y - offset.z) / height * scale + seed * (float)s.NextDouble();
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
        if (structureLists.Count != 0)
        {
            if (drawGizmo)
            {
                DrawWirePlane(structureLists[gizmoAssetIndex].upperLimit, fill);
                DrawWirePlane(structureLists[gizmoAssetIndex].lowerLimit, fill);

                foreach (Vector3 v3 in spawnableGizmoPositions)
                {
                    DrawMarker(v3);
                }
            }
        }
    }

    void DrawMarker(Vector3 position)
    {
        if (gameObject.transform.childCount == 0) return;
        var a = Sqrt(gameObject.transform.childCount) / 2 - 0.5f;

        Gizmos.color = Color.blue;
        Vector3 corner0 = position + Vector3.forward / 2.5f + Vector3.up * 5 + new Vector3(-149, 0, 149) * a;
        Vector3 corner2 = position - Vector3.forward / 2.5f + Vector3.up * 5 + new Vector3(-149, 0, 149) * a;
        Vector3 corner1 = position + Vector3.left / 2.5f + Vector3.up * 5 + new Vector3(-149, 0, 149) * a;
        Vector3 corner3 = position - Vector3.left / 2.5f + Vector3.up * 5 + new Vector3(-149, 0, 149) * a;
        
        Gizmos.DrawLine(corner0, corner2);
        Gizmos.DrawLine(corner1, corner3);
    }

    void DrawWirePlane(float gizmoHeight, bool gizmoFill)
    {
        int childCount = (int)Sqrt(gameObject.transform.childCount);
        int w = width * childCount / 2;
        int h = height * childCount / 2;

        Gizmos.color = new Color32(255, 255, 0, 255);

        Gizmos.DrawLine(new Vector3(-w, gizmoHeight, h), new Vector3(w, gizmoHeight, h));
        Gizmos.DrawLine(new Vector3(-w, gizmoHeight, h), new Vector3(-w, gizmoHeight, -h));
        Gizmos.DrawLine(new Vector3(-w, gizmoHeight, -h), new Vector3(w, gizmoHeight, -h));
        Gizmos.DrawLine(new Vector3(w, gizmoHeight, -h), new Vector3(w, gizmoHeight, h));

        if (gizmoFill == true)
        {
            Gizmos.color = new Color32(255, 255, 0, 100);
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
    public GameObject[] assets = new GameObject[] { GameObject.CreatePrimitive(PrimitiveType.Cube) };
    public float upperLimit = 50f;
    public float lowerLimit = -50f;
    [Range(0f, 1f)] public float density = 0.2f;
    [Range(0f, 100f)] public float radius = 1f;
    [Space(5), Range(0.01f, 1f)] public float slopeAngle = 0.8f;
    public bool invert = false;
    [Space(5)] public bool canRotate = true;
    [Range(0.01f, 1f)] public float rotate = 1f;

    [Header("Noise Map Settings")] [Range(0f, 1f)]
    public float spawnRangeValue = 0.5f;

    [Range(0f, 1f)] public float secondarySpawnRangeValue = 0.4f;
    [Range(0f, 1f)] public float secondarySpawnProbability = 0.2f;
    public bool invertNoise = false;

    [HideInInspector] public List<Vector3> spawnablePositions = new List<Vector3>();
}