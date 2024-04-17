using System;
using System.Linq;
using System.Collections.Generic;
using Cinemachine;
using MyBox;
using UnityEditor.Animations;
using UnityEngine;
using Random = UnityEngine.Random;

public class RaycastTest : MonoBehaviour
{
    List<GameObject> gameObjects = new List<GameObject>();
    public List<StructureClass> structureList = new List<StructureClass>();
    public Transform parent;
    public int seed;
    [Space(10)] public LayerMask layerToHit;

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
            plane.transform.position = new Vector3(transformPosition.x, transformPosition.y + 200, transformPosition.z);
            plane.name = "New Plane";
            plane.transform.parent = parent.transform;

            for (int k = 0; k < structureList.Count; k++)
            {
                for (int j = 0; j < structureList[k].spawnAmount / gameObjects.Count; j++)
                {
                    Vector3 rng = new Vector3((float)prng.NextDouble() * bounds.x, (float)prng.NextDouble() * bounds.y,
                        (float)prng.NextDouble() * bounds.z) - bounds * 0.5f;
                    var dot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    dot.transform.localScale = new Vector3(5, 5, 5);
                    dot.transform.parent = plane.transform;
                    dot.transform.position = new Vector3(plane.transform.position.x + rng.x, plane.transform.position.y,
                        plane.transform.position.z + rng.z);

                    Ray ray = new Ray(dot.transform.position, Vector3.down);

                    if (Physics.Raycast(ray, out RaycastHit hit, 10000, layerToHit))
                    {
                        {
                            structureList[k].name = structureList[k].asset.name;

                            if (Mathf.Abs(hit.normal.y) > structureList[k].slopeAngle)
                            {
                                var asset = Instantiate(structureList[k].asset);
                                asset.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                                asset.transform.localScale = new Vector3(4, 8, 4);
                                asset.transform.parent = hit.transform;
                                asset.transform.rotation = Quaternion.identity;
                            }
                        }
                    }
                }
            }
        }
    }
}

[Serializable]
public class StructureClass
{
    [HideInInspector] public string name;
    public GameObject asset;
    public int spawnAmount;
    public int weight;
    [Range(0f, 1f)] public float slopeAngle;
    public bool canRotate;
}