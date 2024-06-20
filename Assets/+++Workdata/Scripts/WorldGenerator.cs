using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldGenerator : MonoBehaviour
{
    public GameObject meshObject;
    public int size;

    private Dictionary<Vector2, Chunk> chunkDict = new Dictionary<Vector2, Chunk>();

    [ButtonMethod]
    private void Spawn()
    {
        SpawnWithNoise noise = GetComponent<SpawnWithNoise>();
        float topLeftX = (size - 1) / -2f;
        float topLeftZ = (size - 1) / 2f;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                GameObject chunk = Instantiate(meshObject);
                var renderer = chunk.GetComponent<Renderer>();
                var b = chunk.GetComponent<MapDisplay>();
                var localScale = meshObject.transform.localScale;

                Vector3 bounds = renderer.bounds.size;
                Vector3 position = new Vector3((topLeftX + i) * bounds.x, 0, (topLeftZ - j) * bounds.z);

                for (int k = 0; k < b.noiseType.Length; k++)
                {
                    b.noiseType[k].noisePreset.noiseSettings.offset.x = position.x / localScale.x;
                    b.noiseType[k].noisePreset.noiseSettings.offset.y = position.z / localScale.x;
                    b.OnValidate();
                }
                
                new ChunkInfo(position);
                
                DestroyImmediate(chunk.GetComponent<MapDisplay>());
                //chunkDict.Add(position, new Chunk(meshObject, position));

                chunk.transform.position = position;
                chunk.transform.parent = transform;

                chunk.isStatic = true;
            }
        }
        
        meshObject.SetActive(false);
        noise.OnValidate();
    }

    [ButtonMethod]
    void Clear()
    {
        meshObject.SetActive(true);
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}

public class ChunkInfo
{
    private Vector3 chunkPosition;
    public ChunkInfo(Vector3 position)
    {
        chunkPosition = position;
    }
}

[Serializable]
public class Chunk
{
    public GameObject meshObject;

    public Chunk(GameObject gameObject, Vector2 position)
    {
        meshObject = gameObject;

        Debug.Log(meshObject);
        Debug.Log(position);
    }
}