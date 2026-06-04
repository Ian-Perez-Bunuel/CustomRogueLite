using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class PlayerController : MonoBehaviour
{
    CharacterController controller;
    [SerializeField] Transform orientation;
    PlayerInput input;

    [Header("Movement Stats")]
    public float speed;
    [SerializeField] const float MAX_SPEED = 5;
    Vector3 velocity;

    [Header("Jump")]
    [SerializeField] float jumpHeight = 5;
    bool canJump = false;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance;
    [SerializeField] LayerMask groundMask;
    bool isGrounded;

    [Header("Gravity")]
    [SerializeField] bool useGravity = true;
    [SerializeField] float gravity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Jump
        if (isGrounded)
            canJump = true;

        if (input.actions["Jump"].IsPressed() && canJump)
        {
            Jump();
        }

        // Gravity
        if (Input.GetKeyDown(KeyCode.F))
        {
            useGravity = !useGravity;
        }
        if (useGravity)
        {
            Gravity();
        }

        // Movement
        Movement();
    }

    void Movement()
    {
        Vector2 moveInput = input.actions["Move"].ReadValue<Vector2>();

        Vector3 moveDirection = orientation.right * moveInput.x + orientation.forward * moveInput.y;
        Vector3 movement = moveDirection * Time.deltaTime * speed;

        controller.Move(movement);
    }

    void Gravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;
            return;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        canJump = false;
    }
}
