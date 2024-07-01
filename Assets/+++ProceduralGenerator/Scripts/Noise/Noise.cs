using UnityEngine;
using static UnityEngine.Mathf;
using static PadrieExtension;

public class Noise
{
    public static float WhiteNoise(Vector2 p)
    {
        float value = Sin(Vector2.Dot(p, new Vector2(12.9898f, 78.233f))) * 43758.5453f; // This method returns White Noise by essentially mutliplying a Sine wave so much,
                                                                                         // it looks random
        return Fract(value);
    }

    #region Value Noise
    
    public static float ValueNoise(float x, float y)
    {
        // Creates a grid looking structure
        Vector2 uv = new Vector2(Fract(x), Fract(y));
        Vector2 id = new Vector2(Floor(x), Floor(y));
        
        uv = uv * uv * (Vector2.one * 3f - 2f * uv);
        
        // linearly interpolate two values (a and b) by t.
        // We take the top(t) and bottom(b) positions of a grid, and Lerp the 
        // White Noise we made earlier by the uv.

        float topLeft = WhiteNoise(id + new Vector2(0, 1));
        float topRight = WhiteNoise(id + new Vector2(1, 1));
        float t = Lerp(topLeft, topRight, uv.x);
        
        float botLeft = WhiteNoise(id);
        float botRight = WhiteNoise(id + new Vector2(1, 0));
        float b = Lerp(botLeft, botRight, uv.x);

        return Lerp(b, t, uv.y);
    }
    
    #endregion
    
    #region Voronoi Noise
    
    public static float VoronoiNoise(float x, float y, float randomness)
    {
        float dis = 1f;

        // Creates a grid looking structure
        Vector2 id = new Vector2(Floor(x), Floor(y));
        Vector2 uv = new Vector2(x - id.x, y - id.y);

        for (int j = -1; j <= 1; j++)
        {
            for (int i = -1; i <= 1; i++)
            {
                Vector2 o = new Vector2(WhiteNoise(id + new Vector2(i, j)), WhiteNoise(id + new Vector2(i, j))); // Create points in the grids
                o = new Vector2(0.5f + 0.5f * Sin(6.2831f * o.x), 0.5f + 0.5f * Sin(6.2831f * o.y)) / randomness; // Places points randomly in the grids
                
                // Gets the distance between the two nearest points and returns it
                Vector2 diff = new Vector2(i, j) + o - uv;
                float d = Sqrt(diff.x * diff.x + diff.y * diff.y);
                dis = Min(dis, d);
            }
        }

        return dis;
    }
    
    #endregion
}
