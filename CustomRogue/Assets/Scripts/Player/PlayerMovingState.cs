using UnityEngine;
using UnityEngine.Assertions.Must;

public class PlayerMovingState : PlayerBaseState
{
    Vector2 moveDirection;

    public override void EnteredState(PlayerStateManager player)
    {
        player.rb.linearDamping = player.groundedDrag;
    }

    public override void UpdateState(PlayerStateManager player)
    {
        // Jumping
        player.Jump(); // Does checks inside

        // Swap state if not grounded
        if (!player.grounded)
        {
            player.SwitchState(player.aerialMovingState); // Swap to aerial since just jumped
        }

        moveDirection = player.move.action.ReadValue<Vector2>();

        if (!player.grounded)
        {
            player.SwitchState(player.aerialMovingState);
        }
        else if (player.rb.linearVelocity.magnitude <= 0f)
        {
            player.SwitchState(player.idleState);
        }
    }

    public override void FixedUpdateState(PlayerStateManager player)
    {
        // read world-space input dir once
        Vector3 inputWorld =
            player.orientation.forward * moveDirection.y +
            player.orientation.right * moveDirection.x;

        Vector3 movement;

        if (player.OnSlope())
        {
            // Project input onto slope so you move along the surface
            Vector3 slopeMove = Vector3.ProjectOnPlane(inputWorld, player.raycastHit.normal).normalized;

            movement = slopeMove * player.groundedSpeed * 20f;

            // Keep the body pinned to the ground
            if (player.rb.linearVelocity.y > 0f)
            {
                player.rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else
        {
            movement = inputWorld.normalized * player.groundedSpeed * 20f;
        }

        player.rb.AddForce(movement, ForceMode.Force);
    }


    public override void OnCollision(PlayerStateManager player)
    {

    }

    public override void ExitState(PlayerStateManager player)
    {

    }

    private Vector3 GetSlopeMoveDirection(PlayerStateManager player)
    {
        return Vector3.ProjectOnPlane(moveDirection, player.raycastHit.normal);
    }
}
