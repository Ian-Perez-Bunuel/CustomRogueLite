using System;
using UnityEngine;
using UnityEngine.Splines.Interpolators;

[Serializable]
public class PlayerBurrow : PlayerState
{
    [Header("Movement")]
    public float speed;

    [Header("Jump")]
    [SerializeField] float maxJumpHeight;
    float jumpHeight;
    [SerializeField] float jumpChargeSpeed;
    bool isChargingJump = false;

    public override void OnEnter(PlayerController player)
    {
        player.visuals.SetToBurrow();
        player.playerCamera.SetThirdPerson();
    }
    public override void OnExit(PlayerController player)
    {
        player.visuals.SetToDefault();
        player.playerCamera.SetFirstPerson();
    }

    public override void Update(PlayerController player)
    {
        // Exit burrow
        if (player.burrow.action.WasPressedThisFrame())
        {
            player.ChangeState(player.defaultState);
        }
        // Started charge
        if (player.input.actions["Jump"].WasPressedThisFrame())
        {
            isChargingJump = true;
        }

        // Jump out of burrow charge
        if (player.input.actions["Jump"].IsInProgress())
        {
            jumpHeight += jumpChargeSpeed;

            // Check if it's within bounds
            if (jumpHeight >= maxJumpHeight)
            {
                Jump(player);
            }
        }
        else if (isChargingJump)
        {
            if (player.input.actions["Jump"].WasReleasedThisFrame())
                Jump(player);
        }

        // Gravity
        if (player.isGrounded && player.velocity.y < 0)
        {
            player.velocity.y = -1f;
        }
        else if (!player.isGrounded)
        {
            player.velocity = player.gravity.Apply(player.velocity);
        }

        player.controller.Move(player.velocity * Time.deltaTime);

        // Movement
        Movement(player);
    }

    void Movement(PlayerController player)
    {
        Vector2 moveInput = player.input.actions["Move"].ReadValue<Vector2>();

        Vector3 moveDirection = player.orientation.right * moveInput.x + player.orientation.forward * moveInput.y;
        Vector3 movement = moveDirection * Time.deltaTime * speed;

        player.visuals.RotateBurrow(movement);
        player.controller.Move(movement);
    }

    void Jump(PlayerController player)
    {
        isChargingJump = false;
        player.velocity.y = Mathf.Sqrt(jumpHeight * -2 * player.gravity.GetGravity());

        jumpHeight = 0.0f;

        player.ChangeState(player.defaultState);
    }
}
