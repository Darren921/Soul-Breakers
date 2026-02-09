using UnityEngine;
[System.Serializable]
public class PlayerCrouchMoveState : PlayerMovingState
{
    protected override float MoveSpeed => Player.WalkSpeed;
    protected override void ApplyVelocity(PlayerController player)
    {
        var velocity = new Vector3(SmoothedMoveDir.x * MoveSpeed, player.rb.linearVelocity.y);
//        player.rb.linearVelocity = velocity;    
    }


    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        switch (player.IsCrouching)
        {
            // state swap
            case true when player.PlayerMove.x == 0:
                playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Crouching);
                break;
            case false when player.IsWalking:
                playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Walking);
                break;
            case false when player.PlayerMove.x == 0:
                playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Neutral);
                break;
            case true when (player.PlayerMove.x != 0 && player.IsAttacking):
                playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Attack);
                break;
        }
    }
}
