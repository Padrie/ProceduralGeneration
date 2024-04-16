using System;
using System.Linq;
using System.Collections.Generic;
using Cinemachine;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

public class RaycastTest : MonoBehaviour
{
    public List<GameObject> gameObjects = new List<GameObject>();
    public Transform parent;
    public int seed;
    public int dotAmount = 10;

    [ButtonMethod]
    public void SpawnDots()
    {
        gameObjects.Clear();
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < 2; i++)
            foreach (Transform child in parent.transform)
                DestroyImmediate(child.gameObject);
        
        foreach (Transform child in transform)
        {
            gameObjects.Add(child.gameObject);
        }

        for (int i = 0; i < gameObjects.Count; i++)
        {
            var renderer = gameObjects[i].gameObject.GetComponent<Renderer>();
            Vector3 transformPosition = gameObjects[i].gameObject.transform.position;
            Vector3 bounds = renderer.bounds.size;

            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.localScale = new Vector3(bounds.x / 10, 25, bounds.z / 10);
            plane.transform.position = new Vector3(transformPosition.x, transformPosition.y + 100, transformPosition.z);
            plane.name = "New Plane";
            plane.transform.parent = parent.transform;

            for (int j = 0; j < dotAmount / gameObjects.Count; j++)
            {
                Vector3 rng = new Vector3((float)prng.NextDouble() * bounds.x, (float)prng.NextDouble() * bounds.y, (float)prng.NextDouble() * bounds.z) - bounds * 0.5f;
                var dot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dot.transform.localScale = new Vector3(5, 5, 5);
                dot.transform.parent = plane.transform;
                dot.transform.position = new Vector3(plane.transform.position.x + rng.x, plane.transform.position.y, plane.transform.position.z + rng.z);
            }
        }
    }
}
