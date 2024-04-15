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
        float topLeftX = (size - 1) / -2f;
        float topLeftZ = (size - 1) / 2f;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                GameObject a = Instantiate(meshObject);
                var renderer = a.GetComponent<Renderer>();
                var b = a.GetComponent<MapDisplay>();
                var localScale = meshObject.transform.localScale;

                Vector3 bounds = renderer.bounds.size;
                Vector3 position = new Vector3(((topLeftX + i) * bounds.x), 0, (topLeftZ - j) * bounds.z);
                
                b.noiseSettings.offset.x = position.x / localScale.x;
                b.noiseSettings.offset.y = position.z / localScale.x;
                b.OnValidate();
                
                new ChunkInfo(position);

                //chunkDict.Add(position, new Chunk(meshObject, position));

                a.transform.position = position;
                a.transform.parent = transform;
            }
        }
    }

    [ButtonMethod]
    void Clear()
    {
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
        Debug.Log(position);
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