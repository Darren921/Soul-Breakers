using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAttackState : PlayerBaseState
{
    private Coroutine cooldownCoroutine;
    private InputReader.MovementInputResult lastMove;

    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (player.Animations.Animator.GetBool(player.Animations.Idle))
        {
            player.Animations.Animator.SetBool(player.Animations.Idle, false);
        }
        player.Animations.Animator.SetBool(player.Animations.Attacking, true);


        player.InputReader.ConsumeCurrentInput();
//        Debug.Log("Entered attack state");


        //    player.InputReader.currentAttackCached = player.InputReader.CurrentAttackInput;
        //        Debug.Log(lastMove); 
//          Debug.Log(player.InputReader.LastAttackInput);
    }


    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {

        player.Animations.Animator.SetBool(player.Animations.airborne, !player.GravityManager.IsGrounded);

//        Debug.Log(player.Rb.linearVelocity); 

        if (CancelCheck(player) && !player.Animations.IsActiveFrame )
        {
            player.canCancel = false;
            player.Animations.attackCancel = true;
            Debug.Log("attack cancel");
            player.Animations.AnimationToPlay = player.CharacterData.characterAttacks.ReturnAttackData(player.InputReader.LastAttackInput, player.InputReader.curState).AnimHash;
            Debug.Log(  player.Animations.AnimationToPlay);
            PerformAttack(player);

            
        }

        if (!player.Animations.attackCancel)
        {
            if (player.IsAttacking && !player.OnAttackCoolDown && !player.Animations.attackCancel)
            {
                if(  player.Animations.AnimationToPlay == -1)  player.Animations.AnimationToPlay = player.CharacterData.characterAttacks.ReturnAttackData(player.InputReader.LastAttackInput, player.InputReader.curState).AnimHash;
                Debug.Log("attacking");
                PerformAttack(player);
            }

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
        Debug.Log($"Cancel check {player.canCancel} and input priority = {player.InputReader.LastAttackInput.Priority} vs last hit {player.InputReader.currentAttackCached.Input.Priority}" + $"and not cancelled prior {!player.Animations.attackCancel} and cur input != none {player.InputReader.currentAttackCached.Input.Priority != -1}  ");
        if (player.canCancel && player.InputReader.CurrentAttackInput.Input.Priority > player.InputReader.currentAttackCached.Input.Priority  &&  player.InputReader.currentAttackCached.Input.Priority != -1 &&  player.InputReader.CurrentAttackInput.Input.Priority != -1)
        {
            return true;
        }
        return false;
    }


    private void PerformAttack(PlayerController player)
    {
        if (player.Animations.attackCancel)
        {
            player.Animations.Animator.Play(  player.Animations.AnimationToPlay, 0, 0f);
            cooldownCoroutine = player.StartCoroutine(EnforceCooldown(player));
        }
        if (!player.IsAttacking || player.OnAttackCoolDown || player.Animations.attackCancel) return;
        Debug.Log("attacking");
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
        Debug.Log("Exit attacking");
        player.Animations.attackCancel = false;
        player.GravityManager.ResetVelocity();
        player.Animations.ResetAttackingTrigger();
        player.Animations.AnimationToPlay = -1;
    }
}
