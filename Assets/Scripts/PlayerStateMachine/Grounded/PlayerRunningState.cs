using UnityEngine;

[System.Serializable]

public class PlayerRunningState : PlayerMovingState
{
    protected override float MoveSpeed => Player.RunSpeed;
    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        base.EnterState(playerStateManager, player);
        player.IsRunning = true;
        
    }

    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (player.PlayerMove == Vector3.zero || player.PlayerMove == Vector3.left || player.InputReader.CurrentMoveInput == InputReader.MovementInputResult.Backward || player.IsAttacking || player.PlayerMove.x > 0)
        {
            // if (!player.Decelerating)
            // {
                          playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.Jumping| PlayerStateManager.PlayerStateTypes.Crouching | PlayerStateManager.PlayerStateTypes.Walking| PlayerStateManager.PlayerStateTypes.Attack | PlayerStateManager.PlayerStateTypes.Jumping);

            //     player.StartCoroutine(player.OnDeceleration(player));
            // }
          
        }  
        

    }

    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
//        Debug.Log( !player.DashMarcoActive ? new Vector2(player.PlayerMove.x, 0) :  !player.Reversed ? new Vector2(1, 0) : new Vector2(-1, 0));
        if (player.PlayerMove == Vector3.zero || player.PlayerMove == Vector3.left ||
            player.InputReader.CurrentMoveInput == InputReader.MovementInputResult.Backward || player.IsAttacking)
        {
            if (!player.Decelerating)
            {

                player.StartCoroutine(player.OnDeceleration(player));
            }
        }

        SetMoveDir( !player.Reversed ? new Vector2(1, 0) : new Vector2(-1, 0));
        SmoothMovement();
        ApplyVelocity(player);
    }

    protected override void ApplyVelocity(PlayerController player)
    {
        if(player.PlayersColliding || player.AtBorder) return;
        player.rb.MovePosition(player.transform.position + SmoothedMoveDir * (MoveSpeed * Time.fixedDeltaTime ));
        /*var velocity =  new Vector3(SmoothedMoveDir.x * MoveSpeed, player.rb.linearVelocity.y) ;
        player.rb.linearVelocity = velocity;  */  
    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
      player.IsRunning = false;
       Debug.Log(player.rb.linearVelocity);

    }
    
}
