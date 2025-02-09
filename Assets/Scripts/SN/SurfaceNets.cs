using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SurfaceNets : MonoBehaviour
{
    public static int gridSize = 16;
    public static int voxelSize = 5;

    public GameObject spherePrefab;

    private List<Vector3> surfacePoints = new List<Vector3>();

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
    }

    public int[,] edges = new int[12, 2]
    {
        {0, 1}, {1, 2}, {2, 3}, {3, 0}, // Bottom edges
        {4, 5}, {5, 6}, {6, 7}, {7, 4}, // Top edges
        {0, 4}, {1, 5}, {2, 6}, {3, 7}  // Side edges
    };

    Voxel[] grid = new Voxel[gridSize * gridSize * gridSize];

    int flattenIndex(int x, int y, int z)
    {
        return x * gridSize * gridSize + y * gridSize + z;
    }

    float sampleSDF(int x, int y, int z)
    {
        float radius = 20.0f;
        UnityEngine.Vector3 center = new UnityEngine.Vector3(gridSize * voxelSize / 2, gridSize * voxelSize / 2, gridSize * voxelSize / 2);
        return UnityEngine.Vector3.Distance(new UnityEngine.Vector3(x, y, z), center) - radius;
    }

    UnityEngine.Vector3 computeGradient(float x, float y, float z)
    {
        float epsilon = 0.01f;
        float dx = sampleSDF((int)(x + epsilon), (int)y, (int)z) - sampleSDF((int)(x - epsilon), (int)y, (int)z);
        float dy = sampleSDF((int)x, (int)(y + epsilon), (int)z) - sampleSDF((int)x, (int)(y - epsilon), (int)z);
        float dz = sampleSDF((int)x, (int)y, (int)(z + epsilon)) - sampleSDF((int)x, (int)y, (int)(z - epsilon));

        return UnityEngine.Vector3.Normalize(new UnityEngine.Vector3(dx, dy, dz));
    }

    UnityEngine.Vector3 ComputeTransformedAB(UnityEngine.Matrix4x4 A, UnityEngine.Vector3 b)
    {
        // Initialize the augmented matrix
        float[,] M = new float[4, 4];
        for (int i = 0; i < 3; i++)
        {
            M[i, 0] = A[i, 0];
            M[i, 1] = A[i, 1];
            M[i, 2] = A[i, 2];
            M[i, 3] = b[i];
        }

        // Apply Givens rotations
        for (int col = 0; col < 3; col++)
        {
            for (int row = col + 1; row < 4; row++)
            {
                float a = M[col, col];
                float B = M[row, col];
                float r = Mathf.Sqrt(a * a + B * B);
                float c = a / r;
                float s = -B / r;

                for (int k = 0; k < 4; k++) // Rotate columns
                {
                    float tempCol = c * M[col, k] - s * M[row, k];
                    M[row, k] = s * M[col, k] + c * M[row, k];
                    M[col, k] = tempCol;
                }
            }
        }

        // Extract results
        UnityEngine.Matrix4x4 hatA = new UnityEngine.Matrix4x4();
        UnityEngine.Vector3 hatB = new UnityEngine.Vector3();
        float R = M[3, 3];

        for (int i = 0; i < 3; i++)
        {
            hatA[i, 0] = M[i, 0];
            hatA[i, 1] = M[i, 1];
            hatA[i, 2] = M[i, 2];
            hatB[i] = M[i, 3];
        }

        UnityEngine.Vector3 position = hatA * hatB;

        return position;
    }


    void Start()
    {
        int cnt = 0;
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
                        cornerPositions = new UnityEngine.Vector3[8]
                    };

                    for (int corner = 0; corner < 8; corner++)
                    {
                        int corner_x = x * voxelSize + (corner & 1) * voxelSize;
                        int corner_y = y * voxelSize + ((corner >> 1) & 1) * voxelSize;
                        int corner_z = z * voxelSize + ((corner >> 2) & 1) * voxelSize;

                        voxel.cornerPositions[corner] = new UnityEngine.Vector3(corner_x, corner_y, corner_z);
                        voxel.densities[corner] = sampleSDF(corner_x, corner_y, corner_z);
                    }

                    int index = flattenIndex(x, y, z);
                    grid[index] = voxel;

                    // Initialize edge data for each voxel
                    initEdgeData(grid[index]);

                    // Accumulate the matrix and vector
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
                        Debug.Log("vertex position :\n" + C);
                        surfacePoints.Add(C);
                        Instantiate(spherePrefab, C, UnityEngine.Quaternion.identity);
                    }

                }
            }
        }
    }



    void initEdgeData(Voxel voxel)
    {
        for (int i = 0; i < 12; i++)
        {
            int vertex1 = edges[i, 0];
            int vertex2 = edges[i, 1];

            float sampleV1 = voxel.densities[vertex1];
            float sampleV2 = voxel.densities[vertex2];

            if ((sampleV1 < 0 && sampleV2 > 0) || (sampleV1 > 0 && sampleV2 < 0))
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

    // Update is called once per frame
    void Update()
    {

    }

}