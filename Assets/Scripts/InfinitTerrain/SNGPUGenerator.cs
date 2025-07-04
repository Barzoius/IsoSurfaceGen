using UnityEngine;
using System.Collections.Generic;

struct Quad
{
    public Vector3 vertex1;
    public Vector3 vertex2;
    public Vector3 vertex3;
    public Vector3 vertex4;

}
public class SNGPUGenerator : MeshGenerator
{
    public ComputeShader surfaceNetsShader;
    public ComputeShader fieldCompute;
    public ComputeShader editCompute;

    public int fieldSize = 64; // effective size WITHOUT padding
    public float isoLevel = 0.0f;

    float voxelSize;
    Vector3 chunkWorldPosition;

    [HideInInspector] public RenderTexture scalarFieldTexture;


    private bool initialized = false;

    // Buffers
    ComputeBuffer vertexBuffer;
    ComputeBuffer quadBuffer;


    public override Mesh ConstructMesh(Vector3 position, float size, int pvoxelSize)
    {
        chunkWorldPosition = position;
        voxelSize = pvoxelSize;


        if (!initialized || scalarFieldTexture == null || !scalarFieldTexture.IsCreated())
        {
            InitScalarFieldTexture();
            GenerateScalarField();
            initialized = true;
        }

        RunSurfaceNetsCompute();

        Mesh mesh = ExtractMesh();
        ReleaseBuffers();

        return mesh;
    }

    private void InitScalarFieldTexture()
    {
        if (scalarFieldTexture != null)
            scalarFieldTexture.Release();

        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;
        scalarFieldTexture = new RenderTexture(fieldSize+3, fieldSize+3, 0)
        {
            graphicsFormat = format,
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            volumeDepth = fieldSize + 3,
            enableRandomWrite = true
        };
        scalarFieldTexture.Create();
    }

    private void GenerateScalarField()
    {
        int kernel = fieldCompute.FindKernel("CreateScalarField");

        fieldCompute.SetTexture(kernel, "ScalarFieldTexture", scalarFieldTexture);
        fieldCompute.SetInt("textureSize", fieldSize + 1);
        fieldCompute.SetFloats("chunkWorldPosition", chunkWorldPosition.x, chunkWorldPosition.y, chunkWorldPosition.z);
        fieldCompute.SetFloat("voxelSize", voxelSize);

        int threadGroups = Mathf.CeilToInt((fieldSize + 1) / 8f);
        fieldCompute.Dispatch(kernel, threadGroups, threadGroups, threadGroups);
    }

    public override void Edit(Vector3 point, float density, float radius)
    {
        float pixelWorld = 1f;

        int editRadius = Mathf.CeilToInt(radius / pixelWorld);

        float tx = Mathf.Clamp01((point.x - chunkWorldPosition.x) / (fieldSize * voxelSize));

        float ty = Mathf.Clamp01((point.y - chunkWorldPosition.y) / (fieldSize * voxelSize));
        float tz = Mathf.Clamp01((point.z - chunkWorldPosition.z) / (fieldSize * voxelSize));

        int editX = Mathf.RoundToInt(tx * (fieldSize)) + 1;
        int editY = Mathf.RoundToInt(ty * (fieldSize)) + 1;
        int editZ = Mathf.RoundToInt(tz * (fieldSize)) + 1;

        Debug.Log($"Editing at voxel [{editX}, {editY}, {editZ}] with radius {editRadius}");

        int kernel = editCompute.FindKernel("CSMain");

        editCompute.SetFloat("density", density);
        editCompute.SetFloat("deltaTime", Time.deltaTime);
        editCompute.SetInts("brushCentre", editX, editY, editZ);
        editCompute.SetInt("brushRadius", editRadius);
        editCompute.SetInt("size", fieldSize + 1);

        editCompute.SetTexture(kernel, "EditedTexture", scalarFieldTexture);

        int threadGroups = Mathf.CeilToInt((fieldSize + 1) / 8f);
        editCompute.Dispatch(kernel, threadGroups, threadGroups, threadGroups);
    }


    void RunSurfaceNetsCompute()
    {
        int voxelCount = fieldSize * fieldSize * fieldSize;

        vertexBuffer = new ComputeBuffer(voxelCount, sizeof(float) * 3, ComputeBufferType.Append);
        quadBuffer = new ComputeBuffer(voxelCount, sizeof(float) * 3 * 4, ComputeBufferType.Append);
       

        vertexBuffer.SetCounterValue(0);
        quadBuffer.SetCounterValue(0);

        // Kernel 1: InitVertices
        int initKernel = surfaceNetsShader.FindKernel("InitVertices");
        surfaceNetsShader.SetTexture(initKernel, "ScalarFieldTexture", scalarFieldTexture);
        surfaceNetsShader.SetInt("gridSize", fieldSize);
        surfaceNetsShader.SetInt("textureSize", fieldSize + 1);
        surfaceNetsShader.SetFloat("voxelSize", voxelSize);
        surfaceNetsShader.SetFloat("isoLevel", isoLevel);
        surfaceNetsShader.SetVector("chunkWorldPosition", chunkWorldPosition);
        surfaceNetsShader.SetTexture(initKernel, "ScalarField", scalarFieldTexture);
        surfaceNetsShader.SetBuffer(initKernel, "Vertices", vertexBuffer);

        int threadGroups = Mathf.CeilToInt(fieldSize / 8f);
        surfaceNetsShader.Dispatch(initKernel, threadGroups / 8, threadGroups / 8, threadGroups / 8);

        // Kernel 2: GetQuads
        int quadKernel = surfaceNetsShader.FindKernel("GetQuads");
        surfaceNetsShader.SetTexture(quadKernel, "ScalarFieldTexture", scalarFieldTexture);
        surfaceNetsShader.SetBuffer(quadKernel, "Vertices", vertexBuffer);
        surfaceNetsShader.SetBuffer(quadKernel, "Quads", quadBuffer);
        surfaceNetsShader.Dispatch(quadKernel, threadGroups / 8, threadGroups / 8, threadGroups / 8);

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
    }
}
