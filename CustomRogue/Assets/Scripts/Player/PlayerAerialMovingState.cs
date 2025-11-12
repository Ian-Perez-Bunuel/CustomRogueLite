using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAerialMovingState : PlayerBaseState
{
    Vector2 moveDirection;

    public override void EnteredState(PlayerStateManager player)
    {
        player.rb.linearDamping = player.aerialDrag;
    }

    public override void UpdateState(PlayerStateManager player)
    {
        moveDirection = player.move.action.ReadValue<Vector2>();
    }

    public override void FixedUpdateState(PlayerStateManager player)
    {
        if (player.grounded)
        {
            // Wait before swapping back to grounded movement
            player.StartCoroutine(WaitBeforeGrounded(player));
        }

        Vector3 movement = player.orientation.forward * moveDirection.y + player.orientation.right * moveDirection.x;
        movement = movement.normalized * player.aerialSpeed * 10f;

        player.rb.AddForce(movement, ForceMode.Force);

        if (player.Jump())
        {
            // Stop transition
            player.StopCoroutine(WaitBeforeGrounded(player));
        }
    }

    public override void OnCollision(PlayerStateManager player)
    {

    }

    public override void ExitState(PlayerStateManager player)
    {
        
    }

    IEnumerator WaitBeforeGrounded(PlayerStateManager player)
    {
        yield return new WaitForSeconds(0.1f);

        if (player.grounded)
        {
            player.SwitchState(player.movingState);
        }
    }
}
