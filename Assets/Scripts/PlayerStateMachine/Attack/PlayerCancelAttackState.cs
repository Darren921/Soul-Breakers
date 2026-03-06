using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerCancelAttackState : PlayerBaseState
{
    private bool CancelPlaying;
    private int TempHash;
    
    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (player.Animations.Animator.GetBool(player.Animations.Idle))
        {
            player.Animations.Animator.SetBool(player.Animations.Idle, false);
        }
        player.Animations.CancelActive = false;
        player.canCancel = false;
        Debug.Log(" cancel detected " + player.name);
        player.Animations.AnimationToPlay = player.CharacterData.characterAttacks.ReturnAttackData(player.InputReader.LastAttackInput, player.InputReader.curState).AnimHash;
        TempHash =  player.Animations.AnimationToPlay;
        Debug.Log(player.Animations.AnimationToPlay);
        player.Animations.Animator.Play(  player.Animations.AnimationToPlay, 0, 0f);
        CancelPlaying   = true;
        player.Animations.Animator.SetBool(player.Animations.Attacking, true);

        player.StartCoroutine(WaitForCancelAnimation(player));
        Debug.Log("cancel attack" + player.name);
    }

    private IEnumerator WaitForCancelAnimation(PlayerController player)
    {
        Debug.Log( $" cur hash = {player.Animations.Animator.GetCurrentAnimatorStateInfo(0).fullPathHash} temp hash = {TempHash}");
        yield return new WaitUntil(() =>
            player.Animations.Animator.GetCurrentAnimatorStateInfo(0).fullPathHash != TempHash);
    }


    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        Debug.Log(player.name);
    
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
        Debug.Log("Exit Cancel attacking" +  player.name);
        player.GravityManager.ResetVelocity();
        player.Animations.ResetAttackingTrigger();
        player.Animations.AnimationToPlay = -1;
    }
}
