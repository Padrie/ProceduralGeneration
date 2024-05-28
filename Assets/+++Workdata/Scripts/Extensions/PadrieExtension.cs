using UnityEngine;

public static class PadrieExtension
{
    public static float[,] Convolute(float[,] array1, float[,] array2)
    {
        int height = array1.GetLength(0);
        int width = array1.GetLength(1);
        float[,] newArray = new float[height, width];

        return newArray;
    }

    // public static float[,] Additive(float[,] array1, float[,] array2, bool @switch)
    // {
    //     int height = array1.GetLength(0);
    //     int width = array1.GetLength(1);
    //     float[,] newArray = new float[height, width];
    //
    //     for (int y = 0; y < height; y++)
    //     {
    //         for (int x = 0; x < width; x++)
    //         {
    //             if (@switch == true)
    //             {
    //                 newArray[y, x] = array1[y, x] + array2[y, x] / 4;
    //                 //Debug.Log(newArray[y,x]);
    //             }
    //             else
    //             {
    //                 newArray[y, x] = array1[y, x] + array2[y, x] / 2;
    //                 //Debug.Log(newArray[y,x]); 
    //             }
    //         }
    //     }
    //
    //     return newArray;
    // }

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

    // public static float[,] DomainWarping(float[,] array1, float[,] array2, bool @switch, params TestNoise[] a)
    // {
    //     int height = array1.GetLength(0);
    //     int width = array1.GetLength(1);
    //     float[,] newArray = new float[height, width];
    //
    //     newArray = Additive(array1, array2, @switch);
    //
    //     return newArray;
    // }

    public static float[,] DomainWarping(TestNoise[] testNoiseArray)
    {
        float[,] newArray = new float[150, 150];

        newArray = Additive(testNoiseArray);

        return newArray;
    }

    public static void Test(params int[] a)
    {
        Debug.Log(a);
    }

    public static float Fract(float x)
    {
        float result = x - Mathf.Floor(x);

        return result;
    }
}