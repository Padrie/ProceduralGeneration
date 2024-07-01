using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mathf;

public static class PadrieExtension
{
    // Used for Domain Warping. Gets the different Noise Maps and adds them together
    public static float[,] Additive(DomainWarpedNoise[] array)
    {
        float[,] newArray = new float[array[0].height, array[0].width];

        for (int y = 0; y < array[0].height; y++)
        {
            for (int x = 0; x < array[0].width; x++)
            {
                foreach (var test in array)
                {
                    newArray[y, x] += test.RetrieveNoiseMap(y, x);
                }

                newArray[y, x] /= array.Length;
            }
        }

        return newArray;
    }

    // This only uses the Additive method we made above and applies it to a new 2D Float array
    public static float[,] DomainWarping(DomainWarpedNoise[] array)
    {
        float[,] newArray = new float[150, 150];

        newArray = Additive(array);

        return newArray;
    }

    /// <summary> returns whole numbers </summary>
    public static float Fract(float x)
    {
        float result = x - Floor(x);

        return result;
    }
    
    
    // IfSwitch based on a bool
    public static float IfSwitch(bool @switch, float @true, float @false)
    {
        return @switch ? @true : @false;
    }
    
    // IfSwitch based on a value of 0 and 1
    public static float IfSwitch(int value, float zero, float one)
    {
        bool @switch = false;

        if (value == 1)
            @switch = false;
        else
            @switch = true;

        return @switch ? zero : one;
    }
    
    
    public static Vector3 Vector3Additive(Vector3 first, Vector3 second)
    {
        float x = first.x + second.x;
        float y = first.y + second.y;
        float z = first.z + second.z;

        return new Vector3(x, y, z);
    }

    // public static float Remap(float iMin, float iMax, float oMin, float oMax, float value)
    // {
    //     float t = InverseLerp(iMin, iMax, value);
    //     return Lerp(oMin, oMax, t);
    // }
}