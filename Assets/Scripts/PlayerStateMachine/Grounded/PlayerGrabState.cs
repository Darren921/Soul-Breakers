using UnityEngine;

public class PlayerGrabState : PlayerBaseState
{
    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        player.OnDisablePlayer();
        // Alex disable this when the animation event to disable grab/grabbing is added
    }

    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (!player.Animations.Animator.GetBool(player.Animations.Grab) || !player.Animations.Animator.GetBool(player.Animations.Grabbed))
        {
          //  playerStateManager.SwitchState(p);
        }
    }

    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
      
    }
}
