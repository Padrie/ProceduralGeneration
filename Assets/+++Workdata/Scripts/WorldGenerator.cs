using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public GameObject meshObject;
    public int size;

    [ButtonMethod]
    private void Spawn()
    {
        float topLeftX = (size - 1) / -2f;
        float topLeftZ = (size - 1) / 2f;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                GameObject test = Instantiate(meshObject);
                var renderer = test.GetComponent<Renderer>();
                
                Vector3 bounds = renderer.bounds.size;
                Vector3 position = new Vector3((topLeftX + i) * bounds.x, 0, (topLeftZ - j) * bounds.z);

                test.transform.position = position;
                test.transform.parent = transform;
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