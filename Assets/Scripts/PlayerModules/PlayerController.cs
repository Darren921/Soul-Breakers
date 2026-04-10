using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour, Controls.IPlayerActions,IComparable
{

    #region Class references
    public  PlayerAnimations Animations { get; private set; }
    internal Controls _controls;
    private Controls.PlayerActions _playerActions;
    internal InputReader InputReader;
   [SerializeField] internal CharacterSO CharacterData;
    internal GravityManager GravityManager;
    public HitDetection HitDetection;
    internal PlayerKnockBack PlayerKnockBack;
    public PlayerStateManager _playerStateManager;
    internal PlayerVFX playerVFX;
    internal PlayerUI playerUI;
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
    [SerializeField]internal float superMeter;
    internal bool PlayersColliding;
    [field : SerializeField]  public GameObject playerModel { get; private set; }

    public bool PlayerConnected { get;  private set; }
    internal bool canCancel;
    internal DetectOtherPlayer _detector; 
    internal bool CancelPlaying;
   [SerializeField] internal Transform ProjectilePos; 
    #endregion
    

    public bool isDead { get; private set; } 

    private void Awake()
    {
        Animations = GetComponentInChildren<PlayerAnimations>();
        playerVFX = GetComponentInChildren<PlayerVFX>();
        GetOnObjectComponents();
        MinDashHeight = 1.487012f;
        HitDetection.OnDeath += OnPlayerDeath;
    }

    private void GetOnObjectComponents()
    {
        playerUI = GetComponent<PlayerUI>();
        FrictionBox = GetComponent<BoxCollider>();
        PlayerKnockBack = GetComponent<PlayerKnockBack>();
        HitDetection = GetComponentInChildren<HitDetection>();
        _playerStateManager = GetComponent<PlayerStateManager>();
        GravityManager = GetComponentInChildren<GravityManager>();
        InputReader = GetComponent<InputReader>();
        rb = GetComponent<Rigidbody>();
        _detector = GetComponentInChildren<DetectOtherPlayer>();
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
        if (device is not null)
        {
            _playerActions = _controls.Player;
        }
        PlayerConnected = device is not null;
        SetUpCallBacks();
        OnEnablePlayer();
        EnablePlayerUI();
        SetUpCharacterVariables();
    }

    public void DisconnectPlayer()
    {
        PlayerConnected = false;
        _playerActions.Disable();

        _controls = null;
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
        if(PlayerConnected)  _controls.UI.Disable();
    }

    public void EnablePlayerUI()
    {
        if(PlayerConnected) _controls.UI.Enable();
    }

    public void OnEnablePlayer()
    {
        if(PlayerConnected) _playerActions.Enable();
    }

    public void OnDisablePlayer()
    {
        if(PlayerConnected) _playerActions.Disable();
    }

    private void OnPlayerDeath()
    {
        if (Health <= 0)
        {
            isDead = true;
            //gameObject.SetActive(false);
            
            
            Time.timeScale = 1; 
        }
        InputReader.enabled = false;
        _playerStateManager.ResetStateMachine();
        StopAllCoroutines();
     if(PlayerConnected)  _playerActions.RemoveCallbacks(this);
        HitDetection.OnDeath -= OnPlayerDeath;
        OnDisablePlayer();
        PauseManager.Instance?.UnregisterPlayer(this);
    }

    private void OnDestroy()
    {
        HitDetection.OnDeath -= OnPlayerDeath;
        OnDisablePlayer();
        DisablePlayerUI();
        PauseManager.Instance?.UnregisterPlayer(this);

    }

    

    private void SetUpCharacterVariables()
    {
        //All character data is added here (future ones must be added here as well)

        JumpHeight = CharacterData.jumpHeight;
        GravScale = CharacterData.normGravScale;
        WalkSpeed = CharacterData.walkSpeed;
        RunSpeed = CharacterData.runSpeed;
        if(Health < 100) return; 
        Health = CharacterData.health;
    }


 


    private void Update()
    {
        if (InputReader.CurrentMoveInput == InputReader.MovementInputResult.Backward && IsRunning) IsRunning = false;   
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
//        print($"{!IsRunning } {InputReader.GetValidMoveInput()}");
        if (!IsRunning || InputReader.GetValidMoveInput() is not InputReader.MovementInputResult.Backward) return; 
        StopRun(false);
    }
    
    public void OnLight(InputAction.CallbackContext context)
    {
        ReadAttackInput(context, InputReader.AttackType.Light);
    }
    
    public void OnMedium(InputAction.CallbackContext context)
    {
        ReadAttackInput(context,InputReader.AttackType.Medium);
    }

    public void OnHeavy(InputAction.CallbackContext context)
    {
        ReadAttackInput(context, InputReader.AttackType.Heavy);
    }
    
    public void OnSpecial(InputAction.CallbackContext context)
    {
        ReadAttackInput(context, InputReader.AttackType.Special);
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
   
    private void ReadAttackInput(InputAction.CallbackContext context,InputReader.AttackType type )
    {
        PlayerAttackAction?.Invoke(type);
        if (IsAttacking || !context.performed) return;
        SetAttackVars();
        

    }

    private void SetAttackVars()
    {
//        Debug.Log("Set Attacking");
        IsAttacking = true;    
        
    }

    public void SetFrictionBox(bool value)
    {
//        print("Used" + value);
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
        if(IsRunning) return;
        print("Starting run");
        IsRunning = true;
        IsWalking = false;
    }
    private void StopRun(bool startWalk)
    {
        IsRunning = false;
        print("Stop run");
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

  

    public IEnumerator OnDeceleration(PlayerController player)
    {
        Decelerating = true;
        yield return new WaitForSeconds(player.DecelerationDuration);
        Decelerating = false;
        SetFrictionBox(false);
        while (Decelerating)
        {
            SetFrictionBox(true);
            Debug.Log(rb.linearVelocity);
            player.rb.MovePosition( player.transform.position + new Vector3(Mathf.MoveTowards(player.rb.linearVelocity.x, 0f, 1 * Time.deltaTime), 0, player.rb.linearVelocity.z) * Time.fixedDeltaTime );
         // player.rb.linearVelocity = new Vector3(Mathf.MoveTowards(player.rb.linearVelocity.x, 0f, 1 * Time.deltaTime), 0, player.rb.linearVelocity.z); 
           yield return new WaitForFixedUpdate();
          

        }

    }
    #endregion


    public int CompareTo(object obj)
    {
        if(obj is null) return 1;
        var other = obj as PlayerController;
        return transform.position.x.CompareTo(other.transform.position.x);
    }
}