using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SurfaceNetsGPUGenerator", menuName = "MeshGenerators/SurfaceNetsGPU")]
public class SNGPUGenerator : MeshGenerator
{

    
    

  

    public override Mesh ConstructMesh(Vector3 position, float size, int pvoxelSize)
    {
       
        Mesh mesh = new Mesh();

        return mesh;
    }
}
