using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, Controls.IPlayerActions
{


    #region Animator Hashed variables

    internal readonly int Idle = Animator.StringToHash("Idle");
    private readonly int Walking = Animator.StringToHash("Walking");
    private readonly int Running = Animator.StringToHash("Running");
    internal readonly int Jump = Animator.StringToHash("Jumping");
    private readonly int Crouch = Animator.StringToHash("Crouching");
    internal readonly int Dashing = Animator.StringToHash("Dashing");
    internal readonly int AirDashing = Animator.StringToHash("Dashing");

    private int Attacking => Animator.StringToHash("Attacking");
    private int Light => Animator.StringToHash("Light");
    private int Heavy => Animator.StringToHash("Heavy");
    private int Medium => Animator.StringToHash("Medium");
    private int Special => Animator.StringToHash("Special");
    
    internal int left = Animator.StringToHash("Left");
    internal int right = Animator.StringToHash("Right");
    internal int airborne = Animator.StringToHash("Airborne");
    internal int blocking = Animator.StringToHash("Blocking");//NEW, FOR BLOCKING
    private int StartUp = Animator.StringToHash("StartUp");
    private int Active = Animator.StringToHash("Active");
    private int Recovery = Animator.StringToHash("Recovery");
    internal readonly int WalkDir = Animator.StringToHash("WalkDir");

    #endregion

    #region Class references

    private Controls _controls;
    private Controls.PlayerActions _playerActions;
    internal InputReader InputReader;
   [SerializeField] internal CharacterSO CharacterData;
    internal Animator Animator;
    internal GravityManager GravityManager;
    internal HitDetection PlayerHitDetection;
    internal PlayerKnockBack PlayerKnockBack;
    public PlayerStateManager _playerStateManager;

    #endregion
    

    #region Crouching and Dashing variables

    [SerializeReference]  internal bool IsCrouching;
    [SerializeField]  internal bool IsDashing;
    [SerializeField]  internal bool AtDashHeight;

    #endregion

    #region PlayerActions
    public Action OnJump;
    public  Action<InputReader.AttackType> PlayerAttackAction;
    #endregion
    
    #region Attack Check Variables

    [field: SerializeField] public bool IsAttacking { get; private set; }
    [field: SerializeField]  public bool OnAttackCoolDown { get; set; }
    [field: SerializeField]  public bool IsActiveFrame{get; private set;}
    
    public bool IsBeingAttacked;//NEW, FOR BLOCKING, idk where else to put this

    #endregion
    
    #region Move Variables
    [field: SerializeField]public Vector3 PlayerMove { get; private set; }
    [SerializeField] internal float WalkSpeed;
    [SerializeField] internal float RunSpeed;
    [SerializeField] internal bool IsWalking;
    [SerializeField] internal bool IsRunning;
    #endregion

    #region Jump Variables

    [Tooltip("Origin of the grounded Raycast, DO NOT TOUCH PLEASE")] 
    [SerializeField] internal Transform raycastPos;
    [SerializeField] internal int JumpCharges;
    [SerializeField] internal float JumpHeight;
    internal float RaycastDistance; //2.023f
    internal float GravScale; // (Hold for now )  character data affects gravity 5 
    [SerializeField] internal float Velocity;
    internal Rigidbody rb;
     [SerializeField] internal InputReader.MovementInputResult DashDir;
     [SerializeField] internal bool IsGrounded;
     [SerializeField] internal bool SuperJumpActive;
     [SerializeField] internal GameObject hitBox;
     [SerializeField] internal bool JumpPressed;

    #endregion

    #region Decelerating Variables
    [SerializeField]  private float DecelerationDuration ;
    [SerializeField] internal bool Decelerating;
    [SerializeField]   private float _elapsedTime;
    [SerializeField]  internal bool DecelActive;
    #endregion
  
    #region Misc variables

    [SerializeField] internal bool Reversed;
    [SerializeField]  internal bool HitStun;
    [SerializeField]  internal float Health;
    [SerializeField]  internal bool AtBorder;
    [SerializeField]  internal bool DashMarcoActive;
    [SerializeField] private float MinDashHeight;
    private BoxCollider FrictionBox;
    #endregion

    public bool isDead { get; private set; } 

    private void Awake()
    {
        GetOnObjectComponents();
        MinDashHeight = 1.487012f;
        IsGrounded = true;
        RaycastDistance = 1.807687f;
        HitDetection.OnDeath += OnPlayerDeath;
    }

    private void GetOnObjectComponents()
    {
        FrictionBox = GetComponent<BoxCollider>();
        PlayerKnockBack = GetComponent<PlayerKnockBack>();
        PlayerHitDetection = GetComponentInChildren<HitDetection>();
        _playerStateManager = GetComponent<PlayerStateManager>();
        GravityManager = GetComponent<GravityManager>();
        InputReader = GetComponent<InputReader>();
        Animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        PauseManager.Instance.RegisterPlayer(this);
    }

    public void InitializePlayer(InputDevice device)
    {
        //Setup all player controls (note if players > inputs, players aren't set up)
        _controls = new Controls();
        //creates a new set of controls for the chosen device 
        _controls.devices = new[] { device };
        _playerActions = device != null ? _controls.Player : new Controls.PlayerActions();
        SetUpCallBacks();
        OnEnablePlayer();
        SetUpCharacterVariables();
    }

    private void SetUpCallBacks()
    {
        _playerActions.Run.performed += OnRun;
        _playerActions.Run.canceled += OnRun;
        _playerActions.DashMacro.performed += OnDashMacro;
        _playerActions.DashMacro.canceled += OnDashMacro;
        _playerActions.Dash.performed += OnDash;
        _playerActions.Move.performed += OnMove;
        _playerActions.Move.canceled += OnMove;
        _playerActions.Light.performed += OnLight;
        _playerActions.Medium.performed += OnMedium;
        _playerActions.Heavy.performed += OnHeavy;
        _playerActions.Special.performed += OnSpecial;
        _playerActions.Jumping.performed += OnJumping;
        _playerActions.SuperJump.performed += OnSuperJump;
    }

    public void DisablePlayerUI()
    {
        _controls.UI.Disable();
    }

    public void EnablePlayerUI()
    {
        _controls.UI.Enable();
    }

    public void OnEnablePlayer()
    {
        _playerActions.Enable();
    }

    public void OnDisablePlayer()
    {
        _playerActions.Disable();
    }

    private void OnPlayerDeath()
    {
        if (Health <= 0)
        {
            isDead = true;
            gameObject.SetActive(false);
        }
        InputReader.enabled = false;
        _playerStateManager.ResetStateMachine();
        StopAllCoroutines();
        _playerActions.RemoveCallbacks(this);
        HitDetection.OnDeath -= OnPlayerDeath;
        _playerActions.Disable();
        PauseManager.Instance?.UnregisterPlayer(this);
    }

    private void OnDestroy()
    {
        _playerActions.RemoveCallbacks(this);
        HitDetection.OnDeath -= OnPlayerDeath;
        _playerActions.Disable();
        PauseManager.Instance?.UnregisterPlayer(this);

    }


    private void SetUpCharacterVariables()
    {
        //All character data is added here (future ones must be added here as well)

        JumpHeight = CharacterData.jumpHeight;
        GravScale = CharacterData.gravScale;
        WalkSpeed = CharacterData.walkSpeed;
        RunSpeed = CharacterData.runSpeed;
        Health = CharacterData.health;
    }


 
    private void OnCollisionStay(Collision collision)
    {
        IsGrounded = GravityManager.CheckGrounded(this);
        if (!collision.gameObject.CompareTag("Player")) return;
//        Debug.Log(collision.gameObject.name);
        if(IsGrounded) SetFrictionBox(true);
    }

   
    private void OnCollisionExit(Collision other)
    {
        IsGrounded = false;
        SetFrictionBox(false);
    }

    private void Update()
    {
        // sets animator booleans
        Animator?.SetBool(airborne, !IsGrounded);
        
        switch (IsGrounded)
        {
            case true:
               Animator?.SetBool(Crouch, IsCrouching);
               Animator?.SetBool(Walking, IsWalking);
               Animator?.SetBool(Running, IsRunning);
                break;
            case false:
               Animator?.SetBool(Crouch, false);
               Animator?.SetBool(Walking, false);
               Animator?.SetBool(Running, false);
               SetFrictionBox(false);
                break;
        }
        if (Animator) IsActiveFrame = Animator.GetBool(Active);
        AtDashHeight = !IsGrounded && transform.localPosition.y > MinDashHeight;

    }

   


    // Contains onMove, onDash,etc.
    #region Control contexts 
    public void OnMove(InputAction.CallbackContext context)
    {
        //Turns off running and walking when player releases context or player stops F
        //default till running begins
       
        PlayerMove = context.ReadValue<Vector3>();
        if (!IsRunning &&
            _playerStateManager.currentState !=
            _playerStateManager.States[PlayerStateManager.PlayerStateTypes.Running] && PlayerMove.x != 0)
        {
            IsWalking = true;
        }
        else if (PlayerMove.x == 0)
        {
            IsWalking = false;
        }
        IsCrouching = PlayerMove.y < 0;

    }
    public void OnDash(InputAction.CallbackContext context)
    {
        switch (IsGrounded)
        {
            case false:
            {
                if (IsDashing || IsGrounded || JumpCharges == 0 || !AtDashHeight) break;
                print("entered dash");
                PerformDash(true);
                break;
            }
            case true:
            {
                if (IsDashing || InputReader.CurrentMoveInput == InputReader.MovementInputResult.Forward) return;
                print("entered dash");
                PerformDash();
                break;
            }
        }
    }
    public void OnRun(InputAction.CallbackContext context)
    {
     
        if (context.performed && InputReader.GetValidMoveInput() is not (InputReader.MovementInputResult.Backward or InputReader.MovementInputResult.None or InputReader.MovementInputResult.Down) && IsGrounded && !IsRunning)
        {
            StartRun();
        }
        if (!IsRunning || !context.canceled || PlayerMove.x == 0 || _playerStateManager.currentState == _playerStateManager.States[PlayerStateManager.PlayerStateTypes.Running]) return;
        StopRun(true);
    }
    
    public void OnLight(InputAction.CallbackContext context)
    {
        ReadAttackInput(context, InputReader.AttackType.Light, Light);
    }
    
    public void OnMedium(InputAction.CallbackContext context)
    {
        ReadAttackInput(context,InputReader.AttackType.Medium,Medium);
    }

    public void OnHeavy(InputAction.CallbackContext context)
    {
        ReadAttackInput(context, InputReader.AttackType.Heavy, Heavy);
    }
    
    public void OnSpecial(InputAction.CallbackContext context)
    {
        //ReadAttackInput(context, InputReader.AttackType.Special, Special);
        Debug.Log("Special attack triggered");
    }
    public void OnJumping(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnJump?.Invoke();
        }
    }

    public void OnSuperJump(InputAction.CallbackContext context)
    {
        SuperJumpActive = true;
    }

    #endregion
   
    private void ReadAttackInput(InputAction.CallbackContext context,InputReader.AttackType type ,int AnimatorHash  )
    {
        PlayerAttackAction?.Invoke(type);
        Animator?.SetTrigger(AnimatorHash);
        if (OnAttackCoolDown || IsAttacking || !context.performed) return;
        SetAttackVars();
        

    }

    private void SetAttackVars()
    {
        Debug.Log("Set Attacking");
        Animator?.SetBool(Attacking,true);
        IsAttacking = true;    }

    public void SetFrictionBox(bool value)
    {
        FrictionBox.enabled = value;
    }
    #region Run/Dash
    private void PerformDash(bool isAirDashing = false)
    {
        IsDashing = true;
        DashDir = InputReader.CurrentMoveInput;
        IsRunning = false;
        IsWalking = false;
    }
    private void StartRun()
    {
        IsRunning = true;
        IsWalking = false;
    }
    private void StopRun(bool startWalk)
    {
        IsRunning = false;
        if (startWalk) IsWalking = true;
    }


    public void OnDashMacro(InputAction.CallbackContext context)
    {
   
        //shortcut for dash 
        print("entered dash Marco");
        DashMarcoActive = true;
        switch (context.performed)
        {
            case true when InputReader.CurrentMoveInput is not (InputReader.MovementInputResult.Forward or InputReader.MovementInputResult.None) && IsGrounded:
                print("dash back");
                PerformDash();
                break;
            case true when !IsGrounded:
                print("air dash");
                if (IsDashing || IsGrounded || JumpCharges == 0 || !AtDashHeight) break;
                PerformDash(true);
                break;
            case true:
            {
                print("sprinting");
                StartRun();
                if (!context.canceled) return;
                print("sprint cancel ");
                StopRun(false);
                break;
            }
        }

        if (!context.canceled) return;
        DashMarcoActive = false;
    }

  
    internal IEnumerator DecelerationCurve(PlayerController player)
    {
        Decelerating = true;

        while (_elapsedTime < DecelerationDuration && player.DecelActive)
        {
            player.rb.linearVelocity = Vector3.Lerp(player.rb.linearVelocity, new Vector3(0f, 0, 0), DecelerationDuration);
            _elapsedTime += Time.deltaTime;
            yield return null;
        }

        Decelerating = false;
        _elapsedTime = 0f;
    }
    

    #endregion
 

    #region Attack Animations System

    public void ResetAttackingTrigger()
    {
        //This may need to change to separate ones for each attack
        // This is used at the end of each animation 
        IsAttacking = false;
        print("Reset attacking trigger");
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