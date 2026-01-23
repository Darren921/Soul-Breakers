using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, Controls.IPlayerActions
{

    #region Class references
    public  PlayerAnimations Animations { get; private set; }
    private Controls _controls;
    private Controls.PlayerActions _playerActions;
    internal InputReader InputReader;
   [SerializeField] internal CharacterSO CharacterData;
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

    [field: SerializeField] public bool IsAttacking { get; internal set; }
    [field: SerializeField]  public bool OnAttackCoolDown { get; set; }
    
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
        Animations = GetComponent<PlayerAnimations>();
        GetOnObjectComponents();
        MinDashHeight = 1.487012f;
        RaycastDistance = 2F;
        HitDetection.OnDeath += OnPlayerDeath;
    }

    private void GetOnObjectComponents()
    {
        FrictionBox = GetComponent<BoxCollider>();
        PlayerKnockBack = GetComponent<PlayerKnockBack>();
        PlayerHitDetection = GetComponentInChildren<HitDetection>();
        _playerStateManager = GetComponent<PlayerStateManager>();
        GravityManager = GetComponentInChildren<GravityManager>();
        InputReader = GetComponent<InputReader>();
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
        GravScale = CharacterData.normGravScale;
        WalkSpeed = CharacterData.walkSpeed;
        RunSpeed = CharacterData.runSpeed;
        Health = CharacterData.health;
    }


 


    private void Update()
    {
        
        // sets animator booleans
       if(!GravityManager.IsGrounded) SetFrictionBox(false);
        AtDashHeight = !GravityManager.IsGrounded && transform.localPosition.y > MinDashHeight;

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
        switch (GravityManager.IsGrounded)
        {
            case false:
            {
                if (IsDashing || GravityManager.IsGrounded || JumpCharges == 0 || !AtDashHeight) break;
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
     
        if (context.performed && InputReader.GetValidMoveInput() is not (InputReader.MovementInputResult.Backward or InputReader.MovementInputResult.None or InputReader.MovementInputResult.Down) && GravityManager.IsGrounded && !IsRunning)
        {
            StartRun();
        }
        if (!IsRunning || !context.canceled || PlayerMove.x == 0 || _playerStateManager.currentState == _playerStateManager.States[PlayerStateManager.PlayerStateTypes.Running]) return;
        StopRun(true);
    }
    
    public void OnLight(InputAction.CallbackContext context)
    {
        ReadAttackInput(context, InputReader.AttackType.Light, Animations.Light);
    }
    
    public void OnMedium(InputAction.CallbackContext context)
    {
        ReadAttackInput(context,InputReader.AttackType.Medium,Animations.Medium);
    }

    public void OnHeavy(InputAction.CallbackContext context)
    {
        ReadAttackInput(context, InputReader.AttackType.Heavy, Animations.Heavy);
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
        if(!OnAttackCoolDown) Animations.Animator?.SetTrigger(AnimatorHash);
        if (OnAttackCoolDown || IsAttacking || !context.performed) return;
        SetAttackVars();
        

    }

    private void SetAttackVars()
    {
        Debug.Log("Set Attacking");
        Animations.Animator?.SetBool(Animations.Attacking,true);
        IsAttacking = true;    
    }

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
            case true when InputReader.CurrentMoveInput is not (InputReader.MovementInputResult.Forward or InputReader.MovementInputResult.None) && GravityManager.IsGrounded:
                print("dash back");
                PerformDash();
                break;
            case true when !GravityManager.IsGrounded:
                print("air dash");
                if (IsDashing || GravityManager.IsGrounded || JumpCharges == 0 || !AtDashHeight) break;
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
 

  
}