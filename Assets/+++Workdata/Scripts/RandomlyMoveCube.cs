using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomlyMoveCube : MonoBehaviour
{
    public GameObject cube;
    public int count = 100;
    public int moveDir = 1;
    public bool update = true;

    private void Awake()
    {
        StartCoroutine(MoveCube());
    }

    IEnumerator MoveCube()
    {
        while (update)
        {
            int moveDir = Random.Range(-1, 1);
            
            var cubePos = cube.transform.position;

            int moveDirX = Random.Range(-moveDir, moveDir * 2);
            int moveDirY = Random.Range(-moveDir, moveDir * 2);

            cube.transform.position = new Vector3(cubePos.x + moveDirX, 0, cubePos.z + moveDirY);
            yield return null;
        }
    }
}