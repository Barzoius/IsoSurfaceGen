using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEditor;

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

    public ComputeShader editCompute;

    public int fieldSize = 64;
    public float isoLevel = 0.0f;

    float voxelSize;
    Vector3 chunkWorldPosition;

    private ComputeBuffer triangleBuffer;
    private ComputeBuffer triCountBuffer;

    [HideInInspector] public RenderTexture scalarFieldTexture;

    private bool initialized = false;

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

        int maxTriangleCount = fieldSize * fieldSize * fieldSize * 5; 
        triangleBuffer = new ComputeBuffer(maxTriangleCount, Marshal.SizeOf(typeof(Triangle)), ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);

        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        int kernel = marchingCubesShader.FindKernel("MarchCube");

        marchingCubesShader.SetTexture(kernel, "ScalarFieldTexture", scalarFieldTexture);
        marchingCubesShader.SetBuffer(kernel, "triangleBuffer", triangleBuffer);
        marchingCubesShader.SetInt("gridSize", fieldSize);
        marchingCubesShader.SetInt("textureSize", fieldSize+1);
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


    private void InitScalarFieldTexture()
    {
        if (scalarFieldTexture != null)
            scalarFieldTexture.Release();

        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;
        scalarFieldTexture = new RenderTexture(fieldSize+3, fieldSize+3 , 0)
        {
            graphicsFormat = format,
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            volumeDepth = fieldSize +3 ,
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

        int threadGroups = Mathf.CeilToInt((fieldSize+1) / 8f);
        fieldCompute.Dispatch(kernel, threadGroups, threadGroups, threadGroups);
    }
}
