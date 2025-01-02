using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MC_LOGISTICS;

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

    float sampleSDF(int x, int y, int z)
    {
        float radius = 40.0f; 
        UnityEngine.Vector3 center = new UnityEngine.Vector3(gridSize * voxelSize / 2, gridSize * voxelSize / 2, gridSize * voxelSize / 2);
        return UnityEngine.Vector3.Distance(new UnityEngine.Vector3(x, y, z), center) - radius;
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

                        cornerPositions = new UnityEngine.Vector3[8]
                    };

                    for (int corner = 0; corner < 8; corner++)
                    {
                        int corner_x = x * voxelSize + (corner & 1) * voxelSize;
                        int corner_y = y * voxelSize + ((corner >> 1) & 1) * voxelSize;
                        int corner_z = z * voxelSize + ((corner >> 2) & 1) * voxelSize;

                        voxel.cornerPositions[corner] = new UnityEngine.Vector3(corner_x, corner_y, corner_z);
                        voxel.densities[corner] = sampleSDF(corner_x, corner_y, corner_z); ;


                        UnityEngine.Vector3 position = new UnityEngine.Vector3(corner_x, corner_y, corner_z);

                        GameObject newSphere = Instantiate(spherePrefab, position, UnityEngine.Quaternion.identity);

                        Renderer renderer = newSphere.GetComponent<Renderer>();

                        if (renderer != null)
                        {
                            if (sampleSDF(corner_x, corner_y, corner_z) > 0)
                            {
                                renderer.material.color = UnityEngine.Color.white;
                                newSphere.SetActive(false);
                            }
                            else if (sampleSDF(corner_x, corner_y, corner_z) < 0)
                            {
                                renderer.material.color = UnityEngine.Color.black;
                                newSphere.SetActive(true);
                            }
                            else if (sampleSDF(corner_x, corner_y, corner_z) == 0)
                            {
                                renderer.material.color = UnityEngine.Color.green;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Renderer not found on the instantiated object.");
                        }

                        //Instantiate(spherePrefab, position, UnityEngine.Quaternion.identity);
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
