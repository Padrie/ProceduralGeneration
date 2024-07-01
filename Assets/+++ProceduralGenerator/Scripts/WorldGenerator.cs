using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldGenerator : MonoBehaviour
{
    public GameObject meshObject;
    public int size = 3;
    
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
                GameObject chunk = Instantiate(meshObject); // We spawn the meshObject we added above
                var renderer = chunk.GetComponent<Renderer>();
                var b = chunk.GetComponent<MapDisplay>();
                var localScale = meshObject.transform.localScale;

                Vector3 bounds = renderer.bounds.size; // Get the bounds of the meshObject
                Vector3 position = new Vector3((topLeftX + i) * bounds.x, 0, (topLeftZ - j) * bounds.z); // changes position of meshObject by getting the size of the map
                                                                                                         // adding I to it, and multiplying that by the bounds

                for (int k = 0; k < b.noiseType.Length; k++)
                {
                    b.noiseType[k].noisePreset.noiseSettings.offset.x = position.x / localScale.x;
                    b.noiseType[k].noisePreset.noiseSettings.offset.y = position.z / localScale.x;
                    b.OnValidate();
                }
                
                DestroyImmediate(chunk.GetComponent<MapDisplay>());

                chunk.transform.position = position;
                chunk.transform.parent = transform;

                chunk.isStatic = true;
            }
        }
        
        meshObject.SetActive(false);
        noise.OnValidate();
    }

    // Deletes every child in Parent
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