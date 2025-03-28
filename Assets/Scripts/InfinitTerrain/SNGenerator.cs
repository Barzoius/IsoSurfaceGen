using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[CreateAssetMenu(fileName = "SurfaceNetsGenerator", menuName = "MeshGenerators/SurfaceNets")]
public class SMGenerator : MeshGenerator
{

    public static int gridSize;
    public static int voxelSize;

    private List<Vector3> VertexBuffer = new List<Vector3>();
    private List<int> TriangleBuffer = new List<int>();

    struct Edge
    {
        public UnityEngine.Vector3 normal;
        public UnityEngine.Vector3 intersection;
        public bool crossed;
    }

    struct Voxel
    {
        public float[] densities;
        public UnityEngine.Vector3[] cornerPositions;
        public Edge[] edgeData;

        public int INDEX;

        public Vector3 vertex;
    }

    public int[,] edges = new int[12, 2]
    {
            {0,4},{1,5},{2,6},{3,7},	// x-axis 
			{0,2},{1,3},{4,6},{5,7},	// y-axis
			{0,1},{2,3},{4,5},{6,7}		// z-axis
    };

    Voxel[] grid = new Voxel[gridSize * gridSize * gridSize];


    // Predefined corner offsets
    private static readonly Vector3[] cornerOffsets = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(1, 1, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1)
    };


    int[,] directions = { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

    int flattenIndex(int x, int y, int z)
    {
        return x * gridSize * gridSize + y * gridSize + z;
    }

    //float SampleSDF(Vector3 position)
    //{
    //    float scale = 0.1f;
    //    float heightMultiplier = 20f; //  max height

    //    float height = Mathf.PerlinNoise(position.x * scale, position.z * scale) * heightMultiplier;

    //    return position.y - height;
    //}

    float SampleSDF(Vector3 position)
    {
        Vector2 t = new Vector2(5.0f, 2.0f); // major (30) / minor (10) radius 
        Vector3 center = new Vector3(gridSize * voxelSize / 2, gridSize * voxelSize / 2, gridSize * voxelSize / 2);

        Vector3 p = position - center;
        Vector2 q = new Vector2(Vector3.Distance(new Vector3(p.x, p.y, 0), Vector3.zero) - t.x, p.z);

        return q.magnitude - t.y;
    }



    //float SampleSDF(Vector3 position, float height = 1)
    //{
    //    return position.y - height;
    //}

    UnityEngine.Vector3 computeGradient(float x, float y, float z)
    {
        float epsilon = 0.01f;
        float dx = SampleSDF(new Vector3((int)(x + epsilon), (int)y, (int)z)) - SampleSDF(new Vector3((int)(x - epsilon), (int)y, (int)z));
        float dy = SampleSDF(new Vector3((int)x, (int)(y + epsilon), (int)z)) - SampleSDF(new Vector3((int)x, (int)(y - epsilon), (int)z));
        float dz = SampleSDF(new Vector3((int)x, (int)y, (int)(z + epsilon))) - SampleSDF(new Vector3((int)x, (int)y, (int)(z - epsilon)));


        return UnityEngine.Vector3.Normalize(new UnityEngine.Vector3(dx, dy, dz));
    }


    void initEdgeData(Voxel voxel)
    {
        for (int i = 0; i < 12; i++)
        {
            int vertex1 = edges[i, 0];
            int vertex2 = edges[i, 1];

            float sampleV1 = voxel.densities[vertex1];
            float sampleV2 = voxel.densities[vertex2];

            if (Mathf.Abs(sampleV1 - sampleV2) > 0.0001f && (sampleV1 < 0) != (sampleV2 < 0))
            {
                float t = sampleV1 / (sampleV1 - sampleV2);

                UnityEngine.Vector3 intersectionPoint =
                    UnityEngine.Vector3.Lerp(voxel.cornerPositions[vertex1],
                                                 voxel.cornerPositions[vertex2], t);

                UnityEngine.Vector3 normal = computeGradient(intersectionPoint.x, intersectionPoint.y, intersectionPoint.z);

                voxel.edgeData[i] = new Edge
                {
                    intersection = intersectionPoint,
                    normal = normal,
                    crossed = true
                };
            }
            else
            {
                voxel.edgeData[i] = new Edge
                {
                    crossed = false
                };
            }
        }
    }

    void InitGridDat()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    Voxel voxel = new Voxel
                    {
                        densities = new float[8],
                        edgeData = new Edge[12],
                        cornerPositions = new UnityEngine.Vector3[8],
                        INDEX = 0,
                        vertex = Vector3.zero
                    };


                    // Base position of the voxel
                    Vector3 basePos = new Vector3(x * voxelSize, y * voxelSize, z * voxelSize);

                    for (int corner = 0; corner < 8; corner++)
                    {
                        Vector3 cornerPos = basePos + cornerOffsets[corner] * voxelSize;
                        voxel.cornerPositions[corner] = cornerPos;
                        voxel.densities[corner] = SampleSDF(cornerPos);


                        if (voxel.densities[corner] < 0f)
                        {
                            voxel.INDEX |= (1 << corner);
                        }
                    }

                    int index = flattenIndex(x, y, z);
                    grid[index] = voxel;

                    initEdgeData(grid[index]);


                    Vector3 C = new Vector3();
                    int n = 0;
                    foreach (Edge edge in grid[index].edgeData)
                    {
                        if (edge.crossed)
                        {
                            n++;
                            C += edge.intersection;
                        }
                    }


                    if (n > 0)
                    {
                        C = C / n;
                        grid[index].vertex = new Vector3(C.x, C.y, C.z);
                        //Debug.Log("vertex position :\n" + C);
                        //Instantiate(spherePrefab, C, UnityEngine.Quaternion.identity);
                    }
                    else
                    {

                        //Debug.Log("N = " + n);

                    }

                }
            }
        }
    }
    void SurfaceNets(Vector3 position)
    {
        for (int x = 0; x < gridSize - 1; x++)
        {
            for (int y = 0; y < gridSize - 1; y++)
            {
                for (int z = 0; z < gridSize - 1; z++)
                {
                    int currentIndex = flattenIndex(x, y, z);
                    Vector3 v0 = grid[currentIndex].vertex;

                    if (v0 == Vector3.zero)
                    {
                        //Debug.Log($"[Missing Quad ] Skipped at ({x},{y},{z}) due to missing vertex v0");

                        continue; // skip empty voxels
                    }


                    int rightIndex = flattenIndex(x + 1, y, z);
                    int topIndex = flattenIndex(x, y + 1, z);
                    int frontIndex = flattenIndex(x, y, z + 1);

                    // Check X-aligned face (Right)
                    if (x + 1 < gridSize)
                    {
                        Vector3 v1 = grid[rightIndex].vertex;
                        int nextZ = flattenIndex(x + 1, y, z + 1);
                        int nextY = flattenIndex(x, y, z + 1);

                        if (v1 != Vector3.zero && grid[nextZ].vertex != Vector3.zero && grid[nextY].vertex != Vector3.zero)
                        {
                            AddQuad(v0, v1, grid[nextZ].vertex, grid[nextY].vertex);
                        }
                        else
                        {
                            //Debug.Log($"[Missing Quad] Skipped at ({x},{y},{z}) due to missing vertex v1");
                        }
                    }

                    // Check Y-aligned face (Top)
                    if (y + 1 < gridSize)
                    {
                        Vector3 v1 = grid[topIndex].vertex;
                        int nextZ = flattenIndex(x + 1, y + 1, z);
                        int nextY = flattenIndex(x + 1, y, z);

                        if (v1 != Vector3.zero && grid[nextZ].vertex != Vector3.zero && grid[nextY].vertex != Vector3.zero)
                        {
                            AddQuad(v0, v1, grid[nextZ].vertex, grid[nextY].vertex);
                        }
                        else
                        {
                            //Debug.Log($"[Missing Quad] Skipped at ({x},{y},{z}) due to missing vertex v2");
                        }
                    }

                    // Check Z-aligned face (Front)
                    if (z + 1 < gridSize)
                    {
                        Vector3 v1 = grid[frontIndex].vertex;
                        int nextX = flattenIndex(x, y + 1, z + 1);
                        int nextY = flattenIndex(x, y + 1, z);

                        if (v1 != Vector3.zero && grid[nextX].vertex != Vector3.zero && grid[nextY].vertex != Vector3.zero)
                        {
                            AddQuad(v0, v1, grid[nextX].vertex, grid[nextY].vertex);
                        }
                        else
                        {
                            //Debug.Log($"[Missing Quad] Skipped at ({x},{y},{z}) due to missing vertex v3");
                        }
                    }
                }
            }
        }

    }

    public void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int startIdx = VertexBuffer.Count;



        VertexBuffer.Add(v0);
        VertexBuffer.Add(v1);
        VertexBuffer.Add(v2);
        VertexBuffer.Add(v3);

        TriangleBuffer.Add(startIdx);
        TriangleBuffer.Add(startIdx + 2);
        TriangleBuffer.Add(startIdx + 1);

        TriangleBuffer.Add(startIdx);
        TriangleBuffer.Add(startIdx + 3);
        TriangleBuffer.Add(startIdx + 2);

    }


    public override Mesh ConstructMesh(Vector3 position, float size, int pvoxelSize)
    {
        gridSize = (int)size;
        voxelSize = pvoxelSize;
        Mesh mesh = new Mesh();

        VertexBuffer.Clear();
        TriangleBuffer.Clear();

        grid = new Voxel[gridSize * gridSize * gridSize];

        SurfaceNets(position);

        mesh.vertices = VertexBuffer.ToArray();
        mesh.triangles = TriangleBuffer.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

       
       
        return mesh;
    }
}
