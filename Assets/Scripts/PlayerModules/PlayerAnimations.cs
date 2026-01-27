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
    #endregion
    internal Animator Animator;

    private PlayerController _player;
    
    public bool IsActiveFrame{get; private set;}
    public bool IsRecoveryFrame{get; private set;}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GetComponent<PlayerController>();
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
        //This may need to change to separate ones for each attack
        // This is used at the end of each animation 
        _player.IsAttacking = false;
//        print("Reset attacking trigger");
        Animator?.ResetTrigger(StartUp);
        Animator?.ResetTrigger(Attacking);
        Animator?.SetBool(Light,false);
        Animator?.SetBool(Medium,false);
        Animator?.SetBool(Heavy,false);
        Animator?.SetBool(left, false);
        Animator?.SetBool(right, false);
        Animator?.SetBool(Active,false);
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
    }

    #endregion

}
