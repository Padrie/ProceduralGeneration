using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, NoiseSettings noiseSettings)
    {
        float topLeftX = (noiseSettings.width - 1) / -2f;
        float topLeftZ = (noiseSettings.height - 1) / 2f;

        MeshData meshData = new MeshData(noiseSettings.width, noiseSettings.height);
        int vertexIndex = 0;

        for (int y = 0; y < noiseSettings.height; y++)
        {
            for (int x = 0; x < noiseSettings.width; x++)
            {
                Vector3 vertexPosition = new Vector3((topLeftX + x), heightMap[x, y] * (noiseSettings.heightMultiplier * 100), (topLeftZ - y));
                meshData.vertices[vertexIndex] = vertexPosition;
                meshData.uvs[vertexIndex] = new Vector2(x / (float)noiseSettings.width, y / (float)noiseSettings.height);

                if (x < noiseSettings.width - 1 && y < noiseSettings.height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + noiseSettings.width + 1, vertexIndex + noiseSettings.width);
                    meshData.AddTriangle(vertexIndex + noiseSettings.width + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}