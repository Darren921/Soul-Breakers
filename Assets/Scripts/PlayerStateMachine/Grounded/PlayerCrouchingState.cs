using UnityEngine;

[System.Serializable]
public class PlayerCrouchingState : PlayerBaseState
{
    internal override void EnterState(PlayerStateManager playerStateManager,PlayerController player)
    {
        Debug.Log("Entering PlayerCrouchingState");
   //  player.rb.linearVelocity = Vector3.zero;
      if(player.Animations.GhostAnimator)      player.Animations.GhostAnimator?.Play(player.Animations.GhostCrouch, 0 , 0.1f);
    }

    internal override void UpdateState(PlayerStateManager playerStateManager,PlayerController player)
    {
    
        // swap states 
        playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.Attack | PlayerStateManager.PlayerStateTypes.Jumping );
        if (!player.IsCrouching && player.IsWalking) playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Walking);
        
    }

    internal override void FixedUpdateState(PlayerStateManager playerStateManager,PlayerController playerController)
    {
    }

    internal override void ExitState(PlayerStateManager playerStateManager,PlayerController player)
    {
  //      Debug.Log("Exiting PlayerCrouchingState");
    }
}
