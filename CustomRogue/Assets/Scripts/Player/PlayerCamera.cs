using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    float xRotation = 0f;
    float yRotation = 0f;

    public Transform orientation;
    public Transform cameraTransform;
    public InputActionReference look;

    bool isFirstPerson = true;

    [Header("Cameras")]
    [SerializeField] CinemachineCamera firstPersonCamera;
    [SerializeField] CinemachineCamera thirdPersonCamera;
    [SerializeField] float verticalDefault;
    CinemachineOrbitalFollow thirdPersonOrbitalFollow;

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

        thirdPersonOrbitalFollow = thirdPersonCamera.GetComponent<CinemachineOrbitalFollow>();
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
        if (isFirstPerson)
        {
            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
            cameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
        else
        {
            Vector3 newForward = thirdPersonCamera.transform.forward;
            orientation.forward = new Vector3(newForward.x, 0.0f, newForward.z);
        }
    }

    public void SwapPerspective()
    {
        isFirstPerson = !isFirstPerson;

        if (isFirstPerson)
        {
            // Match first person rotation to the current third-person camera view
            Vector3 forward = thirdPersonCamera.transform.forward;

            yRotation = YawFromForward(forward);
            xRotation = PitchFromForward(forward);
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
            cameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

            firstPersonCamera.Priority = 1;
            thirdPersonCamera.Priority = 0;
        }
        else
        {
            // Match third person orbit to current first-person yaw
            thirdPersonOrbitalFollow.HorizontalAxis.Value = yRotation;
            // Auto look from above
            thirdPersonOrbitalFollow.VerticalAxis.Value = verticalDefault;

            firstPersonCamera.Priority = 0;
            thirdPersonCamera.Priority = 1;
        }
    }

    float YawFromForward(Vector3 forward)
    {
        forward.y = 0f;
        forward.Normalize();
        return Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
    }

    float PitchFromForward(Vector3 forward)
    {
        return -Mathf.Asin(forward.normalized.y) * Mathf.Rad2Deg;
    }
}
