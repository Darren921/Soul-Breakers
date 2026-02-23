using UnityEngine;
using System.Collections;

[System.Serializable]

//Transition checks are in playerstateManager use if they match your needs, example below, whatever transtion you need place in check for transition, else bool check in update 
// playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.Walking | PlayerStateManager.PlayerStateTypes.Crouching);
public class PlayerBlockingState : PlayerBaseState
{
    private PlayerStateManager.PlayerStateTypes _returnState; 
    private Coroutine _blockCoroutine;

    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        Debug.Log("Blocked state triggered");

        player.Animations.Animator.SetBool(player.Animations.blocking, true);
        
        
        _blockCoroutine = player.StartCoroutine(BlockDuration(playerStateManager, player));
    }

    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (player.HitDetection.Blocking == false)
        {
            playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Neutral);
        }
    }

    private IEnumerator BlockDuration(PlayerStateManager playerStateManager, PlayerController player)
    {
        yield return new WaitForSeconds(0.2f);
        player.HitDetection.Blocking = false;
        playerStateManager.SwitchToLastState();
    }

    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (!player.GravityManager.IsGrounded && !player.HitStun)
        {
            player.GravityManager.ApplyGravity(player);
            
            player.rb.MovePosition(player.transform.position + new Vector3(player.PlayerMove.x, player.GravityManager.GetVelocity() , 0f) * Time.fixedDeltaTime );
            //  player.rb.linearVelocity  = new Vector3(player.rb.linearVelocity.x,player.GravityManager.GetVelocity() ,0);
        }
    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (_blockCoroutine != null) player.StopCoroutine(_blockCoroutine);
        player.Animations.Animator.SetBool(player.Animations.blocking, false);
        player.Animations.Animator.SetBool(player.Animations.Hit, false);
        player.HitDetection.resetHit();

    }
}
