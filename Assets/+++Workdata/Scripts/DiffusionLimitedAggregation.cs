using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.Mathf;

public class DiffusionLimitedAggregation : MonoBehaviour
{
    public List<GameObject> tree = new List<GameObject>();
    public Vector2 canvas;
    public int walkers;

    [Header("Spawn Walkers From")] public bool top;
    public bool bottom;
    public bool left;
    public bool right;
    [Space(10)] public int failSafe = 0;
    public int maxLoops = 10000;

    [ButtonMethod]
    public void Generate()
    {
        Dictionary<Vector3, GameObject> positions = new Dictionary<Vector3, GameObject>();

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

        print(topRight);
        print(topLeft);

        var a = GameObject.CreatePrimitive(PrimitiveType.Cube);
        a.transform.parent = transform;
        tree.Add(a);

        bool stuck = false;

        for (int i = 0; i < walkers; i++)
        {
            GameObject walker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stuck = false;

            walker.transform.position = SpawnWalker(topLeft, topRight);

            walker.transform.parent = transform;

            while (!stuck && failSafe < maxLoops)
            {
                failSafe++;
                for (int j = 0; j < tree.Count; j++)
                {
                    var d = Vector3.Distance(walker.transform.position, tree[j].transform.position);

                    if (d <= 1)
                    {
                        stuck = true;
                        break;
                    }
                }

                if (stuck) break;

                var walkerPos = walker.transform.position;

                walker.transform.position =
                    Vector3.ClampMagnitude(new Vector3(walkerPos.x + Move().x, 0, walkerPos.z + Move().y), canvas.x);
            }

            tree.Add(walker);
        }

        foreach (GameObject obj in tree)
        {
            Vector3 position = new Vector3((int)obj.transform.position.x, (int)obj.transform.position.y,
                (int)obj.transform.position.z);

            if (positions.ContainsKey(position))
                DestroyImmediate(obj);
            else
                positions.Add(position, obj);
        }

        tree.RemoveAll(obj => obj == null);
    }

    public Vector3 SpawnWalker(Vector3 first, Vector3 second)
    {
        return new Vector3(Random.Range(first.x, first.z), 0, Random.Range(second.x, second.z));
    }

    Vector2 Move()
    {
        int randomDir = Random.Range(1, 5);
        Vector2 dir = new Vector2(0, 0);

        if (randomDir == 1)
            dir = new Vector2(1, 0);
        else if (randomDir == 2)
            dir = new Vector2(-1, 0);
        else if (randomDir == 3)
            dir = new Vector2(0, 1);
        else if (randomDir == 4)
            dir = new Vector2(0, -1);
        return dir;
    }

    [ButtonMethod]
    public void Clear()
    {
        for (int i = 0; i < 100; i++)
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
                tree.Clear();
            }
    }
}