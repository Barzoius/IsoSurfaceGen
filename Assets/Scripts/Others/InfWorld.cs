using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfWorld : MonoBehaviour
{
    public const float viewDist = 32;
    public Transform cam;
    public static Vector3 camPos;

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
                        chunkDir.Add(chunkCoord, new Chunk(chunkCoord, chunkSize));
                    }
                }
            }
        }


    }


    public class Chunk
    {
        public Vector3 center;
        public float size;
        public Vector3Int id;

        GameObject meshOBJ;

        Bounds bounds;

        public Chunk(Vector3 coord, float csize)
        {
            size = csize;
            center = coord * size;

            bounds = new Bounds(center, Vector3.one * size);

            meshOBJ = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshOBJ.transform.position = center;
            meshOBJ.transform.localScale = Vector3.one * size / 10f;

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
    
        public void BuildMesh()
        {

        }

        public void DrawBoundsGizmo(Color col)
        {
            Gizmos.color = col;
            Gizmos.DrawWireCube(center, Vector3.one * size);
        }

    }

}
