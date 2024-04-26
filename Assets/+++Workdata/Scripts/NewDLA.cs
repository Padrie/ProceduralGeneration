using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.Mathf;

public class NewDLA : MonoBehaviour
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

    private int boolAmount;

    [ButtonMethod]
    public void Generate()
    {
        Clear();

        Dictionary<Vector3, GameObject> positions = new Dictionary<Vector3, GameObject>();

        boolAmount = 0;
        boolAmount += Convert.ToInt32(top);
        boolAmount += Convert.ToInt32(bottom);
        boolAmount += Convert.ToInt32(left);
        boolAmount += Convert.ToInt32(right);

        Vector3 topLeft = new();
        Vector3 topRight = new();
        Vector3 bottomLeft = new();
        Vector3 bottomRight = new();

        if (top || left)
            topLeft = new Vector3(-canvas.x, 0, canvas.y);
        if (top || right)
            topRight = new Vector3(canvas.x, 0, canvas.y);
        if (bottom || left)
            bottomLeft = new Vector3(-canvas.x, 0, -canvas.y);
        if (bottom || right)
            bottomRight = new Vector3(canvas.x, 0, -canvas.y);

        var a = GameObject.CreatePrimitive(PrimitiveType.Cube);
        a.transform.parent = transform;
        a.name = "centre";
        treeList.Add(a);

        stuck = false;
        for (int i = 0; i < walkers / boolAmount; i++)
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

        failSafe = 0;
        while (!stuck && failSafe < maxLoops)
        {
            failSafe++;
            for (int i = 0; i < walkerList.Count; i++)
            {
                GameObject walker = walkerList[i];
                Vector3 walkerPos = walker.transform.position;

                bool attached = false;

                for (int j = 0; j < treeList.Count; j++)
                {
                    float dis = Vector3.Distance(walkerPos, treeList[j].transform.position);

                    if (dis <= 1 && failSafe <= maxLoops)
                    {
                        treeList.Add(walker);
                        walkerList.RemoveAt(i);
                        attached = true;
                        break;
                    }
                }

                if (!attached)
                {
                    Vector3 newPos = walkerPos + Move();
                    newPos.x = Clamp(newPos.x, -canvas.x, canvas.x);
                    newPos.z = Clamp(newPos.z, -canvas.y, canvas.y);
                    walker.transform.position = newPos;
                }
            }

            if (failSafe == maxLoops)
            {
                foreach (GameObject obj in walkerList)
                    DestroyImmediate(obj);

                walkerList.RemoveAll(obj => obj == null);
                print("Finished");
            }

            yield return null;
        }
    }

    Vector3 Move()
    {
        int randomDir = Random.Range(1, 5);
        Vector3 dir = new Vector3(0, 0, 0);

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
        walker.transform.position =
            new Vector3((int)Random.Range(first.x, first.z), 0, (int)Random.Range(second.x, second.z));
        walker.transform.parent = parent;
        walker.name = "walker";
        return walker;
    }

    [ButtonMethod]
    public void Clear()
    {
        for (int i = 0; i < 100; i++)
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
                treeList.Clear();
                walkerList.Clear();
                failSafe = 0;
            }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(canvas.x * 2 + 1, 0, canvas.y * 2 + 1));
    }
}