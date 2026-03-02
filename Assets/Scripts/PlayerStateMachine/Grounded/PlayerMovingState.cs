using UnityEngine;

[System.Serializable]
public abstract class PlayerMovingState : PlayerBaseState
{
    protected PlayerController Player; 
    [field: SerializeField] protected Vector3 MoveDir;
    [field: SerializeField] protected Vector3 SmoothedMoveDir;
    [field: SerializeField] protected Vector3 SmoothedMoveVelocity;

    protected virtual float MoveSpeed => 1;
    
    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
//        Debug.Log("Entered " + playerStateManager.currentState);
        
        Player = player;
  //      player.rb.linearVelocity = Vector3.zero;
    }

    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (player.IsBeingAttacked && player.InputReader.CurrentMoveInput == InputReader.MovementInputResult.Backward)
        {
            playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Blocking);
            return;
        }
        // if (!player.GravityManager.IsGrounded && !player.HitStun)
        // {
        //     player.GravityManager.ApplyGravity(player);
        //     
        //     player.rb.MovePosition(player.transform.position + new Vector3(player.PlayerMove.x, player.GravityManager.GetVelocity() , 0f) * Time.fixedDeltaTime );
        //     //  player.rb.linearVelocity  = new Vector3(player.rb.linearVelocity.x,player.GravityManager.GetVelocity() ,0);
        // }
        if(player.Decelerating || player.PlayersColliding) return;
//        Debug.Log("Player is moving");
        SetMoveDir(new Vector2(player.PlayerMove.x, 0));
        SmoothMovement();
        ApplyVelocity(player);
    }

    protected abstract void ApplyVelocity(PlayerController player);

    protected void SmoothMovement()
    {
        SmoothedMoveDir = Vector3.SmoothDamp(SmoothedMoveDir, MoveDir, ref SmoothedMoveVelocity, 0.2f);
    }

    protected void SetMoveDir(Vector3 newDir)
    {
        MoveDir = newDir.normalized;    
    }
 

   
    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
      //   player.rb.linearVelocity = Vector3.zero;
        SmoothedMoveVelocity = Vector3.zero;
        SmoothedMoveDir = Vector3.zero;

    }
}
