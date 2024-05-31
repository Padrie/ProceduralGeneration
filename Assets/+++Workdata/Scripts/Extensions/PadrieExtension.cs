using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mathf;

public static class PadrieExtension
{
    public static float[,] Convolute(float[,] array1, float[,] array2)
    {
        int height = array1.GetLength(0);
        int width = array1.GetLength(1);
        float[,] newArray = new float[height, width];

        return newArray;
    }

    public static float[,] Additive(TestNoise[] testNoiseArray)
    {
        float[,] newArray = new float[testNoiseArray[0].height, testNoiseArray[0].width];

        for (int y = 0; y < testNoiseArray[0].height; y++)
        {
            for (int x = 0; x < testNoiseArray[0].width; x++)
            {
                foreach (var test in testNoiseArray)
                {
                    newArray[y, x] += test.RetrieveHeightValue(y, x);
                }

                newArray[y, x] /= testNoiseArray.Length;
            }
        }

        return newArray;
    }

    public static float[,] DomainWarping(TestNoise[] testNoiseArray)
    {
        float[,] newArray = new float[150, 150];

        newArray = Additive(testNoiseArray);

        return newArray;
    }

    public static float Fract(float x)
    {
        float result = x - Mathf.Floor(x);

        return result;
    }
    
    public static float IfSwitch(bool @switch, float @true, float @false)
    {
        return @switch ? @true : @false;
    }
    
    public static float IfSwitch(int value, float @zero, float @one)
    {
        bool @switch = false;

        if (value == 1)
            @switch = false;
        else
            @switch = true;

        return @switch ? @zero : @one;
    }

    public static Vector3 Vector3Mutliply(Vector3 first, Vector3 second)
    {
        float x = first.x * second.x;
        float y = first.y * second.y;
        float z = first.z * second.z;

        return new Vector3(x, y, z);
    }
    
    public static Vector3 Vector3Additive(Vector3 first, Vector3 second)
    {
        float x = first.x + second.x;
        float y = first.y + second.y;
        float z = first.z + second.z;

        return new Vector3(x, y, z);
    }

    public static float Remap(float iMin, float iMax, float oMin, float oMax, float value)
    {
        float t = InverseLerp(iMin, iMax, value);
        return Lerp(oMin, oMax, t);
    }
}