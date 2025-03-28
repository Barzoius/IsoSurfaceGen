using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeshGenerator : ScriptableObject
{
    public abstract Mesh ConstructMesh(Vector3 position, float size, int pvoxelSize);
}