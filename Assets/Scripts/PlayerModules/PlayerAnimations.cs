using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour 
{
    #region Animator Hashed variables
    public readonly int Idle = Animator.StringToHash("Idle");
    public readonly int Walking = Animator.StringToHash("Walking");
    public readonly int Running = Animator.StringToHash("Running");
    public readonly int Jump = Animator.StringToHash("Jumping");
    public readonly int Crouch = Animator.StringToHash("Crouching");
    public readonly int Dashing = Animator.StringToHash("Dashing");
    public readonly int DashDir = Animator.StringToHash("DashDir");
    public readonly int AirDashing = Animator.StringToHash("Dashing");
    public readonly int Attacking = Animator.StringToHash("Attacking");
    public readonly int Light = Animator.StringToHash("Light");
    public readonly int Heavy = Animator.StringToHash("Heavy");
    public readonly  int Medium = Animator.StringToHash("Medium");
    public readonly int Special = Animator.StringToHash("Special");
    public readonly int left = Animator.StringToHash("Left");
    public readonly int right = Animator.StringToHash("Right");
    public readonly int airborne = Animator.StringToHash("Airborne");
    public readonly int blocking = Animator.StringToHash("Blocking");
    public readonly int StartUp = Animator.StringToHash("StartUp");
    public readonly int Active = Animator.StringToHash("Active");
    public readonly int Recovery = Animator.StringToHash("Recovery");
    public readonly int WalkDir = Animator.StringToHash("WalkDir");
    public readonly int Grab = Animator.StringToHash("Grab");
    public readonly int Grabbed = Animator.StringToHash("Grabbed");
    public  readonly int Super = Animator.StringToHash("Super");
    public  readonly int Hit = Animator.StringToHash("Hit");
    public  readonly int CancelDectect = Animator.StringToHash("CancelDectect");
    #endregion
    #region Ghost Animator Hashed variables
    public  readonly int GhostIdle = Animator.StringToHash("GhostIdle");
    public  readonly int GhostCrouch = Animator.StringToHash("GhostCrouch");
    public  readonly int GhostCrouchBlock = Animator.StringToHash("GhostCrouchBlock");
    public  readonly int GhostJM = Animator.StringToHash("GhostJM");
    public  readonly int GhostJH = Animator.StringToHash("GhostJH");
    public readonly int GhostJL = Animator.StringToHash("GhostJL");
    #endregion

    
    internal Animator Animator;
   [SerializeField] internal Animator GhostAnimator;
    private PlayerController _player;
   
    internal int AnimationToPlay = -1;
    internal bool CancelActive;

    public bool IsActiveFrame{get; private set;}
    public bool IsRecoveryFrame{get; private set;}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GetComponentInParent<PlayerController>();
        Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Animator?.SetBool(airborne, !_player.GravityManager.IsGrounded);

        switch (_player.GravityManager.IsGrounded)
        {
            case true:
                Animator?.SetBool(Crouch,  _player.IsCrouching);
                if(!_player.Decelerating) Animator?.SetBool(Walking, _player.IsWalking);
                Animator?.SetBool(Running, _player.IsRunning);
                break;
            case false:
                Animator?.SetBool(Crouch, false);
                 Animator?.SetBool(Walking, false);
                Animator?.SetBool(Running, false);
                break;
        }
        if (Animator) IsActiveFrame = Animator.GetBool(Active);
        if (Animator) IsRecoveryFrame = Animator.GetBool(Recovery);

    }
    
    
    #region Attack Animations System

    public void ResetAttackingTrigger()
    {
    //    Animator.StopPlayback();
        //This may need to change to separate ones for each attack
        // This is used at the end of each animation 
        _player.IsAttacking = false;
//        print("Reset attacking trigger");
        Animator?.ResetTrigger(StartUp);
        Animator?.ResetTrigger(Attacking);
        Animator?.SetBool(Light,false);
        Animator?.SetBool(Medium,false);
        Animator?.SetBool(Heavy,false);
        Animator?.SetBool(Active,false);
        Animator?.SetBool(Super,false);
        Animator?.SetBool(Special,false);
        _player.InputReader.CurrentAttackInput.IsBeingUsed = false;
    }

    public void SetUpStartupFrame()
    {
        Animator?.SetBool(StartUp,true);
        
    }

    public void SetUpActiveFrame()
    {
        Animator?.SetBool(Active,true);
        Animator?.SetBool(StartUp,false);

    }

    public void SetUpRecoveryFrame()
    {
        
        Animator?.SetBool(Recovery,true);
        Animator?.SetBool(Active,false);

    }

    public void ResetRecoveryFrame()
    {
        Animator?.SetBool(Recovery,false);
        _player.CancelPlaying = false;
    }

    #endregion

  

    public void SetAttackingHash(InputReader.AttackType inputType)
    {
//        print(inputType);
        if(inputType.ToString().Contains("Super") && _player.superMeter < 100) return;
        switch (inputType)
        {
            
            case InputReader.AttackType.Light:
                Animator?.SetBool(Light,true);
                break;
            case InputReader.AttackType.Grab:
                Animator?.SetBool(Light,true);
                Animator?.SetBool(Medium,true);
                break;
            case InputReader.AttackType.Heavy:
                Animator?.SetBool(Heavy,true);
                break;
            case InputReader.AttackType.Medium:
                Animator?.SetBool(Medium,true);
                break;
            case InputReader.AttackType.SuperLight:
                Animator?.SetBool(Super,true);
                Animator?.SetBool(Light,true);
                break;
            case InputReader.AttackType.SuperMedium:
                Animator?.SetBool(Super,true);
                Animator?.SetBool(Medium,true);
                break;
            case InputReader.AttackType.SuperHeavy:
                Animator?.SetBool(Super,true);
                Animator?.SetBool(Heavy,true);
                break;
            case InputReader.AttackType.Special:
                Animator?.SetBool(Special,true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null);
        }

        if (Animator )
        {
            if(Animator.GetBool(Super)) return;
            if (Animator.GetBool(Heavy) && Animator.GetBool(Light))
            {
                Animator.SetBool(Light,false);
            }
            else if (Animator.GetBool(Medium) && Animator.GetBool(Heavy))
            {
                Animator.SetBool(Medium,false);
            }
            Animator?.SetBool(Attacking,true);
        }
    }
}
