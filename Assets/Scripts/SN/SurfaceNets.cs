using System;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceNets : MonoBehaviour
{
    public static int gridSize = 32;
    public static int voxelSize = 4;

    public GameObject spherePrefab;

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

    private Voxel[] grid = new Voxel[gridSize * gridSize * gridSize];

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

    private bool IsValidCoord(int x) => x >= 0 && x < gridSize;

    private int flattenIndex(int x, int y, int z)
    {
        Debug.Assert(IsValidCoord(x));
        Debug.Assert(IsValidCoord(y));
        Debug.Assert(IsValidCoord(z));
        return x * gridSize * gridSize + y * gridSize + z;
    }

    private static float CubeSDF(Vector3 position)
    {
        Vector3 center = new Vector3(gridSize * voxelSize / 2, gridSize * voxelSize / 2, gridSize * voxelSize / 2);
        Vector3 halfSize = new Vector3(10f, 10f, 10f);
        Vector3 d = new Vector3(
            Mathf.Abs(position.x - center.x) - halfSize.x,
            Mathf.Abs(position.y - center.y) - halfSize.y,
            Mathf.Abs(position.z - center.z) - halfSize.z
        );
        float outside = Mathf.Max(d.x, Mathf.Max(d.y, d.z));
        float inside = Mathf.Min(Mathf.Max(d.x, Mathf.Max(d.y, d.z)), 0.0f);
        return outside + inside;
    }

    private static float SphereSDF(Vector3 position)
    {
        float radius = 10.0f;
        Vector3 center = new Vector3(gridSize * voxelSize / 2, gridSize * voxelSize / 2, gridSize * voxelSize / 2);
        return Vector3.Distance(position, center) - radius;
    }

    private static float SampleSDF(Vector3 position)
    {
        return Mathf.Max(CubeSDF(position), -SphereSDF(position));
    }

    private Vector3 computeGradient(float x, float y, float z)
    {
        float eps = 0.01f;
        float dx = SampleSDF(new Vector3(x + eps, y, z)) - SampleSDF(new Vector3(x - eps, y, z));
        float dy = SampleSDF(new Vector3(x, y + eps, z)) - SampleSDF(new Vector3(x, y - eps, z));
        float dz = SampleSDF(new Vector3(x, y, z + eps)) - SampleSDF(new Vector3(x, y, z - eps));
        return Vector3.Normalize(new Vector3(dx, dy, dz));
    }

    void Start()
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
                        cornerPositions = new Vector3[8],
                        INDEX = 0,
                        vertex = Vector3.zero,
                        vid = -1
                    };

                    Vector3 basePos = new Vector3(x * voxelSize, y * voxelSize, z * voxelSize);

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

        Polygonize();
        GenerateMeshFromBuffers();
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

    void Polygonize()
    {
        for (int x = 0; x < gridSize - 1; x++)
        {
            for (int y = 0; y < gridSize - 1; y++)
            {
                for (int z = 0; z < gridSize - 1; z++)
                {
                    int index = flattenIndex(x, y, z);
                    if (grid[index].vid == -1) continue;

                    Vector3 here = new Vector3(x, y, z);
                    bool solid = SampleSDF(here * voxelSize) < 0;

                    for (int dir = 0; dir < 3; dir++)
                    {
                        int axis1 = 1 << dir;
                        int axis2 = 1 << ((dir + 1) % 3);
                        int axis3 = 1 << ((dir + 2) % 3);

                        Vector3 a1 = cornerOffsets[axis1];
                        Vector3 a2 = cornerOffsets[axis2];
                        Vector3 a3 = cornerOffsets[axis3];

                        Vector3 p0 = (here) * voxelSize;
                        Vector3 p1 = (here + a1) * voxelSize;

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
        Debug.Log($"VertexBuffer: {VertexBuffer.Count}, QuadBuffer: {QuadBuffer.Count}");

        if (VertexBuffer.Count == 0 || QuadBuffer.Count < 4)
        {
            Debug.LogWarning("Empty buffers – skipping mesh generation.");
            return;
        }

        List<int> triangles = new List<int>();
        for (int i = 0; i < QuadBuffer.Count; i += 4)
        {
            int i0 = QuadBuffer[i];
            int i1 = QuadBuffer[i + 1];
            int i2 = QuadBuffer[i + 2];
            int i3 = QuadBuffer[i + 3];

            triangles.Add(i0);
            triangles.Add(i1);
            triangles.Add(i2);

            triangles.Add(i2);
            triangles.Add(i3);
            triangles.Add(i0);
        }

        GenerateMesh(VertexBuffer, triangles);
    }

    void GenerateMesh(List<Vector3> vertices, List<int> triangles)
    {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();


        GameObject meshObject = new GameObject("Surface Nets Mesh");
        meshObject.transform.position = Vector3.zero;


        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;


        Shader shader = Shader.Find("Standard"); 
        if (shader != null)
        {
            Material material = new Material(shader);
            //material.color = Color.green; 
            meshRenderer.material = material;
        }
        else
        {
            Debug.LogError("Shader not found. Are you using URP or HDRP?");
        }
    }



    void initEdgeData(Voxel voxel)
    {
        for (int i = 0; i < 12; i++)
        {
            int v1 = edges[i, 0];
            int v2 = edges[i, 1];

            float d1 = voxel.densities[v1];
            float d2 = voxel.densities[v2];

            if ((d1 < 0) != (d2 < 0))
            {
                float t = d1 / (d1 - d2);
                Vector3 p = Vector3.Lerp(voxel.cornerPositions[v1], voxel.cornerPositions[v2], t);
                Vector3 normal = computeGradient(p.x, p.y, p.z);

                voxel.edgeData[i] = new Edge { crossed = true, intersection = p, normal = normal };
            }
            else
            {
                voxel.edgeData[i] = new Edge { crossed = false };
            }
        }
    }

    public Voxel[] GetVoxelGrid() => grid;
    public int GetGridSize() => gridSize;
    public int GetVoxelSize() => voxelSize;
}
