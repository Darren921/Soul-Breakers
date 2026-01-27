using UnityEngine;

[System.Serializable]

public class PlayerWalkingState : PlayerMovingState
{
    protected override float MoveSpeed => Player.WalkSpeed;
    private float temp;
    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        base.EnterState(playerStateManager, player);
        if(player.Decelerating) return;
        player.IsWalking = true;
    }

    protected override void ApplyVelocity(PlayerController player)
    {
        var velocity = new Vector3(player.PlayerMove.x * MoveSpeed, player.rb.linearVelocity.y);
        player.rb.linearVelocity = velocity;
        if (!player.Animations.Animator.GetCurrentAnimatorStateInfo(0).IsName("Walking")) return;
        switch (player.rb.linearVelocity.x)
        {
            case < 0 when player.Reversed:
                temp = 1;
                break;
            case > 0 when player.Reversed:
                temp = 0;
                break;
            default:
                temp = player.rb.linearVelocity.x switch
                {
                    > 0 => 1,
                    < 0 => 0,
                    _ => player.rb.linearVelocity.x
                };
                break;
        }

        player.Animations.Animator.SetFloat(player.Animations.WalkDir, temp);


    }

    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        //switch states 
        playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Attack | PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.CrouchMove 
                                              | PlayerStateManager.PlayerStateTypes.Running | PlayerStateManager.PlayerStateTypes.Jumping | PlayerStateManager.PlayerStateTypes.Crouching | PlayerStateManager.PlayerStateTypes.Dash);
        
    }
}
