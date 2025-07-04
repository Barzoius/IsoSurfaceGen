using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class InfWorld : MonoBehaviour
{
    //256 MC CPU still runs

    public const float viewDist =  124;

    public Transform cam;
    public static Vector3 camPos;

    [SerializeField]
    private MeshGenerator algorithm;

    int chunkSize;
    int visibleChunks;

    private bool initialChunksGenerated = false;
    private Stopwatch stopwatch = new Stopwatch();

    public Dictionary<Vector3, Chunk> chunkDir = new Dictionary<Vector3, Chunk>();
    List<Chunk> chunks = new List<Chunk>();



    void Start()
    {
        chunkSize = 64;
        visibleChunks = Mathf.RoundToInt(viewDist / chunkSize);

        stopwatch.Start();
    }


    private void Update()
    {
        camPos = cam.position;

       
        if (!initialChunksGenerated)
        {
            UpdateVisibleChunks();
            stopwatch.Stop();
            UnityEngine.Debug.Log($"Initial chunk generation took {stopwatch.ElapsedMilliseconds} ms");
            initialChunksGenerated = true;
        }
        else
        {
            UpdateVisibleChunks(); 
        }
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
                    Vector3 chunkCoord = new Vector3(currentChunkX + offx, currentChunkY + offy, currentChunkZ + offz);

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
                        var chunkGenerator = Instantiate(algorithm);
                        chunkDir.Add(chunkCoord, new Chunk(chunkCoord, chunkSize, chunkGenerator));
                    }
                }
            }
        }



    }


    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.gray;

    //    foreach (var chunk in chunkDir.Values)
    //    {
    //        Gizmos.DrawWireCube(chunk.pos, Vector3.one * chunkSize);
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


        public MeshGenerator meshGenerator;
        public Chunk(Vector3 coord, float csize, MeshGenerator generator)
        {
            size = csize;
            pos = coord * size;

            meshGenerator = generator;

            bounds = new Bounds(pos, Vector3.one * size);
  

            chunkMesh = meshGenerator.ConstructMesh(pos, size, 1);
         
            meshOBJ = new GameObject("Chunk " + pos);

            meshOBJ.transform.position = Vector3.zero;

            int terrainLayerIndex = LayerMask.NameToLayer("Terrain");
            if (terrainLayerIndex != -1)
            {
                meshOBJ.layer = terrainLayerIndex;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Layer 'Terrain' not found. Please create it in Tags & Layers.");
            }

            MeshCollider collider = meshOBJ.AddComponent<MeshCollider>();
            collider.sharedMesh = chunkMesh;

            MeshFilter meshFilter = meshOBJ.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshOBJ.AddComponent<MeshRenderer>();

            meshFilter.mesh = chunkMesh;


            meshRenderer.material = new Material(Shader.Find("Unlit/tth"));

            meshOBJ.SetActive(false);
        }

        public void RegenerateMesh()
        {
            
            chunkMesh = meshGenerator.ConstructMesh(pos, size, 1);
            meshOBJ.GetComponent<MeshFilter>().mesh = chunkMesh;

            MeshCollider collider = meshOBJ.GetComponent<MeshCollider>();
            if (collider != null)
            {
                collider.sharedMesh = chunkMesh;
            }
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
