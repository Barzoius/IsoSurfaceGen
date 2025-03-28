using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InfWorld;

public class ChunkedMesh : MonoBehaviour
{

    int numChunks = 3;
    MeshChunk[] chunks;
    float chunkSize;

    float meshBB = 10;

    [SerializeField]
    private MeshGenerator algorithm;

    void Start()
    {
        InitChunks();

        //for(int i = 0; i < chunks.Length; i++)
        //{
        //    chunks[i].BuildMesh();
        //}
    }

    
    void Update()
    {
        
    }


    void InitChunks() 
    {
        chunks = new MeshChunk[numChunks * numChunks * numChunks];

        chunkSize = meshBB / numChunks;

        int i = 0;

        for (int y = 0; y < numChunks; y++)
        {
            for (int x = 0; x < numChunks; x++)
            {
                for (int z = 0; z < numChunks; z++)
                {
                    Vector3Int coord = new Vector3Int(x, y, z);

                    float posX = (-(numChunks - 1f) / 2 + x) * chunkSize;
                    float posY = (-(numChunks - 1f) / 2 + y) * chunkSize;
                    float posZ = (-(numChunks - 1f) / 2 + z) * chunkSize;

                    Vector3 centre = new Vector3(posX, posY, posZ);

                    MeshChunk chunk = new MeshChunk(coord, centre, chunkSize, algorithm);

                    chunks[i] = chunk;
                    i++;
                }
            }
        }

    }

    void OnDrawGizmos()
    {
        if (chunks == null) return; 

        Gizmos.color = Color.green;

        foreach (var chunk in chunks)
        {
            if (chunk != null) 
            {
                Gizmos.DrawWireCube(chunk.centre, Vector3.one * chunkSize);
            }
        }
    }
}
