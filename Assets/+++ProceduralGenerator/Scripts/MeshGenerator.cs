using UnityEngine;

/// <summary> Creates Mesh for the NoiseGenerator </summary
public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, int width, int height)
    {
        // We do this step to centralize the mesh, meaning the middle of the mesh is on the 0, 0 world position
        float topX = (width - 1) / -2f; // (150 -1) / -2f = -74.5f
        float topY = (height - 1) / 2f; // (150 -1) / 2f = 74.5f

        MeshData meshData = new MeshData(width, height); // First step is to give MeshData and every Array it's size by giving it the width and height
        int vertexIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //(-74.5 + 1, 2, 3..., 150), heightmap, (74.5 - 1, 2, 3..., 150)
                Vector3 vertexPosition = new Vector3(topX + x, heightMap[x, y], topY - y); // After that make a new Vector3 of vertexPositions and get a new position for
                                                                                           // the vertices
                
                //meshData.vertices[0,1,2...] = (-73.5, -72.5, -71.5...), heightmap, (73.5, 72.5, 71.5...)
                meshData.vertices[vertexIndex] = vertexPosition; // Now we add the position of the vertex to the index of the vertices array
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width); // Now we need to make the positions of the triangles
                                                                                                     // 0, 151, 150
                                                                                                     
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1); // And a second one to make a triangle with 6 vertices, that looks like a rectangle
                                                                                                 // 151, 0, 1
                }

                vertexIndex++; // Now we repeat this process 150 * 150 times, going to the next position until we get a mesh
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

    /// <summary> Create New MeshData </summary>
    //Override of MeshData, so it can be called as "New MeshData" and perform logic 
    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight]; // We define the length of the Array // 150 * 150 = 22500
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    /// <summary> pass in Index to create a Triangle </summary>
    /// <param name="a">first vertex</param>
    /// <param name="b">second vertex</param>
    /// <param name="c">third vertex</param>
    
    // We passed in the position of the vertices of the triangle, and now we need to assign the position to the correct Index of the array
    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh(); // Creates a new empty mesh with no information
        mesh.vertices = vertices; // Assigns the vertices Array we made to the vertices of the mesh, but renders no mesh yet
        mesh.triangles = triangles; // Now we connect the vertices by assigning the triangles array positions to the mesh triangles
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}