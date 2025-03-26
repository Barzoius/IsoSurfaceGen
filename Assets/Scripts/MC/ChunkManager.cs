//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ChunkManager : MonoBehaviour
//{

//    public class Voxel
//    {
//        public Vector3 position;
//        public float density;
//        public float[] densities;
//        public Vector3[] cornerPositions;
//        public int INDEX;
//        public List<Vector3> TRIANGLES;

//        public Voxel(Vector3 pos)
//        {
//            position = pos;
//        }
//    }

//    public class Chunk
//    {
//        public Vector3 color;
//        public Vector3 position;
//        private Voxel[,,] voxels;

//        public Chunk(Vector3 position)
//        {
//            this.position = position;
//        }   
//    }


//    int renderDistance = 1;
//    Vector3 camPos;

//    public int chunkSize = 16; // 16 voxels per chunk
//    public int voxelSize = 4;

//    private Dictionary<Vector3Int, Chunk> Active_Chunks = new Dictionary<Vector3Int, Chunk>();

//    void Start()
//    {
        
//    }

//    void Update()
//    {
//        Debug.Log(Camera.main.transform.position);
//    }

//    Vector3Int GetChunk()
//    {
//        float chunkX = camPos.x / chunkSize;
//        float chunkY = camPos.y / chunkSize;

//        return new Vector3Int((int)chunkX, (int)chunkY, 0);

//    }

//    float GetRenderBounds()
//    {
//        return (float)renderDistance * 2 + 1;
//    }

//    void loadActiveChunks()
//    {
//        float renderSpaceBounds = GetRenderBounds();

//        Vector3 currentChunkPos = GetChunk();

//        for(int x = 0; x < renderSpaceBounds; x++)
//        {
//            for(int y = 0; y < renderSpaceBounds; y++)
//            {
//                float chunkX = (x + 1) - (Mathf.Round(renderSpaceBounds/2)) + currentChunkPos.x;
//                float chunkY = (y + 1) - (Mathf.Round(renderSpaceBounds / 2)) + currentChunkPos.y;

//                Chunk chunk = new Chunk(new Vector3(chunkX, chunkY, 0));

//                Active_Chunks.Add(new Vector3Int((int)chunkX, (int)chunkY, 0), chunk);
//            }
//        }
//    }


//}
