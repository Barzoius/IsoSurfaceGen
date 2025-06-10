using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfWorld : MonoBehaviour
{
    //256 MC CPU still runs

    public const float viewDist = 256;

    public Transform cam;
    public static Vector3 camPos;

    [SerializeField]
    private MeshGenerator algorithm;

    int chunkSize;
    int visibleChunks;

    Dictionary<Vector3, Chunk> chunkDir = new Dictionary<Vector3, Chunk>();
    List<Chunk> chunks = new List<Chunk>();

    void Start()
    {
        chunkSize = 32;
        visibleChunks = Mathf.RoundToInt(viewDist / chunkSize);
    }


    private void Update()
    {
        camPos = new Vector3(cam.position.x, cam.position.y, cam.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        for(int i = 0; i  < chunks.Count; i++)
        {
            chunks[i].SetVisible(false);
        }

        chunks.Clear();

        int currentChunkX = Mathf.RoundToInt(camPos.x / chunkSize);
        int currentChunkY = Mathf.RoundToInt(camPos.y / chunkSize);
        int currentChunkZ = Mathf.RoundToInt(camPos.z / chunkSize);

        for(int offz = -visibleChunks; offz <= visibleChunks; offz++)
        {
            for (int offy = -visibleChunks; offy <= visibleChunks; offy++)
            {
                for (int offx = -visibleChunks; offx <= visibleChunks; offx++)
                {
                    Vector3 chunkCoord = new Vector3(currentChunkX + offx, 0, currentChunkZ + offz);

                    if(chunkDir.ContainsKey(chunkCoord))
                    {
                        chunkDir[chunkCoord].UpdateChunk();

                        if(chunkDir[chunkCoord].IsVisible())
                        {
                            chunks.Add(chunkDir[chunkCoord]);
                        }
                    }
                    else
                    {
                        chunkDir.Add(chunkCoord, new Chunk(chunkCoord, chunkSize, algorithm));
                    }
                }
            }
        }



    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;

    //    foreach (var chunk in chunkDir.Values)
    //    {
    //        Gizmos.DrawWireCube(chunk.pos , Vector3.one * chunkSize);
    //        Debug.Log(chunk.pos);
           
    //    }
    //}


    public class Chunk
    {
        public Vector3 pos; // center
        public float size;
        public Vector3Int id;

        GameObject meshOBJ;

        Mesh chunkMesh;
        Bounds bounds;


        private MeshGenerator meshGenerator;
        public Chunk(Vector3 coord, float csize, MeshGenerator generator)
        {
            size = csize;
            pos = coord * size;

            meshGenerator = generator;

            bounds = new Bounds(pos, Vector3.one * size);
  

            chunkMesh = meshGenerator.ConstructMesh(pos, size, 1);
         

            meshOBJ = new GameObject("Chunk " + pos);

            meshOBJ.transform.position = Vector3.zero; 
 

            MeshFilter meshFilter = meshOBJ.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshOBJ.AddComponent<MeshRenderer>();

            meshFilter.mesh = chunkMesh;


            meshRenderer.material = new Material(Shader.Find("Custom/HeightColored"));

            meshOBJ.SetActive(false);
        }

        public void UpdateChunk()
        {
            float viewerDist = Mathf.Sqrt(bounds.SqrDistance(camPos));

            bool visible = viewerDist <= viewDist;

            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshOBJ.SetActive(visible);
           
        }

        public bool IsVisible()
        {
            return meshOBJ.activeSelf;
        }
    



    }

}
