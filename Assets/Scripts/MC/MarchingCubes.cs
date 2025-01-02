using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes : MonoBehaviour
{
    public static int gridSize = 16;
    public static int voxelSize = 5;

    public GameObject spherePrefab;

    struct Voxel
    {
        public float[] densities;
        public UnityEngine.Vector3[] cornerPositions;
    }

    Voxel[] grid = new Voxel[gridSize * gridSize * gridSize];


    int flattenIndex(int x, int y, int z)
    {
        return x * gridSize * gridSize + y * gridSize + z;
    }

    // Start is called before the first frame update
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

                        cornerPositions = new UnityEngine.Vector3[8]
                    };

                    for (int corner = 0; corner < 8; corner++)
                    {
                        int corner_x = x * voxelSize + (corner & 1) * voxelSize;
                        int corner_y = y * voxelSize + ((corner >> 1) & 1) * voxelSize;
                        int corner_z = z * voxelSize + ((corner >> 2) & 1) * voxelSize;

                        voxel.cornerPositions[corner] = new UnityEngine.Vector3(corner_x, corner_y, corner_z);
                        voxel.densities[corner] = 0.0f;

                        UnityEngine.Vector3 position = new UnityEngine.Vector3(corner_x, corner_y, corner_z);
                        Instantiate(spherePrefab, position, UnityEngine.Quaternion.identity);
                    }

                    int index = flattenIndex(x, y, z);
                    grid[index] = voxel;
                }

            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}