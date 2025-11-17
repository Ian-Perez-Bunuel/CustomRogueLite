using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;

public class PlayerStateManager : MonoBehaviour
{
    private PlayerBaseState currentState;
    public PlayerIdleState idleState = new PlayerIdleState();
    public PlayerMovingState movingState = new PlayerMovingState();
    public PlayerAerialMovingState aerialMovingState = new PlayerAerialMovingState();

    public Rigidbody rb;
    public Transform orientation;

    [Header("Movement")]
    public InputActionReference move;
    public float groundedSpeed = 7f;
    public float groundedDrag = 10.0f;

    [Header("Aerial Movement")]
    public float aerialSpeed = 7f;
    public float aerialDrag = 6.0f;
    Gravity gravity;
    [HideInInspector] public bool useGravity;
    public float gravityForceRising = 5f;
    public float gravityForceFalling = 10f;

    [Header("Jump")]
    public InputActionReference jump;
    public bool canJump;
    public float jumpPower = 15f;

    [Header("Ground Check")]
    public float height;
    public float sphereCastRadius;
    [HideInInspector] public RaycastHit raycastHit;
    public LayerMask groundLayerMask;
    public bool grounded;

    [Header("SlopeHandeling")]
    public float maxSlopeAngle;

    // Start is called before the first frame update
    void Start()
    {
        gravity = GetComponent<Gravity>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        currentState = idleState;
        currentState.EnteredState(this);
    }

    // Update is called once per frame
    void Update()
    {
        currentState.UpdateState(this);
    }

    void FixedUpdate()
    {
        // Ground check
        grounded = Physics.SphereCast(transform.position, sphereCastRadius, Vector3.down, out raycastHit, height * 0.5f + 0.2f, groundLayerMask);

        // Treat gentle slopes as ground => disable custom gravity there
        useGravity = !OnSlope();

        if (!grounded && useGravity)
        {
            if (rb.linearVelocity.y > 0)
            {
                gravity.SetGravity(new Vector3(0, -gravityForceRising, 0));
            }
            else if (rb.linearVelocity.y < 0)
            {
                gravity.SetGravity(new Vector3(0, -gravityForceFalling, 0));
            }

            canJump = false;
        }
        else
        {
            gravity.SetGravity(Vector3.zero); // Important so you don't slide on gentle slopes
            canJump = true;
        }

        currentState.FixedUpdateState(this);
    }

    public bool Jump()
    {
        if (canJump && jump.action.IsPressed())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0.0f, rb.linearVelocity.z);
            rb.AddForce(new Vector3(0.0f, jumpPower, 0.0f), ForceMode.Impulse);
            canJump = false;

            return true;
        }
        return false;
    }

    public void SwitchState(PlayerBaseState t_newState)
    {
        currentState.ExitState(this);
        currentState = t_newState;
        currentState.EnteredState(this);
    }

    public bool OnSlope()
    {
        // Slightly longer ray than the player to be safe
        if (raycastHit.collider != null)
        {
            float angle = Vector3.Angle(Vector3.up, raycastHit.normal);
            return angle > 0f && angle <= maxSlopeAngle;
        }
        return false;
    }
}
