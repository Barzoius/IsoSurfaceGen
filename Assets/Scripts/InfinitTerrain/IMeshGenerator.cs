using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeshGenerator : ScriptableObject
{
    public abstract Mesh ConstructMesh(Vector3 position, float size, int pvoxelSize);

    public abstract void Edit(Vector3 point, float density, float radius);

}