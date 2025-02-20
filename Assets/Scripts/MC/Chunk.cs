//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//private class Chunk : MonoBehaviour
//{
//    public Vector3Int position;


//    public Mesh mesh;

//    MeshFilter meshFilter;
//    MeshRenderer meshRenderer;
//    //MeshCollider meshCollider;

//    public Chunk(Vector3Int pos)
//    {
//        position = pos;

//        meshFilter = GetComponent<MeshFilter>();
//        meshRenderer = GetComponent<MeshRenderer>();
//        //meshCollider = GetComponent<MeshCollider>();

//        if (meshFilter == null)
//        {
//            meshFilter = gameObject.AddComponent<MeshFilter>();
//        }

//        if (meshRenderer == null)
//        {
//            meshRenderer = gameObject.AddComponent<MeshRenderer>();
//        }

//        mesh = meshFilter.sharedMesh;
//        if (mesh == null)
//        {
//            mesh = new Mesh();
//            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
//            meshFilter.sharedMesh = mesh;
//        }

//    }

//}
