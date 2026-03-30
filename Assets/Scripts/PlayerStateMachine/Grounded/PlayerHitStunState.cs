using System;
using System.Collections;
using UnityEngine;


[Serializable]
public class PlayerHitStunState : PlayerBaseState
{

  
    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        
        player.Animations.ResetAttackingTrigger();
      if(player.GravityManager.IsGrounded)  player.StartCoroutine(WaitForHitStun(player));
      else player.StartCoroutine(WaitForHitStunAirborne(player));
    }

   
   
   

    private IEnumerator WaitForHitStun(PlayerController player)
    {
        var originalSpeed = SetHitStun(player);
        Debug.Log("HitStun");
        Debug.Log(player.HitDetection.otherPlayer.InputReader.CurAttackData.HitStun);
        yield return new WaitForSeconds(player.HitDetection.otherPlayer.InputReader.CurAttackData.HitStun);
        Debug.Log("HitStun complete"); 
        DisableHitStun(player, originalSpeed);
    }

    private IEnumerator SetHitStop(PlayerController player)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(player.HitDetection.otherPlayer.InputReader.CurAttackData.HitStop);
        Time.timeScale = 1;
    }

 

    private IEnumerator WaitForHitStunAirborne(PlayerController player)
    {
        Debug.Log("HitStunAirborne");
        var originalSpeed = SetHitStun(player);
        //      Debug.Log("HitStun");
        yield return new WaitUntil(() => player.GravityManager.IsGrounded);
//        Debug.Log("HitStun complete");
        DisableHitStun(player, originalSpeed);
    } 



    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (player.InputReader.curState == AttackData.States.Airborne && player.GravityManager.IsGrounded)
        {
            playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.KnockDown);
        }

//        Debug.Log(player.PlayerHitDetection.otherPlayer.Animations.IsActiveFrame);
//        Debug.Log(!player.HitStun);
        if (!player.HitStun && !player.HitDetection.otherPlayer.Animations.IsActiveFrame)
        {
//            Debug.Log("Entered ");
            playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.Attack | PlayerStateManager.PlayerStateTypes.Crouching | PlayerStateManager.PlayerStateTypes.Dash | PlayerStateManager.PlayerStateTypes.Jumping | PlayerStateManager.PlayerStateTypes.Walking | PlayerStateManager.PlayerStateTypes.Running);
        }
        
    }


    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (!player.GravityManager.IsGrounded && !player.PlayerKnockBack._isBeingKnockedBack)
        {
            player.GravityManager.ApplyGravity(player);
            
            player.rb.MovePosition(player.transform.position + new Vector3(player.rb.linearVelocity.x , player.GravityManager.GetVelocity(), 0) * (Time.fixedDeltaTime ) );
        }
    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
//        Debug.Log("Exit State");
        player.Animations.Animator.SetBool(player.Animations.Hit, false);
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
    private  float SetHitStun(PlayerController player)
    {
        player.StartCoroutine(SetHitStop(player));
        var originalSpeed = player.Animations.Animator.speed;
        player.OnDisablePlayer();
        player.Animations.Animator.speed = 0;
        player.HitStun = true;
        return originalSpeed;
    }

    private static void DisableHitStun(PlayerController player, float originalSpeed)
    {
        player.OnEnablePlayer();
        player.Animations.Animator.speed = originalSpeed;
        player.HitStun = false;
    }


    
}
