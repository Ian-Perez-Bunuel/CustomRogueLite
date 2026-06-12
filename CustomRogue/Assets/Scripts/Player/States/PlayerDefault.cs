using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDefault : PlayerState
{
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
            player.canJump = true;

        if (player.input.actions["Jump"].IsPressed() && player.canJump)
        {
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
        Vector3 movement = moveDirection * Time.deltaTime * player.speed;

        player.controller.Move(movement);
    }

    void Jump(PlayerController player)
    {
        player.velocity.y = Mathf.Sqrt(player.jumpHeight * -2 * player.gravity.GetGravity());
        player.canJump = false;
    }
}
