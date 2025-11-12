using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    float xRotation = 0f;
    float yRotation = 0f;

    public Transform orientation;
    public InputActionReference look;

    [Header("Sensitivity")]
    public float sensX = 1.0f;
    public float sensY = 1.0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Prefer typing full numbers than decimals
        sensX /= 100;
        sensY /= 100;
    }

    Vector2 lookDelta;

    void Update()
    {
        lookDelta = look.action.ReadValue<Vector2>();

        float mouseX = lookDelta.x * sensX;
        float mouseY = lookDelta.y * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }

    void LateUpdate()
    {
        // Apply rotations after movement to avoid follow jitter
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        if (orientation != null)
        {
            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }
}
