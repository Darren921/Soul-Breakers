using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerStateManager : MonoBehaviour
{
    
    internal Dictionary<PlayerStateTypes, PlayerBaseState> States;
    public string CurrentStateName => currentState?.GetType().Name;

    [field:SerializeField] public PlayerBaseState currentState { get; private set; }
    [field:SerializeField] internal PlayerBaseState lastState { get; private set; }
 
    private PlayerController _player;

    [Flags]
    public enum PlayerStateTypes
    {
        None = 0,
        Neutral = 1 << 0,
        Crouching = 1 << 1,
        Jumping = 1 << 2,
        Walking = 1 << 3,
        Running = 1 << 4,
        Attack = 1 << 5,
        CrouchMove = 1 << 6,
        Dash = 1 << 7,
        AirDash = 1 << 8,
        HitStun = 1 << 9,
        Blocking = 1 << 10,
        Grab = 1 << 11,
        KnockDown = 1 << 12,
    }

 
    #region StatesList
    public PlayerBaseState[] AirborneStates { get; private set; }
    public PlayerBaseState[] StandingStates { get; private set; }

    public PlayerBaseState[] CrouchingStates { get; private set; }
    #endregion
    
    #region States
    private void SortStates()
    {
        AirborneStates = new[]
        {
            States[PlayerStateTypes.AirDash],
            States[PlayerStateTypes.Jumping],
        };
        StandingStates = new[]
        {
            States[PlayerStateTypes.Walking],
            States[PlayerStateTypes.Running],
            States[PlayerStateTypes.Neutral],
            States[PlayerStateTypes.Dash],
            States[PlayerStateTypes.Grab],
        };
        CrouchingStates = new[]
        {
            States[PlayerStateTypes.Crouching],
        };
    }

    private void CreateStatesList()
    {
        //This dictionary makes it that each state is available and not duped 
        States = new Dictionary<PlayerStateTypes, PlayerBaseState>
        {
            { PlayerStateTypes.Neutral, new PlayerNeutralState() },
            { PlayerStateTypes.Crouching, new PlayerCrouchingState() },
            { PlayerStateTypes.Jumping, new PlayerJumpingState() },
            { PlayerStateTypes.Walking, new PlayerWalkingState() },
            { PlayerStateTypes.Running, new PlayerRunningState() }, 
            { PlayerStateTypes.Attack, new PlayerAttackState() },
            { PlayerStateTypes.CrouchMove, new PlayerCrouchMoveState() },
            { PlayerStateTypes.Dash, new PlayerDashState() },
            { PlayerStateTypes.AirDash, new PlayerAirDashState() },
            { PlayerStateTypes.HitStun, new PlayerHitStunState() },
            { PlayerStateTypes.Blocking, new PlayerBlockingState() },
            { PlayerStateTypes.Grab, new PlayerGrabState() },
            { PlayerStateTypes.KnockDown, new PlayerKnockDownState() }
        };
    }

    #endregion
 
    void Awake()
    {
        CreateStatesList();
        SortStates();
    }
    void Start()
    {
        _player = GetComponent<PlayerController>();
        ResetStateMachine();
    }
    void Update()
    {
        currentState.UpdateState(this, _player);
    }

    void FixedUpdate()
    {
        currentState.FixedUpdateState(this, _player);
    }
   
  

    public void SwitchState(PlayerStateTypes newType)
    {
        if (States.TryGetValue(newType, out var state))
        {
            currentState?.ExitState(this, _player);
            lastState = currentState;
            currentState = state;
//            Debug.Log(currentState.GetType().Name);
            currentState?.EnterState(this, _player);
        }
    }
    
    public void ResetStateMachine()
    {
        currentState = States[PlayerStateTypes.Neutral];
    }


    public void CheckForTransition(PlayerStateTypes transitionType)
    {
       
        if (transitionType.HasFlag(PlayerStateTypes.AirDash))
        {
            if (_player.IsDashing && _player.AtDashHeight)
                SwitchState(PlayerStateTypes.AirDash);
            return;
        }
        if (transitionType.HasFlag(PlayerStateTypes.Jumping))
            if (_player.PlayerMove.y > 0)
                SwitchState(PlayerStateTypes.Jumping);
        if (transitionType.HasFlag(PlayerStateTypes.Neutral))
            if (_player.PlayerMove == Vector3.zero && _player.GravityManager.IsGrounded)
            {
                SwitchState(PlayerStateTypes.Neutral); 
            }
       
        if (transitionType.HasFlag(PlayerStateTypes.Walking))
            if (_player.PlayerMove.x != 0 && !_player.IsRunning)
                SwitchState(PlayerStateTypes.Walking);

        if (transitionType.HasFlag(PlayerStateTypes.Running))
            if (_player.IsRunning)
                SwitchState(PlayerStateTypes.Running);

        if (transitionType.HasFlag(PlayerStateTypes.Dash))
            if (_player.IsDashing && _player.GravityManager.IsGrounded && _player.InputReader.GetValidMoveInput() != InputReader.MovementInputResult.Forward)
                SwitchState(PlayerStateTypes.Dash);

        if (transitionType.HasFlag(PlayerStateTypes.Attack))
            if (_player.IsAttacking && !_player.OnAttackCoolDown)
                SwitchState(PlayerStateTypes.Attack);

        if (transitionType.HasFlag(PlayerStateTypes.CrouchMove))
            if (_player.IsCrouching && _player.PlayerMove.x != 0 && !_player.IsAttacking)
                SwitchState(PlayerStateTypes.CrouchMove);

        if (transitionType.HasFlag(PlayerStateTypes.Crouching))
            if (_player.PlayerMove.y < 0 && _player.GravityManager.IsGrounded)
                SwitchState(PlayerStateTypes.Crouching);
    }

    private void OnDestroy()
    {
    }

    public void SwitchToLastState()
    {
        currentState?.ExitState(this, _player);
        print(lastState);
        currentState = lastState;
        currentState?.EnterState(this, _player);
    }
}