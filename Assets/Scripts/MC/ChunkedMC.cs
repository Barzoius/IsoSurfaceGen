//using MC_LOGISTICS;
//using System.Collections.Generic;
//using UnityEngine;

//public class ChunkedMC : MonoBehaviour
//{
//    public static int chunkSize = 4;
//    public static int voxelSize = 5;
//    public static float isosurface = 0f;

//    private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

//    public static readonly Vector3[] cornerOffsets = new Vector3[]
//    {
//        new Vector3(0, 0, 0), new Vector3(1, 0, 0),
//        new Vector3(1, 1, 0), new Vector3(0, 1, 0),
//        new Vector3(0, 0, 1), new Vector3(1, 0, 1),
//        new Vector3(1, 1, 1), new Vector3(0, 1, 1)
//    };

//    private void Start()
//    {
//        GenerateChunks();
//    }

//    private void GenerateChunks()
//    {
//        int numChunks = 2; // Adjust as needed

//        for (int cx = 0; cx < numChunks; cx++)
//        {
//            for (int cy = 0; cy < numChunks; cy++)
//            {
//                for (int cz = 0; cz < numChunks; cz++)
//                {
//                    Vector3Int chunkPos = new Vector3Int(cx, cy, cz);
//                    Chunk chunk = new Chunk(chunkPos);
//                    chunk.GenerateVoxels();
//                    chunk.GenerateMesh();
//                    chunks[chunkPos] = chunk;
//                }
//            }
//        }
//    }

//}

//public class Voxel
//{
//    public Vector3 position;
//    public float density;
//    public float[] densities;
//    public Vector3[] cornerPositions;
//    public int INDEX;
//    public List<Vector3> TRIANGLES;

//    public Voxel(Vector3 pos)
//    { 
//        position = pos;
//    }
//}

//public class Chunk
//{
//    public Vector3Int position;
//    private Voxel[,,] voxels;
//    private GameObject meshObject;
//    private List<Vector3[]> triangles = new List<Vector3[]>();

//    public Chunk(Vector3Int pos)
//    {
//        position = pos;
//        voxels = new Voxel[ChunkedMC.chunkSize, ChunkedMC.chunkSize, ChunkedMC.chunkSize];
//        meshObject = new GameObject($"Chunk {position}");
//        meshObject.transform.position = position * ChunkedMC.chunkSize * ChunkedMC.voxelSize;
//        meshObject.AddComponent<MeshFilter>();
//        meshObject.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
//    }

//    public static int gridSize = 4;
//    public static int voxelSize = 5;
//    float SampleSDF(Vector3 position)
//    {
//        float radius = 20.0f;
//        Vector3 center = new Vector3(gridSize * voxelSize / 2, gridSize * voxelSize / 2, gridSize * voxelSize / 2);
//        return Vector3.Distance(position, center) - radius;
//    }

//    public void GenerateVoxels()
//    {
//        for (int x = 0; x < ChunkedMC.chunkSize; x++)
//        {
//            for (int y = 0; y < ChunkedMC.chunkSize; y++)
//            {
//                for (int z = 0; z < ChunkedMC.chunkSize; z++)
//                {
//                    Vector3 worldPos = meshObject.transform.position + new Vector3(x, y, z) * MarchingCubes.voxelSize;
//                    Voxel voxel = new Voxel(worldPos);
//                    voxels[x, y, z] = voxel;
//                }
//            }
//        }
//    }



//    public void GenerateMesh()
//    {
//        Mesh mesh = new Mesh();
//        List<Vector3> vertices = new List<Vector3>();
//        List<int> indices = new List<int>();

//        triangles.Clear();

//        for (int x = 0; x < ChunkedMC.chunkSize - 1; x++)
//        {
//            for (int y = 0; y < ChunkedMC.chunkSize - 1; y++)
//            {
//                for (int z = 0; z < ChunkedMC.chunkSize - 1; z++)
//                {
//                    ProcessVoxel(x, y, z);
//                }
//            }
//        }

//        foreach (var tri in triangles)
//        {
//            vertices.Add(tri[0]);
//            indices.Add(vertices.Count - 1);

//            vertices.Add(tri[2]); // Swap order of last two vertices
//            indices.Add(vertices.Count - 1);

//            vertices.Add(tri[1]);
//            indices.Add(vertices.Count - 1);
//        }

//        mesh.vertices = vertices.ToArray();
//        mesh.triangles = indices.ToArray();
//        mesh.RecalculateNormals();

//        meshObject.GetComponent<MeshFilter>().mesh = mesh;
//    }


//    private void ProcessVoxel(int x, int y, int z)
//    {
//        Voxel voxel = voxels[x, y, z];

//        int cubeIndex = 0;
//        float[] densities = new float[8];
//        Vector3[] cornerPositions = new Vector3[8];

//        for (int i = 0; i < 8; i++)
//        {
//            cornerPositions[i] = voxel.position + ChunkedMC.cornerOffsets[i] * ChunkedMC.voxelSize;
//            densities[i] = SampleSDF(cornerPositions[i]); ;

//            if (densities[i] < MarchingCubes.isosurface)
//            {
//                cubeIndex |= (1 << i);
//            }
//        }

//        for (int i = 0; MC_DATA.triTable[cubeIndex, i] != -1; i += 3)
//        {
//            int a0 = MC_DATA.cornerIndexAFromEdge[MC_DATA.triTable[cubeIndex, i]];
//            int b0 = MC_DATA.cornerIndexBFromEdge[MC_DATA.triTable[cubeIndex, i]];

//            int a1 = MC_DATA.cornerIndexAFromEdge[MC_DATA.triTable[cubeIndex, i + 1]];
//            int b1 = MC_DATA.cornerIndexBFromEdge[MC_DATA.triTable[cubeIndex, i + 1]];

//            int a2 = MC_DATA.cornerIndexAFromEdge[MC_DATA.triTable[cubeIndex, i + 2]];
//            int b2 = MC_DATA.cornerIndexBFromEdge[MC_DATA.triTable[cubeIndex, i + 2]];

//            triangles.Add(new Vector3[]
//            {
//                VertexLerp(cornerPositions[a0], cornerPositions[b0], densities[a0], densities[b0]),
//                VertexLerp(cornerPositions[a1], cornerPositions[b1], densities[a1], densities[b1]),
//                VertexLerp(cornerPositions[a2], cornerPositions[b2], densities[a2], densities[b2])
//            });
//        }
//    }

//    private Vector3 VertexLerp(Vector3 p1, Vector3 p2, float v1, float v2)
//    {
//        float t = (MarchingCubes.isosurface - v1) / (v2 - v1);
//        return p1 + t * (p2 - p1);
//    }
//}
