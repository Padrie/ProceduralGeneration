using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class CreateMesh : MonoBehaviour
{
    public GameObject parentObject;
    public Vector3 position;

    public Dictionary<Vector3, TestMesh> testMeshDict = new Dictionary<Vector3, TestMesh>();

    [ButtonMethod]
    public void Test()
    {
        if (testMeshDict.ContainsKey(position)) return;
        
        testMeshDict.Add(position, new TestMesh(position, parentObject.transform));

        if (testMeshDict.ContainsKey(position))
            print("worked");
        else
            print("didnt work");
    }

    [ButtonMethod]
    public void RemoveObject()
    {
        List<Vector3> keysToRemove = new List<Vector3>();

        foreach (KeyValuePair<Vector3, TestMesh> kvp in testMeshDict)
        {
            if (kvp.Key == position)
            {
                DestroyImmediate(kvp.Value.meshObject);

                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (Vector3 key in keysToRemove)
        {
            testMeshDict.Remove(key);
        }
    }
}

public class TestMesh
{
    public Vector3 position;
    public GameObject meshObject;

    public TestMesh(Vector3 coord, Transform parent)
    {
        meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        position = coord;
        
        meshObject.transform.parent = parent;
        meshObject.transform.position = coord;

        Console();
    }

    public void Console()
    {
        Debug.Log(position);
        Debug.Log(meshObject);
    }
}