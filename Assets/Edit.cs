using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class Edit : MonoBehaviour
{
    public float editRadius = 10;
    public float editStrength = 20;
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


        if (Input.GetMouseButton(0)) // Left-click 
        {
            Debug.Log("pressed");
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            //Debug.DrawRay(ray.origin, ray.direction * 200f, Color.red);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, terrainLayer))
            {
                Debug.Log("hit");

                TryEditChunk(hit.point);
            }
        }

        
        if (Input.GetKeyDown(KeyCode.E))
            subtractMode = !subtractMode;
    }

    void TryEditChunk(Vector3 hitPoint)
    {
        float chunkSize = 64f; 
        Vector3 min = hitPoint - Vector3.one * editRadius;
        Vector3 max = hitPoint + Vector3.one * editRadius;

        Vector3Int minChunkCoord = new Vector3Int(
            Mathf.FloorToInt(min.x / chunkSize),
            Mathf.FloorToInt(min.y / chunkSize),
            Mathf.FloorToInt(min.z / chunkSize)
        );

        Vector3Int maxChunkCoord = new Vector3Int(
            Mathf.FloorToInt(max.x / chunkSize),
            Mathf.FloorToInt(max.y / chunkSize),
            Mathf.FloorToInt(max.z / chunkSize)
        );

        for (int x = minChunkCoord.x; x <= maxChunkCoord.x; x++)
        {
            for (int y = minChunkCoord.y; y <= maxChunkCoord.y; y++)
            {
                for (int z = minChunkCoord.z; z <= maxChunkCoord.z; z++)
                {
                    Vector3 chunkCoord = new Vector3(x, y, z);

                    if (world.chunkDir.TryGetValue(chunkCoord, out var chunk))
                    {

                            float strength = subtractMode ? -editStrength : editStrength;
                            chunk.meshGenerator.Edit(hitPoint, strength, editRadius);
                            chunk.RegenerateMesh();
                        
                    }
                }
            }
        }

    }


}
