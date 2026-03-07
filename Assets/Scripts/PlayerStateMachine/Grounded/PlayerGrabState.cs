using System.Collections;
using UnityEngine;

public class PlayerGrabState : PlayerBaseState
{
    private bool CancelPlaying;
    private int TempHash;
    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        
        // player.OnDisablePlayer();
        // Alex disable this when the animation event to disable grab/grabbing is added
     
    }
    
    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        Debug.Log(player.name);
    }
  

    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        //applying the custom gravity when player is airborne 
        player.GravityManager.ApplyGravityToPlayer(player);
    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
     
    }
}
