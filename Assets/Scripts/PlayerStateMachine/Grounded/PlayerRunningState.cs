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
        //controls the decel curve to make slow down movement more accurate 
        if (player.PlayerMove == Vector3.zero && !player.DashMarcoActive || player.IsCrouching)
        {
            if (!player.Decelerating && !player.DecelActive)
            {
                player.DecelActive = true;
                player.Decelerating = true;
                player.StartCoroutine(player.DecelerationCurve(player));
            }
        }

        playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Attack | PlayerStateManager.PlayerStateTypes.Jumping);

    var backWardDir = player.Reversed ? 1 : -1;
    
    
        Debug.Log( player.DecelActive);
        Debug.Log(!player.Decelerating);
        Debug.Log((int)player.PlayerMove.x == backWardDir);
        Debug.Log(player.InputReader.CurrentMoveInput == InputReader.MovementInputResult.Backward);
        if (player.InputReader.CurrentMoveInput == InputReader.MovementInputResult.Backward &&  (int)player.PlayerMove.x == backWardDir )
        {
            Debug.Log("entered");
            playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Walking);
        }
       
        
        //switch states 
        if(player.Decelerating || !player.DecelActive ) return;
        
      
        playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.Jumping| PlayerStateManager.PlayerStateTypes.Crouching | PlayerStateManager.PlayerStateTypes.Walking);
        

    }

    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
//        Debug.Log( !player.DashMarcoActive ? new Vector2(player.PlayerMove.x, 0) :  !player.Reversed ? new Vector2(1, 0) : new Vector2(-1, 0));
     
        SetMoveDir( !player.Reversed ? new Vector2(1, 0) : new Vector2(-1, 0));
        SmoothMovement();
        ApplyVelocity(player);
    }

    protected override void ApplyVelocity(PlayerController player)
    {
        var velocity =  new Vector3(SmoothedMoveDir.x * MoveSpeed, player.rb.linearVelocity.y) ;
        player.rb.linearVelocity = velocity;    
    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
      player.IsRunning = false;          
      Debug.Log(player.rb.linearVelocity);
      player.DecelActive = false;
      player.SetFrictionBox(false);
    }
}
