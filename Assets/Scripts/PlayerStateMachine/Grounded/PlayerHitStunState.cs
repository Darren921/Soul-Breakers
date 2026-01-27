using System;
using System.Collections;
using UnityEngine;


[Serializable]
public class PlayerHitStunState : PlayerBaseState
{
    private static readonly int Hit = Animator.StringToHash("Hit");

    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        //player.CharacterData.
      if(player.GravityManager.IsGrounded)  player.StartCoroutine(WaitForHitStun(player));
      else player.StartCoroutine(WaitForHitStunAirborne(player));
    }

   
   
   

    private IEnumerator WaitForHitStun(PlayerController player)
    {
        var originalSpeed = SetHitStun(player);
        //      Debug.Log("HitStun");
        yield return new WaitForSecondsRealtime(player.PlayerHitDetection.otherPlayer.CharacterData.characterAttacks.ReturnAttackData(player.PlayerHitDetection.otherPlayer.InputReader.LastAttackInput,player.PlayerHitDetection.otherPlayer.InputReader.curState).HitStun);
//        Debug.Log("HitStun complete");
        DisableHitStun(player, originalSpeed);
    }
    private IEnumerator WaitForHitStunAirborne(PlayerController player)
    {
        var originalSpeed = SetHitStun(player);
        //      Debug.Log("HitStun");
        yield return new WaitUntil(() => player.GravityManager.IsGrounded);
//        Debug.Log("HitStun complete");
        DisableHitStun(player, originalSpeed);
    } 



    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (player.HitStun) player.Animations.Animator.SetBool(Hit, true);
        if (player.InputReader.curState == AttackData.States.Airborne && player.GravityManager.IsGrounded)
        {
            playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.KnockDown);
        }
        if (!player.HitStun && !player.PlayerHitDetection.otherPlayer.Animations.IsActiveFrame) playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.Attack | PlayerStateManager.PlayerStateTypes.Crouching | PlayerStateManager.PlayerStateTypes.Dash | PlayerStateManager.PlayerStateTypes.Jumping | PlayerStateManager.PlayerStateTypes.Walking | PlayerStateManager.PlayerStateTypes.Running);
        
    }


    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (!player.GravityManager.IsGrounded)
        {
            player.GravityManager.ApplyGravity(player);
            
            player.rb.linearVelocity  = new Vector3(player.rb.linearVelocity.x,player.GravityManager.GetVelocity() ,0);
        }
    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
//        Debug.Log("Exit State");
        player.Animations.Animator.SetBool(Hit,false);
    }
    
    
    
    public class WaitForFrames : CustomYieldInstruction
    {
        private readonly int _targetFrameCount;

        public WaitForFrames(int numberOfFrames)
        {
            _targetFrameCount = Time.frameCount + numberOfFrames;
        }

        public override bool keepWaiting => Time.frameCount < _targetFrameCount;
    }
    private static float SetHitStun(PlayerController player)
    {
        var originalSpeed = player.Animations.Animator.speed;
        player.OnDisablePlayer();
        player.HitStun = true;
        player.Animations.Animator.speed = 0;
        return originalSpeed;
    }

    private static void DisableHitStun(PlayerController player, float originalSpeed)
    {
        player.OnEnablePlayer();
        player.Animations.Animator.speed = originalSpeed;
        player.HitStun = false;
    }


    
}
