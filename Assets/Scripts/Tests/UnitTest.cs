using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DC;

public class UnitTest
{
    [Test]
    public void TestDCVPSimplePasses()
    {
        // Use the Assert class to test conditions
    }


    [UnityTest]
    public IEnumerator TestDCVPWithEnumeratorPasses()
    {

        yield return null;
    }

    [UnityTest]
    public IEnumerator VertexPosition_WithinVoxelBounds()
    {

        GameObject go = new GameObject();
        var dc = go.AddComponent<SurfaceNets>();
        dc.spherePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);  // Needed to avoid null reference

        yield return null; 

        var grid = dc.GetVoxelGrid();
        int gridSize = dc.GetGridSize();
        int voxelSize = dc.GetVoxelSize();

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    int index = x * gridSize * gridSize + y * gridSize + z;
                    var voxel = grid[index];

                    Vector3 vertex = voxel.vertex;
                    if (vertex == Vector3.zero) continue; // Skip empty voxels

                    Vector3 min = new Vector3(x * voxelSize, y * voxelSize, z * voxelSize);
                    Vector3 max = min + Vector3.one * voxelSize;

                    Assert.IsTrue(
                        vertex.x >= min.x && vertex.x <= max.x &&
                        vertex.y >= min.y && vertex.y <= max.y &&
                        vertex.z >= min.z && vertex.z <= max.z,
                        $"Vertex {vertex} out of bounds for voxel at ({x},{y},{z})"
                    );
                }
            }
        }
    }
}
