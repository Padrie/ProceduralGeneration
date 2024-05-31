using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SpawnWithNoise : MonoBehaviour
{
    [Header("Noise Settings")] public int seed;
    public Vector3 offset;
    public int width = 150;
    public int height = 150;
    public float scale = 20f;
    public bool drawGizmo = false;
    [Range(0, 10)] public int gizmoAssetIndex;
    [Header("Asset Settings")] public StructureList[] structureLists;
    public GameObject[] gameObjects;

    GameObject parent;
    List<Vector3> spawnablePositions = new List<Vector3>();
    List<Vector3> spawnableGizmoPositions = new List<Vector3>();
    List<GameObject> spawnedObject = new List<GameObject>();

    System.Random prng;

    private void OnValidate()
    {
        spawnableGizmoPositions.Clear();
        prng = new System.Random(seed);

        transform.position = offset;

        if (gizmoAssetIndex >= structureLists.Length)
            gizmoAssetIndex = structureLists.Length - 1;
    
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        for (int x = 0; x < width; x += 3)
        {
            for (int y = 0; y < height; y += 3)
            {
                float perlin = Mathf.Round(Perlin(x, y, structureLists[gizmoAssetIndex].invertNoise) * 100) / 100;

                if (ShouldSpawnGizmo(perlin))
                {
                    spawnableGizmoPositions.Add(new Vector3(topLeftX + offset.x + x, 0, topLeftZ + offset.z - y));
                }
            }
        }
    }

    [ButtonMethod]
    public void StartGeneration()
    {
        Vector3 saved = offset;
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            offset = gameObjects[i].transform.position;
            CreateMap();
        }
        
        offset = saved;
    }


    [ButtonMethod]
    public void CreateMap()
    {
        prng = new System.Random(seed);

        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        for (int i = 0; i < structureLists.Length; i++)
        {
            structureLists[i].spawnablePositions.Clear();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float perlin = Mathf.Round(Perlin(x, y, structureLists[i].invertNoise) * 100) / 100;

                    if (ShouldSpawn(perlin, i))
                    {
                        structureLists[i].spawnablePositions.Add(new Vector3(topLeftX + offset.x + x, 0, topLeftZ + offset.z - y));
                    }
                }
            }
        }

        SpawnMapObjects();
    }

    [ButtonMethod]
    public void SpawnMapObjects()
    {
        spawnedObject.Clear();

        parent = new GameObject();

        for (int i = 0; i < structureLists.Length; i++)
        {
            var positions = structureLists[i].spawnablePositions;
            for (int j = 0; j < positions.Count * structureLists[i].density; j++)
            {
                int random = prng.Next(0, positions.Count);
                Vector3 randomPos = new Vector3((float)prng.NextDouble(), 0, (float)prng.NextDouble());
                GameObject test = Instantiate(structureLists[i].asset, positions[random] + randomPos, Quaternion.identity);
                test.transform.parent = parent.transform;
                spawnedObject.Add(test);
            }
        }

        print("Spawned " + spawnedObject.Count + " objects");
    }


    public float Perlin(int x, int y, bool invert)
    {
        float sampleX = ((float)x + offset.x) / width * scale + seed;
        float sampleY = ((float)y - offset.z) / height * scale + seed;
        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
        return invert ? 1f - perlinValue : perlinValue;
    }


    private bool ShouldSpawn(float perlin, int index)
    {
        if (perlin > structureLists[index].spawnRangeValue)
        {
            return true;
        }

        if (perlin > structureLists[index].secondarySpawnRangeValue)
        {
            return prng.NextDouble() < structureLists[index].secondarySpawnProbability;
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


    private void OnDrawGizmos()
    {
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        if (drawGizmo)
        {
            foreach (Vector3 v3 in spawnableGizmoPositions)
            {
                DrawPlane(v3);
            }
            //Gizmos.DrawCube(new Vector3(0,0,0), new Vector3(topLeftX * 2, 1, topLeftZ * 2));
        }
    }

    public void DrawPlane(Vector3 position)
    {
        Vector3 corner0 = position + Vector3.forward / 2 + Vector3.down * 5;
        Vector3 corner2 = position - Vector3.forward / 2 + Vector3.down * 5;
        Vector3 corner1 = position + Vector3.left / 2 + Vector3.down * 5;
        Vector3 corner3 = position - Vector3.left / 2 + Vector3.down * 5;

        Debug.DrawLine(corner0, corner2, Color.yellow);
        Debug.DrawLine(corner1, corner3, Color.yellow);
    }
}

[Serializable]
public class StructureList
{
    [HideInInspector] public string name;
    public GameObject asset;
    [Range(0f, 1f)] public float density = 0.2f;
    [Space(5), Range(0.01f, 1f)] public float slopeAngle;
    [Space(5)] public bool invert;
    public bool canRotate;

    [Header("Noise Map Settings")]
    [Range(0f, 1f)] public float spawnRangeValue;
    [Range(0f, 1f)] public float secondarySpawnRangeValue = 0.5f;
    [Range(0f, 1f)] public float secondarySpawnProbability = 0.2f;
    public bool invertNoise;
    [Space(5)] public bool @override;
    public SerializedDictionary<Vector3, GameObject> overrideList = new SerializedDictionary<Vector3, GameObject>();

    [HideInInspector] public List<Vector3> spawnablePositions = new List<Vector3>();
}
