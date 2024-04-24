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
    public int steps;

    [ButtonMethod]
    public void Generate()
    {
        Dictionary<Vector3, GameObject> positions = new Dictionary<Vector3, GameObject>();

        var a = GameObject.CreatePrimitive(PrimitiveType.Cube);
        a.transform.parent = transform;
        tree.Add(a);

        bool stuck = false;

        for (int i = 0; i < steps; i++)
        {
            GameObject walker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stuck = false;

            walker.transform.position = new Vector3(Random.Range(-canvas.x / 2, canvas.y / 2), 0,
                Random.Range(-canvas.x / 2, canvas.y / 2));

            walker.transform.parent = transform;
            while (!stuck)
            {
                for (int j = 0; j < tree.Count; j++)
                {
                    var d = Vector3.Distance(walker.transform.position, tree[j].transform.position);

                    if (d <= 1)
                    {
                        stuck = true;
                        break;
                    }
                }

                if (stuck)
                    break;

                var vel = new Vector3((int)Random.Range(-canvas.x, canvas.x), 0,
                    (int)Random.Range(-canvas.y, canvas.y));

                walker.transform.position = vel;
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