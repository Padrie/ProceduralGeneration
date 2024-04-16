using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class ClearChildren : MonoBehaviour
{
    [ButtonMethod]
    public void Clear()
    {
        for (int i = 0; i < 100; i++)
            foreach (Transform child in transform)
                DestroyImmediate(child.gameObject);
    }
}