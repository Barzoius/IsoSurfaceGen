using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DualContouring : MonoBehaviour
{
    public static int gridSize = 16;
    public static int voxelSize = 5;

    public GameObject spherePrefab;

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

                    foreach (Edge edge in grid[index].edgeData)
                    {
                        if (edge.crossed)
                        {
                            Debug.Log("edge intersection :\n" + edge.intersection);

                            Instantiate(spherePrefab, edge.intersection, UnityEngine.Quaternion.identity);
                        }
                    }

                    //-----PARTICLE BASED

                    //Vector3 C = new Vector3();

                    //int n = 0;
                    //foreach (Edge edge in grid[index].edgeData)
                    //{ 
                    //    if (edge.crossed)
                    //    {
                    //        n++
                    //        C += edge.intersection;
                    //    }
                    //}
                    // C = C/n;

                    //Vector3[] F = new Vector3[8];
                    //for(int i = 0; i < 7; i++)
                    //{
                    //    foreach (Edge edge in grid[index].edgeData)
                    //    {
                    //        Debug.Log("edge intersection :\n" + edge.intersection );
                    //        if (edge.crossed)
                    //        {
                    //            F[i] += edge.normal * (-edge.intersection.magnitude - 
                    //                                    Vector3.Dot(edge.normal, grid[index].cornerPositions[i]));

                    //        }

                    //        Debug.Log("F:\n" + F[i] +  " i: " + i);
                    //    }
                    //}


                    //Vector3 Fl1 = (1 - (C.x / voxelSize)) * F[0] + (C.x / voxelSize) * F[3];
                    //Vector3 Fl2 = (1 - (C.x / voxelSize)) * F[4] + (C.x / voxelSize) * F[7];
                    //Vector3 Fl3 = (1 - (C.x / voxelSize)) * F[1] + (C.x / voxelSize) * F[2];
                    //Vector3 Fl4 = (1 - (C.x / voxelSize)) * F[5] + (C.x / voxelSize) * F[6];

                    //Vector3 Fb1 = (1 - (C.y / voxelSize)) * Fl1 + (C.y / voxelSize) * Fl2;
                    //Vector3 Fb2 = (1 - (C.y / voxelSize)) * Fl3 + (C.y / voxelSize) * Fl4;

                    //Vector3 Force = (1 - (C.z / voxelSize)) * Fb1 + (C.y / voxelSize) * Fb2;


                    //Vector3 pos = Force * 0.05f;

                    //Debug.Log("Force:\n" + Force);

                    //Debug.Log("pos:\n" + pos);
                    //Instantiate(spherePrefab, pos, UnityEngine.Quaternion.identity);




                    //-----QR/SVD ROUTE

                    //UnityEngine.Matrix4x4 A = new UnityEngine.Matrix4x4();
                    //UnityEngine.Vector3 b = new UnityEngine.Vector3();



                    //// Initialize the matrix A to the identity matrix before accumulating
                    //A.SetRow(0, new Vector4(0, 0, 0, 0));
                    //A.SetRow(1, new Vector4(0, 0, 0, 0));
                    //A.SetRow(2, new Vector4(0, 0, 0, 0));
                    //A.SetRow(3, new Vector4(0, 0, 0, 1));  // For homogeneous coordinates

                    //// Accumulate the matrix and vector

                    //foreach (Edge edge in grid[index].edgeData)
                    //{
                    //    if (edge.crossed)
                    //    {
                    //        UnityEngine.Vector3 normal = edge.normal;
                    //        UnityEngine.Vector3 intersectionPoint = edge.intersection;

                    //        // Create the outer product matrix for the normal vector
                    //        UnityEngine.Matrix4x4 normalOuterProduct = new UnityEngine.Matrix4x4();
                    //        normalOuterProduct.m00 = normal.x * normal.x;
                    //        normalOuterProduct.m01 = normal.x * normal.y;
                    //        normalOuterProduct.m02 = normal.x * normal.z;

                    //        normalOuterProduct.m10 = normal.y * normal.x;
                    //        normalOuterProduct.m11 = normal.y * normal.y;
                    //        normalOuterProduct.m12 = normal.y * normal.z;

                    //        normalOuterProduct.m20 = normal.z * normal.x;
                    //        normalOuterProduct.m21 = normal.z * normal.y;
                    //        normalOuterProduct.m22 = normal.z * normal.z;

                    //        // Manually add the outer product to the matrix A
                    //        A.m00 += normalOuterProduct.m00;
                    //        A.m01 += normalOuterProduct.m01;
                    //        A.m02 += normalOuterProduct.m02;
                    //        A.m10 += normalOuterProduct.m10;
                    //        A.m11 += normalOuterProduct.m11;
                    //        A.m12 += normalOuterProduct.m12;
                    //        A.m20 += normalOuterProduct.m20;
                    //        A.m21 += normalOuterProduct.m21;
                    //        A.m22 += normalOuterProduct.m22;

                    //        // Update the vector b
                    //        b += normal * Vector3.Dot(normal, intersectionPoint);
                    //    }
                    //}


                    //if (A.determinant != 0)
                    //{
                    //    UnityEngine.Vector3 position = A.inverse * b;
                    //    Instantiate(spherePrefab, position, UnityEngine.Quaternion.identity);

                    //    Debug.LogWarning("!!!WORKS!!!");
                    //    Debug.Log("Matrix A:\n" + A);
                    //    Debug.Log("Vector b: " + b);
                    //}
                    //else
                    //{
                    //    cnt++;
                    //    // Handle the case where the matrix is singular
                    //    Debug.LogWarning("Matrix A is singular, cannot compute position.");
                    //    Debug.Log("Matrix A:\n" + A);
                    //    Debug.Log("Vector b: " + b);
                    //    Debug.Log("Zeros " + cnt);
                    //}

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
