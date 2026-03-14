using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class PlayerJumpingState : PlayerBaseState
{
    #region Standard Jump Variables

   float velocity;

    #endregion

    private float xJumpVal; // check Try  jump method for changes 
    private Collider collider;
    private bool jumpTriggered;
    private bool atJumpHeight;
    private bool doubleJumpReady ;
    private bool atAirDashHeight;
    
    private Coroutine jumpCoroutine;
    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        player.JumpCharges = player.CharacterData.jumpCharges;
        //apply jump immediately when entering state to prevent update glitches   
        collider = player.GetComponent<Collider>();
        player.Animations.Animator.SetBool(player.Animations.Jump, true);
        TryJump(player);       
        player.JumpCharges--;
        player.OnJump += HandleJumpInput; 

    }

  
     
    

    private void HandleJumpInput()
    {
        jumpTriggered = true;
    }

    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        
        // check to see if player is jumping 
        switch (player.GravityManager.IsGrounded)
        {
            case true:
                player.Animations.Animator.SetBool(player.Animations.Jump, false);
                break;
            case false:
                player.Animations.Animator.SetBool(player.Animations.Jump, true);
                break;
        }
        
        doubleJumpReady =  player.JumpCharges > 0 && !player.SuperJumpActive;
    
  
        //Transitioning states 
        if (!player.GravityManager.IsGrounded)
        {
            if(player.IsDashing && player.AtDashHeight && player.JumpCharges > 0) playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.AirDash);
            playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Attack );
            
            if (player.JumpCharges > 0 && doubleJumpReady && jumpTriggered)
            {
                Debug.Log("Double Jumpped");
                player.Animations.Animator.SetBool(player.Animations.Jump, true);
                doubleJumpReady = false;
                player.JumpCharges--;
                TryJump(player);
                jumpTriggered =  false;
            }
        }
        else
        {
            
            playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.Walking | PlayerStateManager.PlayerStateTypes.Crouching | PlayerStateManager.PlayerStateTypes.Jumping | PlayerStateManager.PlayerStateTypes.Walking );
            if (!player.GravityManager.IsGrounded)
            {
                switch (player.InputReader.CurrentMoveInput)
                {
                    case InputReader.MovementInputResult.Backward:
                        playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Walking);
                        break;
                    case InputReader.MovementInputResult.Forward:
                        playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Running);
                        break;
                }
            }
        }


    }

    private void TryJump(PlayerController player)
    {
        // jumping based off on custom  gravity to ensure the player jumps to same height each time 
        velocity = player.GravityManager.SetJumpVelocity(player);
        var moveInput = player.InputReader.GetValidMoveInput();
//        Debug.Log(player.InputReader.CurrentMoveInput);

        
        xJumpVal = moveInput switch
        {
            InputReader.MovementInputResult.Up => 0,
            InputReader.MovementInputResult.Forward => !player.Reversed ? 3 : -3,   
            InputReader.MovementInputResult.Backward => !player.Reversed ? -3 : 3,
            InputReader.MovementInputResult.UpRight => 3,
            InputReader.MovementInputResult.UpLeft => -3,
            _ => xJumpVal
        };
        
    }


    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        //performing jump and applying custom gravity 
        player.rb.MovePosition(player.transform.position + new Vector3( player.AtBorder ?  0 :  xJumpVal  , player.GravityManager.GetVelocity(), 0) * Time.fixedDeltaTime);
       // player.rb.linearVelocity = new Vector3(xJumpVal, player.GravityManager.GetVelocity(), 0);
        if (!player.GravityManager.IsGrounded  )
        {
            player.GravityManager.ApplyGravity(player);
        }
//               Debug.Log(player.GravityManager.GetVelocity());

    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
        // if (!player.IsAttacking)
        // {
        //     //   Debug.Log("Reseting velo");
        //     player.gravityManager.ResetVelocity();
        // }

        player.Animations.Animator.SetBool(player.Animations.Jump, false);
     if(player.GravityManager.IsGrounded)   player.GravityManager.ResetVelocity();
    // var GroundSnapping = player.GetComponent<Collider>().ClosestPoint(player.transform.position);
        xJumpVal = 0f;
        atJumpHeight = false;
        jumpTriggered =  false;
        //   Debug.Log("Exiting playerJumpingState");
        player.OnJump -= HandleJumpInput;
        player.SuperJumpActive = false;
    }
}