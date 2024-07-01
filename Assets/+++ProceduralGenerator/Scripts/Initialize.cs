using MyBox;
using UnityEngine;

public class Initialize : MonoBehaviour
{
    [ButtonMethod]
    public void CreateAssets()
    {
        gameObject.transform.position = Vector3.zero;
        
        gameObject.name = "-------Noise-------";
        
        GameObject noise = GameObject.CreatePrimitive(PrimitiveType.Plane);
        noise.transform.parent = gameObject.transform;
        noise.name = "(Scripts) Mesh Map Display";
        noise.AddComponent<MapDisplay>();
        
        GameObject spawner = new GameObject();
        spawner.transform.parent = gameObject.transform;
        spawner.name = "(Scripts) World Generator, Spawn With Noise";
        spawner.AddComponent<WorldGenerator>();
        spawner.AddComponent<SpawnWithNoise>();

        spawner.GetComponent<WorldGenerator>().meshObject = noise.gameObject;
        
        DestroyImmediate(this);
    }
}
