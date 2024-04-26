using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.Mathf;

public class EvenNewerDLA : MonoBehaviour
{
    public List<GameObject> treeList = new List<GameObject>();
    public List<GameObject> walkerList = new List<GameObject>();
    public Vector2 canvas;
    public int walkers;

    [Header("Spawn Walkers From")] public bool top;
    public bool bottom;
    public bool left;
    public bool right;
    [Space(10)] public int failSafe = 0;
    public int maxLoops = 10000;

    bool stuck = false;

    [ButtonMethod]
    public void Generate()
    {
        Clear();

        Dictionary<Vector3, GameObject> positions = new Dictionary<Vector3, GameObject>();

        var topLeft = new Vector3(-canvas.x, 0, canvas.y);
        var topRight = new Vector3(canvas.x, 0, canvas.y);
        var bottomLeft = new Vector3(-canvas.x, 0, -canvas.y);
        var bottomRight = new Vector3(canvas.x, 0, -canvas.y);

        var a = GameObject.CreatePrimitive(PrimitiveType.Cube);
        a.transform.parent = transform;
        a.name = "centre";
        treeList.Add(a);

        stuck = false;
        for (int i = 0; i < walkers; i++)
        {
            if (top)
                walkerList.Add(SpawnWalker(topLeft, topRight, transform));
            if (bottom)
                walkerList.Add(SpawnWalker(bottomRight, bottomLeft, transform));
            if (left)
                walkerList.Add(SpawnWalker(bottomLeft, topLeft, transform));
            if (right)
                walkerList.Add(SpawnWalker(topRight, bottomRight, transform));
        }

        StartCoroutine(WalkerMovement());

        foreach (GameObject obj in treeList)
        {
            Vector3 position = new Vector3((int)obj.transform.position.x, (int)obj.transform.position.y,
                (int)obj.transform.position.z);

            if (positions.ContainsKey(position))
                DestroyImmediate(obj);
            else
                positions.Add(position, obj);
        }

        treeList.RemoveAll(obj => obj == null);
    }

    IEnumerator WalkerMovement()
    {
        stuck = false;
        while (!stuck && failSafe < maxLoops)
        {
            failSafe++;
            for (int j = 0; j < walkerList.Count; j++)
            {
                GameObject walker = walkerList[j];
                Vector3 walkerPos = walker.transform.position;

                bool attached = false;

                for (int k = 0; k < treeList.Count; k++)
                {
                    GameObject treeObj = treeList[k];
                    float dis = Vector3.Distance(walkerPos, treeObj.transform.position);

                    if (dis <= 1)
                    {
                        treeList.Add(walker);
                        walkerList.RemoveAt(j);
                        attached = true;
                        break;
                    }
                }

                if (!attached)
                {
                    Vector3 newPos = walkerPos + Move();
                    newPos.x = Mathf.Clamp(newPos.x, -canvas.x, canvas.x);
                    newPos.z = Mathf.Clamp(newPos.z, -canvas.y, canvas.y);
                    walker.transform.position = newPos;
                }
            }

            yield return null;
        }
    }

    Vector3 Move()
    {
        int randomDir = Random.Range(1, 5);
        Vector3 dir = Vector3.zero;

        if (randomDir == 1)
            dir = Vector3.right;
        else if (randomDir == 2)
            dir = Vector3.left;
        else if (randomDir == 3)
            dir = Vector3.forward;
        else if (randomDir == 4)
            dir = Vector3.back;

        return dir;
    }

    public GameObject SpawnWalker(Vector3 first, Vector3 second, Transform parent)
    {
        GameObject walker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        walker.transform.position = new Vector3(Random.Range(first.x, first.z), 0, Random.Range(second.x, second.z));
        walker.transform.parent = parent;
        walker.name = "walker";
        return walker;
    }

    [ButtonMethod]
    public void Clear()
    {
        for (int i = 0; i < 100; i++)
        {
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
                treeList.Clear();
                walkerList.Clear();
                failSafe = 0;
            }
        }
    }
}