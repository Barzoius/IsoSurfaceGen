using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveSN : MonoBehaviour
{
    private List<Vector3> VertexBuffer = new List<Vector3>();
    private List<int> TriangleBuffer = new List<int>();

    public GameObject spherePrefab;
    public int octreeSize = 8; 
    public int depth = 2;

    void Start()
    {
        GenerateOctree();
    }

    void GenerateOctree()
    {
        Octree octree = new Octree();
        octree.size = octreeSize;
        octree.depth = depth;

        Node rootNode = new Node
        {
            size = octreeSize,
            min = Vector3.zero
        };


        octree.ConstructOctree(rootNode);

        List<Vector3> vertices = new List<Vector3>();
        TraverseTree(rootNode, vertices);


        foreach (Vector3 vertex in vertices)
        {
            Instantiate(spherePrefab, vertex, Quaternion.identity);
        }
    }
    public void TraverseTree(Node node, List<Vector3> vertices)
    {
        if (node == null) return;


        if (node.size == 1)
        {
            if (node.nodeData.vertex != Vector3.zero)
            {
                vertices.Add(node.nodeData.vertex);
            }
            return;
        }

        for (int i = 0; i < 8; i++)
        {
            TraverseTree(node.children[i], vertices);
        }
    }


    void Polygonize(Node node)
    {
        if (node == null) return;



    }






    public void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int startIdx = VertexBuffer.Count;



        VertexBuffer.Add(v0);
        VertexBuffer.Add(v1);
        VertexBuffer.Add(v2);
        VertexBuffer.Add(v3);

        TriangleBuffer.Add(startIdx);
        TriangleBuffer.Add(startIdx + 2);
        TriangleBuffer.Add(startIdx + 1);

        TriangleBuffer.Add(startIdx);
        TriangleBuffer.Add(startIdx + 3);
        TriangleBuffer.Add(startIdx + 2);

    }

    void GenerateMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (!meshFilter)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (!meshRenderer)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Custom/doubleSided"));
        }

        meshFilter.mesh = mesh;
    }


}
