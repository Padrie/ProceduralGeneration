using MyBox;
using UnityEngine;

public class ClearChildren : MonoBehaviour
{
    // This Method just deletes every child in the Parent
    [ButtonMethod]
    public void Clear()
    {
        for (int i = 0; i < 100; i++)
            foreach (Transform child in transform)
                DestroyImmediate(child.gameObject);
    }
}