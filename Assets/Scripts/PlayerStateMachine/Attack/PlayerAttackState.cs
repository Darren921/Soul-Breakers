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
//        Debug.Log("Entered attack state");
        if (player.Animations.Animator.GetBool(player.Animations.Idle))
        {
            player.Animations.Animator.SetBool(player.Animations.Idle, false);
        }
  //        Debug.Log(lastMove); 
//          Debug.Log(player.InputReader.LastAttackInput);
    }


    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {

        player.Animations.Animator.SetBool(player.Animations.airborne, !player.GravityManager.IsGrounded);

//        Debug.Log(player.Rb.linearVelocity); 

        if (player.IsAttacking && !player.OnAttackCoolDown)
        {
//            Debug.Log("attacking");
            PerformAttack(player);
        }

        if (player.HitStun)
        {
            playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.HitStun);
        }

        if (player.IsAttacking &&  player.GravityManager.IsGrounded  && player.InputReader.curState == AttackData.States.Airborne)
        {
            player.Animations.Animator.speed = 0;
            player.Animations.ResetAttackingTrigger();
            player.Animations.Animator.Play("Neutral", 0, 0f); 
            playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Neutral);
            player.Animations.Animator.speed = 1;
        }

        
        // State swapping 
        if (!player.GravityManager.IsGrounded || player.IsAttacking) return;
        playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.Walking | PlayerStateManager.PlayerStateTypes.Crouching | PlayerStateManager.PlayerStateTypes.Jumping | PlayerStateManager.PlayerStateTypes.Running);
//        Debug.Log(player.gravityManager.GetVelocity());
    }

    private void PerformAttack(PlayerController player)
    {
        lastMove = player.InputReader.LastAttackInput.Move;
        if (!player.IsAttacking || player.OnAttackCoolDown) return;
        ChosenAttack( player,lastMove);
        cooldownCoroutine = player.StartCoroutine(EnforceCooldown(player));
    }

    private void ChosenAttack(PlayerController player, InputReader.MovementInputResult movement)
    {
        
        switch (movement)
        {
            case InputReader.MovementInputResult.Backward or InputReader.MovementInputResult.UpRight
                or InputReader.MovementInputResult.DownRight:
                player.Animations.Animator.SetBool(player.Animations.right, true);
                break;
            case InputReader.MovementInputResult.Forward or InputReader.MovementInputResult.UpLeft
                or InputReader.MovementInputResult.DownLeft:
                player.Animations.Animator.SetBool(player.Animations.left, true);
                break;
            case InputReader.MovementInputResult.None:
                break;
        }  
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
        if (!player.GravityManager.IsGrounded && player.transform.localPosition.y > 0.1f )
        {
            player.GravityManager.ApplyGravity(player);
            player.rb.MovePosition(player.transform.position + new Vector3(player.rb.linearVelocity.x , player.GravityManager.GetVelocity(), 0) * (Time.fixedDeltaTime ) );
        }

        

//        Debug.Log(player.gravityManager.GetVelocity());
    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
        player.GravityManager.ResetVelocity();
        player.Animations.ResetAttackingTrigger();
    }
}