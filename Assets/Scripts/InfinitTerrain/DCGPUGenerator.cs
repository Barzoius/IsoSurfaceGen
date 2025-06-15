using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "DualContouringGPUGenerator", menuName = "MeshGenerators/DualContouringGPU")]
public class DCGPUGenerator : MeshGenerator
{
    public ComputeShader surfaceNetsShader;
    public ComputeShader fieldCompute;

    public int fieldSize = 64;
    public float isoLevel = 0.0f;

    float voxelSize;
    Vector3 chunkWorldPosition;

    [HideInInspector] public RenderTexture scalarFieldTexture;

    // Buffers
    ComputeBuffer vertexBuffer;
    ComputeBuffer quadBuffer;
    ComputeBuffer indexBuffer;

    public override Mesh ConstructMesh(Vector3 position, float size, int pvoxelSize)
    {
        chunkWorldPosition = position;
        voxelSize = pvoxelSize;

        //InitScalarFieldTexture();
        //GenerateScalarField();

        RunSurfaceNetsCompute();

        Mesh mesh = ExtractMesh();
        ReleaseBuffers();

        return mesh;
    }

    void RunSurfaceNetsCompute()
    {
        int voxelCount = fieldSize * fieldSize * fieldSize;

        vertexBuffer = new ComputeBuffer(voxelCount, sizeof(float) * 3, ComputeBufferType.Append);
        quadBuffer = new ComputeBuffer(voxelCount, sizeof(float) * 3 * 4, ComputeBufferType.Append);
        indexBuffer = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);

        vertexBuffer.SetCounterValue(0);
        quadBuffer.SetCounterValue(0);
        indexBuffer.SetCounterValue(0);

        // Kernel 1: InitVertices
        int initKernel = surfaceNetsShader.FindKernel("InitVertices");
        surfaceNetsShader.SetInt("gridSize", fieldSize);
        surfaceNetsShader.SetFloat("voxelSize", voxelSize);
        surfaceNetsShader.SetFloat("isoLevel", isoLevel);
        surfaceNetsShader.SetVector("chunkWorldPosition", chunkWorldPosition);
        //surfaceNetsShader.SetTexture(initKernel, "ScalarField", scalarFieldTexture);
        surfaceNetsShader.SetBuffer(initKernel, "Vertices", vertexBuffer);

        surfaceNetsShader.Dispatch(initKernel, fieldSize / 8, fieldSize / 8, fieldSize / 8);

        // Kernel 2: GetQuads
        int quadKernel = surfaceNetsShader.FindKernel("GetQuads");
        surfaceNetsShader.SetBuffer(quadKernel, "Vertices", vertexBuffer);
        surfaceNetsShader.SetBuffer(quadKernel, "Quads", quadBuffer);
        surfaceNetsShader.Dispatch(quadKernel, fieldSize / 8, fieldSize / 8, fieldSize / 8);


        int quadCount = GetCount(quadBuffer);
        // Kernel 3: GetIndices
        int triKernel = surfaceNetsShader.FindKernel("GetIndices");
        surfaceNetsShader.SetBuffer(triKernel, "Quads", quadBuffer);
        surfaceNetsShader.SetBuffer(triKernel, "Indices", indexBuffer);
        surfaceNetsShader.Dispatch(triKernel, quadCount, 1, 1);
    }

    Mesh ExtractMesh()
    {
        int quadCount = GetCount(quadBuffer);
        if (quadCount == 0)
            return new Mesh();

        Quad[] quads = new Quad[quadCount];
        quadBuffer.GetData(quads, 0, 0, quadCount);

        Vector3[] vertices = new Vector3[quadCount * 4];
        int[] triangles = new int[quadCount * 6];

        for (int i = 0; i < quadCount; i++)
        {
            int vi = i * 4;
            vertices[vi + 0] = quads[i].vertex1;
            vertices[vi + 1] = quads[i].vertex2;
            vertices[vi + 2] = quads[i].vertex3;
            vertices[vi + 3] = quads[i].vertex4;

            int ti = i * 6;
            triangles[ti + 0] = vi + 0;
            triangles[ti + 1] = vi + 1;
            triangles[ti + 2] = vi + 2;

            triangles[ti + 3] = vi + 2;
            triangles[ti + 4] = vi + 3;
            triangles[ti + 5] = vi + 0;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    int GetCount(ComputeBuffer buffer)
    {
        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer.CopyCount(buffer, countBuffer, 0);
        int[] countArray = { 0 };
        countBuffer.GetData(countArray);
        countBuffer.Release();
        return countArray[0];
    }

    void ReleaseBuffers()
    {
        vertexBuffer?.Release();
        quadBuffer?.Release();
        indexBuffer?.Release();
    }
}
