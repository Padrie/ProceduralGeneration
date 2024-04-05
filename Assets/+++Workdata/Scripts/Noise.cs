using UnityEngine;
using static UnityEngine.Mathf;
using Random = UnityEngine.Random;

public static class Noise
{
    public static float WhiteNoise(Vector2 p)
    {
        float value = Sin(Vector2.Dot(p, new Vector2(12.9898f, 78.233f))) * 43758.5453f;
        return value - Floor(value);
    }

    public static float[,] ValueNoise2D()
    {
        float[,] map = new float[1,1];
        
        return map;
    }
}

public class FBM
{
    
}
