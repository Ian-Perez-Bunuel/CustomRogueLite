using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class PlayerDefault : PlayerState
{
    [Header("Movement")]
    [SerializeField] float speed;

    [Header("Jump")]
    [SerializeField] float jumpHeight;
    bool canJump = false;

    public override void OnEnter(PlayerController player)
    {
    }
    public override void OnExit(PlayerController player)
    {
    }

    public override void Update(PlayerController player)
    {
        // Jump
        if (player.isGrounded)
            canJump = true;

        if (player.burrow.action.WasPressedThisFrame())
        {
            player.ChangeState(player.burrowState);
        }

        if (player.input.actions["Jump"].IsPressed() && canJump)
        {
            Jump(player);
        }

        // Gravity
        if (player.isGrounded && player.velocity.y < 0)
        {
            player.velocity.y = -5f;
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

        player.controller.Move(movement);
    }

    void Jump(PlayerController player)
    {
        player.velocity.y = Mathf.Sqrt(jumpHeight * -2 * player.gravity.GetGravity());
        canJump = false;
    }
}
