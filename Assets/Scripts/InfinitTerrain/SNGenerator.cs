using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SurfaceNetsGenerator", menuName = "MeshGenerators/SurfaceNets")]
public class SMGenerator : MeshGenerator
{

    public static int gridSize;
    public static int voxelSize = 1;

    private List<Vector3> VertexBuffer = new List<Vector3>();
    private List<int> TriangleBuffer = new List<int>();
    private List<int> QuadBuffer = new List<int>();

    public struct Edge
    {
        public Vector3 normal;
        public Vector3 intersection;
        public bool crossed;
    }

    public struct Voxel
    {
        public float[] densities;
        public Vector3[] cornerPositions;
        public Edge[] edgeData;
        public int INDEX;
        public Vector3 vertex;
        public int vid;
    }

    public static readonly int[,] edges = new int[12, 2]
    {
        {0,1},{2,3},{4,5},{6,7},
        {0,2},{1,3},{4,6},{5,7},
        {0,4},{1,5},{2,6},{3,7}
    };

    private Voxel[] grid;

    private static readonly Vector3[] cornerOffsets = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(1, 1, 0),
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(0, 1, 1),
        new Vector3(1, 1, 1)
    };

    private bool IsValidCoord(int x) => x >= 0 && x <= gridSize;
    private int flattenIndex(int x, int y, int z)
    {
        int size = gridSize + 1;
        Debug.Assert(x >= 0 && x < size);
        Debug.Assert(y >= 0 && y < size);
        Debug.Assert(z >= 0 && z < size);
        return x * size * size + y * size + z;
    }


    float SampleSDF(Vector3 position)
    {
        float scale = 0.2f;
        float heightMultiplier = 15f; //  max height

        float height = Mathf.PerlinNoise(position.x * scale, position.z * scale) * heightMultiplier;

        return position.y - height;
    }

    //float SampleSDF(Vector3 position)
    //{
    //    float planeHeight = 2f;
    //    return position.y - planeHeight;
    //}


    public override void Edit(Vector3 point, float density, float radius)
    {
    }

        
    private Vector3 computeGradient(float x, float y, float z)
    {
        float eps = 0.01f;
        float dx = SampleSDF(new Vector3(x + eps, y, z)) - SampleSDF(new Vector3(x - eps, y, z));
        float dy = SampleSDF(new Vector3(x, y + eps, z)) - SampleSDF(new Vector3(x, y - eps, z));
        float dz = SampleSDF(new Vector3(x, y, z + eps)) - SampleSDF(new Vector3(x, y, z - eps));
        return Vector3.Normalize(new Vector3(dx, dy, dz));
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

                Vector3 intersectionPoint = Vector3.Lerp(voxel.cornerPositions[vertex1],
                                                         voxel.cornerPositions[vertex2], t);

                Vector3 normal = computeGradient(intersectionPoint.x, intersectionPoint.y, intersectionPoint.z);

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

    void InitGridData(Vector3 position)
    {
        for (int x = 0; x <= gridSize; x++)
        {
            for (int y = 0; y <= gridSize; y++)
            {
                for (int z = 0; z <= gridSize; z++)
                {
                    Voxel voxel = new Voxel
                    {
                        densities = new float[8],
                        edgeData = new Edge[12],
                        cornerPositions = new Vector3[8],
                        INDEX = 0,
                        vertex = Vector3.zero,
                        vid = -1
                    };

                    Vector3 basePos = position + new Vector3(x * voxelSize, y * voxelSize, z * voxelSize);

                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 cornerPos = basePos + cornerOffsets[i] * voxelSize;
                        voxel.cornerPositions[i] = cornerPos;
                        voxel.densities[i] = SampleSDF(cornerPos);
                        if (voxel.densities[i] < 0f)
                            voxel.INDEX |= (1 << i);
                    }

                    int index = flattenIndex(x, y, z);
                    grid[index] = voxel;

                    initEdgeData(grid[index]);

                    Vector3 avg = Vector3.zero;
                    int n = 0;
                    foreach (Edge edge in grid[index].edgeData)
                    {
                        if (edge.crossed)
                        {
                            avg += edge.intersection;
                            n++;
                        }
                    }

                    if (n > 0)
                    {
                        avg /= n;
                        grid[index].vertex = avg;
                        grid[index].vid = VertexBuffer.Count;
                        VertexBuffer.Add(avg);

                        // Optional debug
                        // Instantiate(spherePrefab, avg, Quaternion.identity);
                    }
                }
            }
        }
    }
    private int getVertexID(Vector3 voxelCoord)
    {
        int x = (int)voxelCoord.x;
        int y = (int)voxelCoord.y;
        int z = (int)voxelCoord.z;

        if (!IsValidCoord(x) || !IsValidCoord(y) || !IsValidCoord(z))
            return -1;

        return grid[flattenIndex(x, y, z)].vid;
    }

    void SurfaceNets(Vector3 position)
    {
        for (int x = 0; x <= gridSize; x++)
        {
            for (int y = 0; y <= gridSize; y++)
            {
                for (int z = 0; z <= gridSize; z++)
                {
                    int index = flattenIndex(x, y, z);
                    if (grid[index].vid == -1) continue;

                    Vector3 here = new Vector3(x, y, z);
                    bool solid = SampleSDF(position + here * voxelSize) < 0;

                    for (int dir = 0; dir < 3; dir++)
                    {
                        int axis1 = 1 << dir;
                        int axis2 = 1 << ((dir + 1) % 3);
                        int axis3 = 1 << ((dir + 2) % 3);

                        Vector3 a1 = cornerOffsets[axis1];
                        Vector3 a2 = cornerOffsets[axis2];
                        Vector3 a3 = cornerOffsets[axis3];

                        Vector3 p0 = position + here * voxelSize;
                        Vector3 p1 = position + (here + a1) * voxelSize;

                        if (SampleSDF(p0) * SampleSDF(p1) > 0)
                            continue;

                        Vector3 v0 = here;
                        Vector3 v1 = here - a2;
                        Vector3 v2 = v1 - a3;
                        Vector3 v3 = here - a3;

                        int i0 = getVertexID(v0);
                        int i1 = getVertexID(v1);
                        int i2 = getVertexID(v2);
                        int i3 = getVertexID(v3);

                        if (i0 == -1 || i1 == -1 || i2 == -1 || i3 == -1)
                            continue;


                        if (!solid)
                            (i1, i3) = (i3, i1);

                        QuadBuffer.Add(i0);
                        QuadBuffer.Add(i1);
                        QuadBuffer.Add(i2);
                        QuadBuffer.Add(i3);
                    }
                }
            }
        }

    }


    void GenerateMeshFromBuffers()
    {
        //Debug.Log($"VertexBuffer: {VertexBuffer.Count}, QuadBuffer: {QuadBuffer.Count}");

        if (VertexBuffer.Count == 0 || QuadBuffer.Count < 4)
        {
            //Debug.LogWarning("Empty buffers � skipping mesh generation.");
            return;
        }

        for (int i = 0; i < QuadBuffer.Count; i += 4)
        {
            int i0 = QuadBuffer[i];
            int i1 = QuadBuffer[i + 1];
            int i2 = QuadBuffer[i + 2];
            int i3 = QuadBuffer[i + 3];

            TriangleBuffer.Add(i0);
            TriangleBuffer.Add(i1);
            TriangleBuffer.Add(i2);

            TriangleBuffer.Add(i2);
            TriangleBuffer.Add(i3);
            TriangleBuffer.Add(i0);
        }



    }




    public override Mesh ConstructMesh(Vector3 position, float size, int pvoxelSize)
    {
        gridSize = (int)size;
        voxelSize = pvoxelSize;
        Mesh mesh = new Mesh();

        VertexBuffer.Clear();
        TriangleBuffer.Clear();
        QuadBuffer.Clear();


        grid = new Voxel[(gridSize + 1) * (gridSize + 1) * (gridSize + 1)];

        InitGridData(position);

        SurfaceNets(position);

        GenerateMeshFromBuffers();

        //Debug.Log($"Chunk origin: {position}, First vertex: {VertexBuffer[0]}");

        mesh.vertices = VertexBuffer.ToArray();
        mesh.triangles = TriangleBuffer.ToArray();
        mesh.RecalculateNormals();
        //mesh.RecalculateTangents();



        return mesh;
    }
}
