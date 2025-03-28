using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using MC_LOGISTICS;

[CreateAssetMenu(fileName = "MarchingCubesGenerator", menuName = "MeshGenerators/MarchingCubes")]
public class MCGenerator : MeshGenerator
{

    public static int gridSize;
    public static int voxelSize;

    public static float isosurface = 0f;

    List<Vector3[]> triangles = new List<Vector3[]>();

    struct Voxel
    {
        public float[] densities;
        public Vector3[] cornerPositions;
        public int INDEX;
        public List<Vector3> TRIANGLES;
    }


    Voxel[] grid;

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

    int FlattenIndex(int x, int y, int z) => x * gridSize * gridSize + y * gridSize + z;

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

    Vector3 VertexLerp(Vector3 p1, Vector3 p2, float v1, float v2)
    {
        float t = (isosurface - v1) / (v2 - v1);
        return p1 + t * (p2 - p1);
    }

    void March(Vector3 position)
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
                        cornerPositions = new Vector3[8],
                        INDEX = 0,
                        TRIANGLES = new List<Vector3>()
                    };

                    // Base position of the voxel
                    Vector3 basePos = new Vector3(x, y, z) * voxelSize;

                 
                    for (int corner = 0; corner < 8; corner++)
                    {
                        Vector3 cornerPos = basePos + cornerOffsets[corner] * voxelSize;
                        voxel.cornerPositions[corner] = cornerPos;
                        voxel.densities[corner] = SampleSDF(cornerPos);

                        if (voxel.densities[corner] < isosurface)
                        {
                            voxel.INDEX |= (1 << corner);
                        }
                    }

                 
                    for (int i = 0; MC_DATA.triTable[voxel.INDEX, i] != -1; i += 3)
                    {
                        int a0 = MC_DATA.cornerIndexAFromEdge[MC_DATA.triTable[voxel.INDEX, i]];
                        int b0 = MC_DATA.cornerIndexBFromEdge[MC_DATA.triTable[voxel.INDEX, i]];

                        int a1 = MC_DATA.cornerIndexAFromEdge[MC_DATA.triTable[voxel.INDEX, i + 1]];
                        int b1 = MC_DATA.cornerIndexBFromEdge[MC_DATA.triTable[voxel.INDEX, i + 1]];

                        int a2 = MC_DATA.cornerIndexAFromEdge[MC_DATA.triTable[voxel.INDEX, i + 2]];
                        int b2 = MC_DATA.cornerIndexBFromEdge[MC_DATA.triTable[voxel.INDEX, i + 2]];

                        triangles.Add(new Vector3[]
                        {
                            VertexLerp(voxel.cornerPositions[a0], voxel.cornerPositions[b0], voxel.densities[a0], voxel.densities[b0]),
                            VertexLerp(voxel.cornerPositions[a1], voxel.cornerPositions[b1], voxel.densities[a1], voxel.densities[b1]),
                            VertexLerp(voxel.cornerPositions[a2], voxel.cornerPositions[b2], voxel.densities[a2], voxel.densities[b2])
                        });
                    }
                
                    int index = FlattenIndex(x, y, z);
                    grid[index] = voxel;

                }
            }
        }
    }


    public override Mesh ConstructMesh(Vector3 position, float size, int pvoxelSize)
    {
        gridSize = (int)size;
        voxelSize = pvoxelSize;
        Mesh mesh = new Mesh();

        triangles.Clear();

        grid = new Voxel[gridSize * gridSize * gridSize];

        March(position);

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        foreach (var tri in triangles)
        {
            vertices.Add(tri[0]);
            indices.Add(vertices.Count - 1);

            vertices.Add(tri[2]); // swap order of last two vertices
            indices.Add(vertices.Count - 1);

            vertices.Add(tri[1]);
            indices.Add(vertices.Count - 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();
        //mesh.RecalculateBounds();

        if (vertices.Count == 0)
        {
            Debug.LogWarning($"Mesh at {position} has NO vertices!");
            return mesh;
        }

        return mesh;
    }
}
