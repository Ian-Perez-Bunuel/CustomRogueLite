using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public override void EnteredState(PlayerStateManager player)
    {
        Debug.Log("Idle Entered");
    }

    public override void UpdateState(PlayerStateManager player)
    {
        if (player.move.action.ReadValue<Vector2>().magnitude > 0) 
        {
            player.SwitchState(player.movingState);
        }
    }

    public override void FixedUpdateState(PlayerStateManager player)
    {
        // Jumping
        if (player.jump.action.IsPressed())
        {
            player.rb.AddForce(new Vector3(0.0f, player.jumpPower, 0.0f), ForceMode.Impulse);
            player.SwitchState(player.aerialMovingState); // Swap to aerial since just jumped
        }
    }

    public override void OnCollision(PlayerStateManager player)
    {

    }

    public override void ExitState(PlayerStateManager player)
    {
        Debug.Log("Idle Exited");
    }
}
