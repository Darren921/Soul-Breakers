using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerCancelAttackState : PlayerBaseState
{
    private int TempHash;
    private string animationName; 
    
    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (player.Animations.Animator.GetBool(player.Animations.Idle))
        {
            player.Animations.Animator.SetBool(player.Animations.Idle, false);
        }
        player.Animations.CancelActive = false;
        player.canCancel = false;
        Debug.Log(" cancel detected " + player.name);
        animationName = player.CharacterData.characterAttacks.ReturnAttackData(player.InputReader.LastAttackInput, player.InputReader.curState).AnimationName;
        player.Animations.AnimationToPlay = player.CharacterData.characterAttacks.ReturnAttackData(player.InputReader.LastAttackInput, player.InputReader.curState).AnimHash;
        Debug.Log(player.Animations.AnimationToPlay);
        
        if (player.Animations.GhostAnimations.ContainsKey(animationName) && player.Animations.GhostAnimator ) player.Animations.GhostAnimator?.Play(player.Animations.GhostAnimations[animationName], 0, 0f);
        player.Animations.Animator.Play(  player.Animations.AnimationToPlay, 0, 0.25f);
       player. CancelPlaying  = true;
       player.Animations.Animator.SetBool(player.Animations.Attacking, true);

        Debug.Log("cancel attack" + player.name);
    }

    
    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        player.Animations.Animator.SetBool(player.Animations.CancelDectect, player.CancelPlaying);
      //  Debug.Log($"{player.IsAttacking} , {player.GravityManager.IsGrounded} , {player.InputReader.curState }");
        if (player.canCancel && player.GravityManager.IsGrounded && player.InputReader.curState == AttackData.States.Airborne)
        {
            Debug.Log("Switch to Grounded");
            player.Animations.Animator.Play("Neutral", 0, 0f);
            playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Neutral);
            player.HitDetection.otherPlayer.HitDetection.resetHit();
        }
        if(player.CancelPlaying) return;
        playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.Walking | PlayerStateManager.PlayerStateTypes.Crouching | PlayerStateManager.PlayerStateTypes.Jumping | PlayerStateManager.PlayerStateTypes.Running);
    }
    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        //applying the custom gravity when player is airborne 
        player.GravityManager.ApplyGravityToPlayer(player);
        
    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
        Debug.Log("Exit Cancel attacking" +  player.name);
        player.GravityManager.ResetVelocity();
        player.Animations.ResetAttackingTrigger();
        player.Animations.AnimationToPlay = -1;
        player.CancelPlaying = false;
    }
}
