using System;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.Mathf;
using static PadrieExtension;

public class SpawnRandom : MonoBehaviour
{
    // public List<StructureClass> structureList = new List<StructureClass>();
    // public int seed;
    // public bool spawnPerChunk;
    // public bool visualize;
    // [Space(10)] public LayerMask layerToHit;
    // public List<GameObject> spawnedObjects = new List<GameObject>();
    //
    // [ButtonMethod]
    // public void RandomSpawn()
    // {
    //     List<GameObject> meshObjects = new List<GameObject>();
    //     meshObjects.Clear();
    //     Vector3 combinedBounds = new Vector3();
    //     System.Random prng = new System.Random(seed);
    //
    //     foreach (Transform child in transform)
    //     {
    //         meshObjects.Add(child.gameObject);
    //     }
    //     
    //     var renderer = meshObjects[0].gameObject.GetComponent<Renderer>();
    //     Vector3 bounds = renderer.bounds.size;
    //     combinedBounds = Vector3Additive(bounds, bounds);
    //
    //     GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
    //     gameObject.transform.localScale = combinedBounds / (20f / Sqrt(meshObjects.Count));
    //     gameObject.transform.position = new Vector3(0, 500, 0);
    //
    //     for (int k = 0; k < structureList.Count; k++)
    //     {
    //         for (int j = 0;
    //              j < IfSwitch(spawnPerChunk, structureList[k].spawnAmount * meshObjects.Count,
    //                  structureList[k].spawnAmount);
    //              j++)
    //         {
    //             Vector3 rng = new Vector3((float)prng.NextDouble() * combinedBounds.x,
    //                 (float)prng.NextDouble() * combinedBounds.y,
    //                 (float)prng.NextDouble() * combinedBounds.z) - combinedBounds * 0.5f;
    //
    //             Ray ray = new Ray(new Vector3(
    //                 gameObject.transform.position.x + rng.x * (Sqrt(meshObjects.Count) / 2),
    //                 gameObject.transform.position.y,
    //                 gameObject.transform.position.z + rng.z * (Sqrt(meshObjects.Count) / 2)), Vector3.down);
    //
    //             if (Physics.Raycast(ray, out RaycastHit hit, 10000, layerToHit))
    //             {
    //                 structureList[k].name = structureList[k].asset.name;
    //
    //                 if (structureList[k].invert
    //                         ? Abs(hit.normal.y) < structureList[k].slopeAngle
    //                         : Abs(hit.normal.y) > structureList[k].slopeAngle)
    //                 {
    //                     var asset = Instantiate(structureList[k].asset);
    //                     asset.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
    //                     asset.transform.parent = hit.transform;
    //                     spawnedObjects.Add(asset);
    //
    //                     if (structureList[k].canRotate)
    //                         asset.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
    //                     else
    //                         asset.transform.rotation = Quaternion.identity;
    //                 }
    //             }
    //         }
    //     }
    //     
    //     DestroyImmediate(gameObject);
    // }
    //
    // [ButtonMethod]
    // public void Clear()
    // {
    //     for (int i = 0; i < spawnedObjects.Count; i++)
    //     {
    //         DestroyImmediate(spawnedObjects[i].gameObject);
    //     }
    //     spawnedObjects.Clear();
    // }
}
//
// [Serializable]
// public class StructureClass
// {
//     [HideInInspector] public string name;
//     public GameObject asset;
//     public int spawnAmount;
//     public int weight;
//     [Space(5)] public bool invert;
//     [Range(0.01f, 1f)] public float slopeAngle;
//     public bool canRotate;
// }