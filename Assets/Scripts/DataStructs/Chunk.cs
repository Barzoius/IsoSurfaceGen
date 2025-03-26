//using UnityEngine;
//using System.Collections.Generic;
//using static UnityEngine.EventSystems.EventTrigger;


//public class Chunk
//{
//    public Vector3 center;
//    public float size;
//    public Vector3Int id;

//    GameObject meshOBJ;

//    Bounds bounds;

//    public Chunk(Vector3 coord)
//    {
//        center = coord * size;

//        bounds = new Bounds(center, Vector3.one * size);

//        meshOBJ = GameObject.CreatePrimitive(PrimitiveType.Plane);
//        meshOBJ.transform.position = center;
//        meshOBJ.transform.localScale = Vector3.one * size / 10f;
//    }

//    public void Update()
//    {
//        bounds.SqrDistance()
//    }

//    public void BuildMesh()
//    {

//    }

//    public void DrawBoundsGizmo(Color col)
//    {
//        Gizmos.color = col;
//        Gizmos.DrawWireCube(center, Vector3.one * size);
//    }

//}
