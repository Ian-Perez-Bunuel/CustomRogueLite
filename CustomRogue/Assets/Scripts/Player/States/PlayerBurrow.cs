using UnityEngine;

public class PlayerBurrow : PlayerState
{
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
        if (player.burrow.action.WasPressedThisFrame())
        {
            player.ChangeState(player.defaultState);
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
}
