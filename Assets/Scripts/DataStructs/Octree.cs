using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Edge
{
    public UnityEngine.Vector3 normal;
    public UnityEngine.Vector3 intersection;
    public bool crossed;
}

public class drawData
{
    public float[] densities;
    public UnityEngine.Vector3[] cornerPositions;
    public Edge[] edgeData;
}

public class Node
{
    public int size;
    public Node[] children = new Node[8];
    public drawData[] nodeData;

    Node() 
    {
        nodeData = null;
        size = 0;

        for (int i = 0; i < 8; i++)
        {
            children[i] = null;
        }
    }

}

public class Octree
{
    public Vector3 center;
    public float size;
    public int depth;

    public Node ConstructLeafNode(Node node)
    {
        return null;
    }

    public Node ConstructOctree(Node node)
    {
        if (node == null) { return null;}

        if(node.size == 1)
        {
           return ConstructLeafNode (node);
        }

        return null;
    }
}
