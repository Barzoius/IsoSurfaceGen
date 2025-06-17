using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class Edit : MonoBehaviour
{
    public float editRadius = 100;
    public float editStrength = 100;
    [SerializeField] private LayerMask terrainLayer;
    public bool subtractMode = false;

    private Camera cam;
    private InfWorld world;

    void Start()
    {
        cam = GetComponent<Camera>();
        world = FindObjectOfType<InfWorld>();

        if (cam == null)
        {
            Debug.LogError("No camera found on this object.");
        }

        if (world == null)
        {
            Debug.LogError("No InfWorld instance found in scene.");
        }
    }

    void Update()
    {


        if (Input.GetMouseButton(0)) // Left-click to edit
        {
            Debug.Log("pressed");
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            //Debug.DrawRay(ray.origin, ray.direction * 200f, Color.red);
            if (Physics.Raycast(ray, out RaycastHit hit, 600f, terrainLayer))
            {
                Debug.Log("hit");

                TryEditChunk(hit.point);
            }
        }

        // Optional toggle between add/subtract mode
        if (Input.GetKeyDown(KeyCode.E))
            subtractMode = !subtractMode;
    }

    void TryEditChunk(Vector3 hitPoint)
    {
        Vector3 chunkCoord = new Vector3(
            Mathf.Floor(hitPoint.x / 64f),
            Mathf.Floor(hitPoint.y / 64f),
            Mathf.Floor(hitPoint.z / 64f)
        );

        if (world.chunkDir.TryGetValue(chunkCoord, out var chunk))
        {
            var generator = chunk.meshGenerator as MCGPUGenerator;
            if (generator != null)
            {
                Debug.Log("edit");
                float strength = subtractMode ? -editStrength : editStrength;
                generator.Edit(hitPoint, strength, editRadius);
                chunk.RegenerateMesh();
            }
        }
    }

    //void OnDrawGizmos()
    //{
    //    if (hit)
    //    {
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawSphere(hitPoint, 0.25f);
    //    }
    //}

}
