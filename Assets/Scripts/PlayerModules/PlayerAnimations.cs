using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

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
    public static readonly int GhostJM = Animator.StringToHash("GhostJM");
    public  static readonly int GhostJH = Animator.StringToHash("GhostJH");
    public static readonly int GhostJL = Animator.StringToHash("GhostJL");
    public static readonly int Ghost2H = Animator.StringToHash("Ghost2H");
    public static readonly int GhostNeutralSpecial = Animator.StringToHash("GhostNeutralSpecial");
    public static readonly int GhostBackSpecial = Animator.StringToHash("GhostBackSpecial");
    public static readonly int GhostDownSpeciatl = Animator.StringToHash("GhostDownSpeciatl");
    public static readonly int ForwardSpecialGhost = Animator.StringToHash("ForwardSpecialGhost");
    public static readonly int GhostSuper1 = Animator.StringToHash("GhostSuper1");
    public static readonly int GhostSuper2 = Animator.StringToHash("GhostSuper2");

    #endregion





    public Dictionary<string, int> GhostAnimations = new()
    {
        {"lightNormA", GhostJL }, 
        {"medNormA", GhostJM },
        {"heavyNormA", GhostJH },
        {"heavyNormC",Ghost2H },
        {"NeutralSpecial",GhostNeutralSpecial },
        {"DownSpecial",GhostDownSpeciatl },
        {"FowardSpecial",ForwardSpecialGhost },
        {"MediumSuper",GhostSuper1 },
        {"HeavySuper",GhostSuper2 },
        {"BackwardSpecial",GhostBackSpecial },
    };
    
    internal Animator Animator;
   [SerializeField] internal Animator GhostAnimator;
    private PlayerController _player;
    internal PlayerVFX _playerVFX;
    internal int AnimationToPlay = -1;
    internal bool CancelActive;

    public bool IsActiveFrame{get; private set;}
    public bool IsRecoveryFrame{get; private set;}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GetComponentInParent<PlayerController>();
        _playerVFX = _player.GetComponent<PlayerVFX>();
        Animator = GetComponent<Animator>();
        if(GhostAnimator ) GhostAnimator.Play(GhostIdle);
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
        _player.InputReader.superPerformed = false;
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

    public void SetActiveHitBox()
    {
        _player.hitBox.gameObject.SetActive(true);
    }

    public void DisableHitBox()
    {
        _player.hitBox.gameObject.SetActive(false);
    }

    public void setAnimtorSpeed(float speed)
    {
        Animator.speed = speed;
    }

    public void resetAnimtorSpeed()
    {
        Animator.speed = 1;
    }

    public void ActivateVFX(GameObject vfx)
    {
        vfx.gameObject.SetActive(true);
    }

    public void DeactivateVFX(GameObject vfx)
    {
        vfx.gameObject.SetActive(false);
    }
    #endregion

    public void SpawnProjectile(GameObject projectileObj)
    {
        var data = _player.CharacterData.characterAttacks.ReturnAttackData(_player.InputReader.SpecialData.Attack, _player.HitDetection.otherPlayer.InputReader.curState);
        var projectile = projectileObj.GetComponent<PlayerProjectile>();
        Debug.Log("Spawned projectile");
        projectile._data = data;
        projectile.owner = _player;
        Instantiate(projectileObj, _player.ProjectilePos.position, quaternion.identity);
        
    }

   
}
