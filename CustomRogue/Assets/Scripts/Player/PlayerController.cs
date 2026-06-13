using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    // Components
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Transform orientation;
    [HideInInspector] public PlayerInput input;

    // States
    PlayerState currentState;
    PlayerDefault groundedState;

    [Header("Movement Stats")]
    public float speed;
    [HideInInspector] public Vector3 velocity;

    [Header("Jump")]
    public float jumpHeight = 5;
    [HideInInspector] public bool canJump = false;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRayRadius;
    [SerializeField] LayerMask groundMask;
    [HideInInspector] public bool isGrounded = false;

    [Header("Gravity")]
    [HideInInspector] public PlayerGravity gravity;

    [Header("Player Cameras")]
    [SerializeField] PlayerCamera playerCamera;

    [Header("Perspective Change")]
    public InputActionReference burrow;
    [HideInInspector] public bool isFirstPerson = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // States
        groundedState = new PlayerDefault();
        ChangeState(groundedState);

        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();

        gravity = GetComponent<PlayerGravity>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRayRadius, groundMask);

        // BURROW NEEDS SWAPPED TO STATE
        if (burrow.action.WasPressedThisFrame())
        {
            playerCamera.SwapPerspective();
        }

        currentState.Update(this);
    }

    void ChangeState(PlayerState newState)
    {
        if (newState == currentState)
            return;


        if (currentState != null)
            currentState.OnExit(this);

        currentState = newState;
        currentState.OnEnter(this);
    }
}
