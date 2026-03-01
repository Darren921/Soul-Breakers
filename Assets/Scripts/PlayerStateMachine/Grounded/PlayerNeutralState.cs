using System.Collections;
using UnityEngine;
using static PlayerStateManager;

[System.Serializable]
public class PlayerNeutralState : PlayerBaseState
{
   
    private Coroutine _idleCoroutine;

    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player )
    {
        if(player is  null) return;
        if(player.Animations.IsRecoveryFrame) player.Animations.ResetRecoveryFrame();
        
        _idleCoroutine = player?.StartCoroutine(CheckIfIdle(player));
//        Debug.Log("Entered PlayerNeutralState");
    }

    internal override void UpdateState(PlayerStateManager playerStateManager,PlayerController player)
    {
        playerStateManager.CheckForTransition(PlayerStateTypes.Attack | PlayerStateTypes.Jumping | PlayerStateTypes.Crouching | PlayerStateTypes.Walking | PlayerStateTypes.Running | PlayerStateTypes.Dash);
        if(player.SuperJumpActive) playerStateManager.SwitchState (PlayerStateTypes.Jumping);
    }
     
    private IEnumerator CheckIfIdle(PlayerController player)
    {
        //Idle state starts animations (TBA)
        yield return new WaitForSeconds(3f);
    //    Debug.Log("Idle");
        player.Animations.Animator.SetBool(player.Animations.Idle,true);
    } 

    internal override void FixedUpdateState(PlayerStateManager playerStateManager,PlayerController player)
    {
        if (!player.GravityManager.IsGrounded && !player.HitStun)
        {
            Debug.Log("gravity on neutral state");
            player.GravityManager.ApplyGravityToPlayer(player);

        }
      
    }

    internal override void ExitState(PlayerStateManager playerStateManager,PlayerController player)
    {
        if (_idleCoroutine != null)
        {
            player.StopCoroutine(_idleCoroutine);
            _idleCoroutine = null;
            player.Animations.Animator.SetBool(player.Animations.Idle,false);

        }        




//        Debug.Log("Exit PlayerNeutralState");
    }
}
