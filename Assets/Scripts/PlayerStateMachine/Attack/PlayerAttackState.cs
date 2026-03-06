using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAttackState : PlayerBaseState
{
    private Coroutine cooldownCoroutine;
    private Coroutine cancelCoroutine;
    private InputReader.MovementInputResult lastMove;
    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (player.Animations.Animator.GetBool(player.Animations.Idle))
        {
            player.Animations.Animator.SetBool(player.Animations.Idle, false);
        }
        player.Animations.Animator.SetBool(player.Animations.Attacking, true);
       
        player.InputReader.ConsumeCurrentInput();
        if (player.Animations.CancelActive )
        {
            playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Grab);
        }
//        Debug.Log("Entered attack state");


        //    player.InputReader.currentAttackCached = player.InputReader.CurrentAttackInput;
        //        Debug.Log(lastMove); 
//          Debug.Log(player.InputReader.LastAttackInput);
    }


    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (player.Animations.CancelActive )
        {
            playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Grab);
        }
        else if (player.IsAttacking && !player.OnAttackCoolDown)
        {
            if(  player.Animations.AnimationToPlay == -1)  player.Animations.AnimationToPlay = player.CharacterData.characterAttacks.ReturnAttackData(player.InputReader.LastAttackInput, player.InputReader.curState).AnimHash;
            Debug.Log("attacking" + player.name);
            PerformAttack(player);
        }
      
        player.Animations.Animator.SetBool(player.Animations.airborne, !player.GravityManager.IsGrounded);

//        Debug.Log(player.Rb.linearVelocity); 

//       Debug.Log(CancelCheck(player)); ;
      



        
     
        if (player.HitStun) playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.HitStun);
        if (player.IsAttacking && player.GravityManager.IsGrounded &&
            player.InputReader.curState == AttackData.States.Airborne)
        {
            Debug.Log("Switch to Grounded");
            player.Animations.Animator.Play("Neutral", 0, 0f);
            playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Neutral);
            player.HitDetection.otherPlayer.HitDetection.resetHit();
        }

        
        // State swapping 
        if (!player.GravityManager.IsGrounded || player.IsAttacking ) return;
        if (player.Animations.CancelActive ) playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.CancelAttack);
        
        playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.Walking | PlayerStateManager.PlayerStateTypes.Crouching | PlayerStateManager.PlayerStateTypes.Jumping | PlayerStateManager.PlayerStateTypes.Running);
//        Debug.Log(player.gravityManager.GetVelocity());
    }

   

    private void PerformAttack(PlayerController player)
    {
        if (!player.IsAttacking || player.OnAttackCoolDown ) return;
        Debug.Log("normal attack" + player.name);
        player.Animations.Animator.Play(  player.Animations.AnimationToPlay, 0, 0f);
        cooldownCoroutine = player.StartCoroutine(EnforceCooldown(player));
    }

    

    private IEnumerator EnforceCooldown(PlayerController player)
    {
        player.OnAttackCoolDown = true;
        yield return new WaitUntil(() => !player.IsAttacking);
        player.OnAttackCoolDown = false;
    }

  





    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        //applying the custom gravity when player is airborne 
        player.GravityManager.ApplyGravityToPlayer(player);
        
    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
        Debug.Log("Exit attacking" + player.name);
        player.GravityManager.ResetVelocity();
        player.Animations.ResetAttackingTrigger();
        player.Animations.AnimationToPlay = -1;
    }
}
