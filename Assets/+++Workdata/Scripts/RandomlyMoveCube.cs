using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomlyMoveCube : MonoBehaviour
{
    public Vector2 canvas;
    public GameObject cube;
    public bool update = true;
    public int steps;
    
    [Header("Spawn Walkers From")]
    public bool top;
    public bool bottom;
    public bool left;
    public bool right;

    [ButtonMethod]
    public void Bounds()
    {
        Vector3 topLeft = new();
        Vector3 topRight = new();
        Vector3 bottomLeft = new();
        Vector3 bottomRight = new();
        
        if (top || left)
            topLeft = new Vector3(-canvas.x, 0, canvas.y);

        if (top || right)
            topRight = new Vector3(canvas.x, 0, canvas.y);

        if (bottom  || left)
            bottomLeft = new Vector3(-canvas.x, 0, -canvas.y);

        if (bottom || right)
            bottomRight = new Vector3(canvas.x, 0, -canvas.y);

        if(top)
            for (int i = 0; i < steps; i++)
                SpawnWalker(topLeft, topRight, transform);
        if(bottom)
            for (int i = 0; i < steps; i++)
                SpawnWalker(bottomRight, bottomLeft, transform);
        if(left)
            for (int i = 0; i < steps; i++)
                SpawnWalker(bottomLeft, topLeft, transform);
        if(right)
            for (int i = 0; i < steps; i++)
                SpawnWalker(topRight, bottomRight, transform);
    }

    public void SpawnWalker(Vector3 first, Vector3 second, Transform parent)
    {
        GameObject walker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        walker.transform.position = new Vector3(Random.Range(first.x, first.z), 0, Random.Range(second.x, second.z));
        walker.transform.parent = parent;
    }

    private void Awake()
    {
        StartCoroutine(MoveCube());
    }

    IEnumerator MoveCube()
    {
        while (update)
        {
            var cubePos = cube.transform.position;

            cube.transform.position = new Vector3(cubePos.x + Move().x, 0, cubePos.z + Move().y);
            yield return new WaitForSeconds(0.2f);
        }
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
}