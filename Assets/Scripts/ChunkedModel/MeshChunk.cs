using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class MeshChunk
{
    public Vector3 centre;
    public float chunkSize;
    public Vector3Int ID;


    public Mesh mesh;


    private MeshGenerator algorithm;

    GameObject meshOBJ;

    public MeshChunk(Vector3Int coord, Vector3 centre, float size, MeshGenerator algorithm)
    {
        ID = coord;
        this.centre = centre;
        chunkSize = size;

        //mesh = new Mesh();

        this.algorithm = algorithm;

        mesh = algorithm.ConstructMesh(ID, chunkSize, 1);

        meshOBJ = new GameObject("Chunk " + coord);
        //meshOBJ.transform.position = coord - new Vector3(size / 2f, size / 2f, size / 2f);


        MeshFilter meshFilter = meshOBJ.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshOBJ.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;


        meshRenderer.material = new Material(Shader.Find("Standard"));


    }


    public void BuildMesh()
    {

    }
}

