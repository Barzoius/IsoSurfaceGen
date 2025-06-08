using System.Collections.Generic;
using UnityEngine;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct Triangle
{
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;
}

[CreateAssetMenu(fileName = "MarchingCubesGPUGenerator", menuName = "MeshGenerators/MarchingCubesGPU")]
public class MCGPUGenerator : MeshGenerator
{
    public ComputeShader marchingCubesShader;
    public ComputeShader fieldCompute;
    public int maxTriangleCount = 1000000;

    [HideInInspector] public RenderTexture scalarFieldTexture;

    public override Mesh ConstructMesh(Vector3 position, float size, int pvoxelSize)
    {
        int gridSize = (int)size;

        int threadGroupSize = 8;
        int numGroups = Mathf.CeilToInt(gridSize / (float)threadGroupSize);
        int kernel = marchingCubesShader.FindKernel("MarchCube");

        // Allocate triangle buffer (each triangle = 3 Vector3s = 36 bytes)
        ComputeBuffer triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);

        // Buffer to retrieve triangle count
        ComputeBuffer triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        // Set shader parameters
        marchingCubesShader.SetInt("gridSize", gridSize);
        marchingCubesShader.SetFloat("isoLevel", 0f);
        marchingCubesShader.SetFloat("voxelSize", pvoxelSize);
        marchingCubesShader.SetVector("offset", position);

        marchingCubesShader.SetBuffer(kernel, "triangleBuffer", triangleBuffer);

        // Dispatch compute shader
        marchingCubesShader.Dispatch(kernel, numGroups, numGroups, numGroups);

        // Retrieve triangle count
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int triangleCount = triCountArray[0];

        // Retrieve triangle data
        Triangle[] triangles = new Triangle[triangleCount];
        triangleBuffer.GetData(triangles);

        // Construct mesh
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        for (int i = 0; i < triangleCount; i++)
        {
            vertices.Add(triangles[i].a);
            indices.Add(vertices.Count - 1);
            vertices.Add(triangles[i].b);
            indices.Add(vertices.Count - 1);
            vertices.Add(triangles[i].c);
            indices.Add(vertices.Count - 1);
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateNormals();

        // Cleanup
        triangleBuffer.Release();
        triCountBuffer.Release();

        return mesh;
    }
}
