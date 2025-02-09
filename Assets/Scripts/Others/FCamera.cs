using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSensitivity = 2f;
    public float boostMultiplier = 2f; // Hold Shift to move faster

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Mouse look
        rotationX -= Input.GetAxis("Mouse Y") * lookSensitivity;
        rotationY += Input.GetAxis("Mouse X") * lookSensitivity;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Prevent flipping

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);

        // Movement
        float moveMultiplier = Input.GetKey(KeyCode.LeftShift) ? boostMultiplier : 1f;
        Vector3 move = new Vector3(
            Input.GetAxis("Horizontal"),  // A/D
            (Input.GetKey(KeyCode.R) ? 1 : 0) - (Input.GetKey(KeyCode.F) ? 1 : 0), // R/F
            Input.GetAxis("Vertical")     // W/S
        );

        transform.position += transform.TransformDirection(move) * moveSpeed * moveMultiplier * Time.deltaTime;

        // Unlock cursor when pressing Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetMouseButtonDown(0)) // Click to lock cursor again
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
