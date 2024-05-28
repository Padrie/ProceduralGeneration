using UnityEngine;
using static UnityEngine.Mathf;
using static PadrieExtension;

public class Noise
{
    public static float WhiteNoise(Vector2 p)
    {
        float value = Sin(Vector2.Dot(p, new Vector2(12.9898f, 78.233f))) * 43758.5453f;
        //Debug.Log(value - Floor(value));
        return value - Floor(value);
    }

    #region Value Noise
    
    public static float ValueNoise(float x, float y, float randomness)
    {
        Vector2 lv = new Vector2(Fract(x), Fract(y));
        Vector2 id = new Vector2(Floor(x), Floor(y));

        lv = lv * lv * (Vector2.one * 3f - 2f * lv);

        float botLeft = WhiteNoise(id);
        float botRight = WhiteNoise(id + new Vector2(1, 0));
        float b = Lerp(botLeft, botRight, lv.x);

        float topLeft = WhiteNoise(id + new Vector2(0, 1));
        float topRight = WhiteNoise(id + new Vector2(1, 1));
        float t = Lerp(topLeft, topRight, lv.x);

        return Lerp(b, t, lv.y);
    }
    
    #endregion
    
    #region Voronoi Noise
    
    public static float VoronoiNoise(float x, float y, float randomness)
    {
        float dis = 1f;

        Vector2 n = new Vector2(Floor(x), Floor(y));
        Vector2 f = new Vector2(x - n.x, y - n.y);

        for (int j = -1; j <= 1; j++)
        {
            for (int i = -1; i <= 1; i++)
            {
                Vector2 o = new Vector2(WhiteNoise(n + new Vector2(i, j)), WhiteNoise(n + new Vector2(i, j)));
                o = new Vector2(0.5f + 0.5f * Sin(6.2831f * o.x), 0.5f + 0.5f * Sin(6.2831f * o.y)) / randomness;
                Vector2 diff = new Vector2(i, j) + o - f;
                float d = Sqrt(diff.x * diff.x + diff.y * diff.y);
                dis = Min(dis, d);
            }
        }

        return dis;
    }
    
    #endregion
}
