using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
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

    public int fieldSize = 64;
    public int nScale = 10;
    public int hScale = 20;
    public float isoLevel = 0.5f;

    float voxelSize;
    Vector3 chunkWorldPosition;

    private ComputeBuffer triangleBuffer;
    private ComputeBuffer triCountBuffer;

    [HideInInspector] public RenderTexture scalarFieldTexture;

    public override Mesh ConstructMesh(Vector3 position, float size, int pvoxelSize)
    {
        InitScalarFieldTexture();

        chunkWorldPosition = position;
        voxelSize = pvoxelSize;

        GenerateScalarField();

        int maxTriangleCount = fieldSize * fieldSize * fieldSize * 5; // Safe overestimate
        triangleBuffer = new ComputeBuffer(maxTriangleCount, Marshal.SizeOf(typeof(Triangle)), ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);

        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        int kernel = marchingCubesShader.FindKernel("MarchCube");

        marchingCubesShader.SetTexture(kernel, "ScalarFieldTexture", scalarFieldTexture);
        marchingCubesShader.SetBuffer(kernel, "triangleBuffer", triangleBuffer);
        marchingCubesShader.SetInt("gridSize", fieldSize);
        marchingCubesShader.SetInt("textureSize", fieldSize);
        marchingCubesShader.SetFloat("voxelSize", pvoxelSize);
        marchingCubesShader.SetFloat("isoLevel", isoLevel);
        marchingCubesShader.SetVector("chunkWorldPosition", position);

        int threadGroups = Mathf.CeilToInt(fieldSize / 8f);
        marchingCubesShader.Dispatch(kernel, threadGroups, threadGroups, threadGroups);

        
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);

        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int triangleCount = triCountArray[0];

        Triangle[] triangles = new Triangle[triangleCount];
        triangleBuffer.GetData(triangles, 0, 0, triangleCount);

        // Build mesh
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

        triangleBuffer.Release();
        triCountBuffer.Release();

        return mesh;
    }

    private void InitScalarFieldTexture()
    {
        if (scalarFieldTexture != null)
            scalarFieldTexture.Release();

        scalarFieldTexture = new RenderTexture(fieldSize, fieldSize, 0, RenderTextureFormat.RFloat)
        {
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            volumeDepth = fieldSize,
            enableRandomWrite = true
        };
        scalarFieldTexture.Create();
    }

    private void GenerateScalarField()
    {
        int kernel = fieldCompute.FindKernel("CreateScalarField");

        fieldCompute.SetTexture(kernel, "ScalarFieldTexture", scalarFieldTexture);
        fieldCompute.SetInt("textureSize", fieldSize);
        fieldCompute.SetFloats("chunkWorldPosition", chunkWorldPosition.x, chunkWorldPosition.y, chunkWorldPosition.z);
        fieldCompute.SetFloat("voxelSize", voxelSize);

        int threadGroups = Mathf.CeilToInt(fieldSize / 8f);
        fieldCompute.Dispatch(kernel, threadGroups, threadGroups, threadGroups);
    }
}
