using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAttackState : PlayerBaseState
{
    private Coroutine cooldownCoroutine;
    private InputReader.MovementInputResult lastMove;
    private bool attackCancel;
    private int AnimationToPlay = -1;

    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {

//        Debug.Log("Entered attack state");
        if (player.Animations.Animator.GetBool(player.Animations.Idle))
        {
            player.Animations.Animator.SetBool(player.Animations.Idle, false);
        }
        player.InputReader.currentAttackCached = player.InputReader.CurrentAttackInput;
        player.InputReader.ConsumeCurrentInput();
        //        Debug.Log(lastMove); 
//          Debug.Log(player.InputReader.LastAttackInput);
    }


    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {

        player.Animations.Animator.SetBool(player.Animations.airborne, !player.GravityManager.IsGrounded);

//        Debug.Log(player.Rb.linearVelocity); 

        if (CancelCheck(player) )
        {
            player.canCancel = false;
            attackCancel = true;
            Debug.Log("attack cancel");
            AnimationToPlay = player.CharacterData.characterAttacks.ReturnAttackData(player.InputReader.LastAttackInput, player.InputReader.curState).AnimHash;
            player.Animations.Animator.StopPlayback();
            player.Animations.ResetAttackingTrigger();
            PerformAttack(player);

        }
        if (player.IsAttacking && !player.OnAttackCoolDown && !attackCancel)
        {
            if(AnimationToPlay == -1)AnimationToPlay = player.CharacterData.characterAttacks.ReturnAttackData(player.InputReader.LastAttackInput, player.InputReader.curState).AnimHash;
                Debug.Log("attacking");
                PerformAttack(player);
        }
        
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
        if (!player.GravityManager.IsGrounded || player.IsAttacking) return;
        playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral |
                                              PlayerStateManager.PlayerStateTypes.Walking |
                                              PlayerStateManager.PlayerStateTypes.Crouching |
                                              PlayerStateManager.PlayerStateTypes.Jumping |
                                              PlayerStateManager.PlayerStateTypes.Running);
//        Debug.Log(player.gravityManager.GetVelocity());
    }

    private bool CancelCheck(PlayerController player)
    {
        if (player.canCancel && player.InputReader.CurrentAttackInput.Input.Priority > player.InputReader.currentAttackCached.Input.Priority && !attackCancel &&  player.InputReader.currentAttackCached.Input.Priority != -1)
        {
            return true;
        }
        return false;
    }


    private void PerformAttack(PlayerController player)
    {

        if (!player.IsAttacking || player.OnAttackCoolDown || attackCancel) return;
        player.Animations.Animator.Play(AnimationToPlay, 0, 0f);
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
        attackCancel = false;
        player.GravityManager.ResetVelocity();
        player.canCancel = false;
        player.Animations.ResetAttackingTrigger();
        AnimationToPlay = -1;
    }
}
