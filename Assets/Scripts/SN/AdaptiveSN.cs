using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveSN : MonoBehaviour
{
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



}
