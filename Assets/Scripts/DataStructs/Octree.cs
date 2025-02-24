using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Edge
{
    public UnityEngine.Vector3 normal;
    public UnityEngine.Vector3 intersection;
    public bool crossed;
}

public struct drawData
{
    public int index;
    public float[] densities;
    public Vector3 position;
    public Vector3[] cornerPositions;
    public Edge[] edgeData;

    public Vector3 vertex;
}

public class Node
{
    public int size;
    public Node[] children = new Node[8];
    public drawData nodeData;
    public Vector3 min;

    public Node()
    {
        size = 0;

        for (int i = 0; i < 8; i++)
        {
            children[i] = null;
        }

  
        nodeData = new drawData
        {
            densities = new float[8],           
            cornerPositions = new Vector3[8],   
            edgeData = new Edge[12]             
        };
    }
}


public class Octree
{
    public Vector3 center;
    public int size;
    public int depth;

    public GameObject spherePrefab;

    float SampleSDF(Vector3 position)
    {
        float radius = 10.0f;
        Vector3 center = new Vector3(size / 2, size / 2, size / 2);
        return Vector3.Distance(position, center) - radius;
    }

    UnityEngine.Vector3 computeGradient(float x, float y, float z)
    {
        float epsilon = 0.01f;
        float dx = SampleSDF(new Vector3((int)(x + epsilon), (int)y, (int)z)) - SampleSDF(new Vector3((int)(x - epsilon), (int)y, (int)z));
        float dy = SampleSDF(new Vector3((int)x, (int)(y + epsilon), (int)z)) - SampleSDF(new Vector3((int)x, (int)(y - epsilon), (int)z));
        float dz = SampleSDF(new Vector3((int)x, (int)y, (int)(z + epsilon))) - SampleSDF(new Vector3((int)x, (int)y, (int)(z - epsilon)));


        return UnityEngine.Vector3.Normalize(new UnityEngine.Vector3(dx, dy, dz));
    }

    private static readonly Vector3[] cornerOffsets = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(1, 1, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1)
    };

    public int[,] edges = new int[12, 2]
    {
            {0,4},{1,5},{2,6},{3,7},	// x-axis 
			{0,2},{1,3},{4,6},{5,7},	// y-axis
			{0,1},{2,3},{4,5},{6,7}     // z-axis
    };


    public Node ConstructLeafNode(Node leaf)
    {
        for(int i = 0; i < 8; i++)
        {
            Vector3 cornerPos = leaf.min + cornerOffsets[i];
            float density = SampleSDF(cornerPos);
            

            int index = 0;

            if (density < 0f)
            {
                index |= (1 << i);
            }

            leaf.nodeData.cornerPositions[i] = cornerPos;
            leaf.nodeData.densities[i] = density;
            leaf.nodeData.index = index;

            initEdgeData(leaf);

            Vector3 C = new Vector3();
            int n = 0;
            foreach (Edge edge in leaf.nodeData.edgeData)
            {
                if (edge.crossed)
                {
                    n++;
                    C += edge.intersection;
                }
            }


            if (n > 0)
            {
                C = C / n;
                leaf.nodeData.vertex = new Vector3(C.x, C.y, C.z);
                //Debug.Log("vertex position :\n" + C);
          
            }
            else
            {

                //Debug.Log("N = " + n);

            }

        }

        return leaf;
    }

    void initEdgeData(Node node)
    {
        for (int i = 0; i < 12; i++)
        {
            int vertex1 = edges[i, 0];
            int vertex2 = edges[i, 1];

            float sampleV1 = node.nodeData.densities[vertex1];
            float sampleV2 = node.nodeData.densities[vertex2];

            if (Mathf.Abs(sampleV1 - sampleV2) > 0.0001f && (sampleV1 < 0) != (sampleV2 < 0))
            {
                float t = sampleV1 / (sampleV1 - sampleV2);

                UnityEngine.Vector3 intersectionPoint =
                    UnityEngine.Vector3.Lerp(node.nodeData.cornerPositions[vertex1],
                                                 node.nodeData.cornerPositions[vertex2], t);

                UnityEngine.Vector3 normal = computeGradient(intersectionPoint.x, intersectionPoint.y, intersectionPoint.z);

                node.nodeData.edgeData[i] = new Edge
                {
                    intersection = intersectionPoint,
                    normal = normal,
                    crossed = true
                };
            }
            else
            {
                node.nodeData.edgeData[i] = new Edge
                {
                    crossed = false
                };
            }
        }
    }


    public Node ConstructOctree(Node node)
    {
        if (node == null) { return null;}

        if(node.size == 1)
        {
           return ConstructLeafNode(node);
        }

        int childSize = node.size / 2;

        bool hasOffsprings = false;

        for(int i = 0; i < 8; i++)
        {
            Node child = new Node();
            child.size = childSize;
            child.min = node.min + (cornerOffsets[i] * childSize);

            node.children[i] = ConstructOctree(child);


            hasOffsprings |= (node.children[i] != null);
        }

        if(!hasOffsprings)
        {
            node = null;
            return null;
        }

        return node;
    }


    

}
