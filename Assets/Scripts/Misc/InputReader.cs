using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class InputReader : MonoBehaviour
{
    #region AttackEnums
    [Flags]
    public enum AttackType
    {
        None = 0,
        Light = 1 << 0,
        Medium = 1 << 1,
        Heavy = 1 << 2,
        Special = 1 << 3,
        Grab = Light | Medium ,
        SuperMedium = Medium | Special,
        SuperHeavy = Heavy | Special,
    }

    private static readonly Dictionary<AttackType, int> Attackpriority = new()
    {
        [AttackType.SuperHeavy] = 6,
        [AttackType.SuperMedium] = 6,
        [AttackType.Grab] = 5,
        [AttackType.Special] = 4, 
        [AttackType.Heavy] = 3,      
        [AttackType.Medium] = 2,      
        [AttackType.Light] = 1, 
        [AttackType.None] = -1,

       
    };


    #endregion
    #region Input Structs

    [Serializable]
    public struct BufferedInput<T>
    {
        public T Input;
        public readonly int CurFrame;
        public  bool IsBeingUsed; 
    

        public BufferedInput(T input, int curFrame, bool isBeingUsed)
        {
            Input = input;
            CurFrame = curFrame;
            IsBeingUsed = isBeingUsed;
        }
    }
    [Serializable]
    public struct Attack : IEquatable<Attack>
    {
        public MovementInputResult Move;
        public AttackType Type;
        public int Priority => Attackpriority.GetValueOrDefault(Type, -1);

        public Attack(AttackType type = AttackType.None ,MovementInputResult move = MovementInputResult.None)
        {
            Type = type;
            Move = move;
//            Debug.Log(Priority);
        }

        public override string ToString()
        {
            var move = Move != MovementInputResult.None ? $"{Move.ToString()} " : ""; 
            var fullMove = string.Concat(move, Type);
            return fullMove;
        }

        public bool Equals(Attack other)
        {
            return Move == other.Move && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            return obj is Attack other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Move, (int)Type);
        }
    }

  

    #endregion
    public enum MovementInputResult
    {
        None,
        Up,
        Down,
        Forward,
        Backward,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
    }
    
    private PlayerController player;
    public MovementInputResult CurrentMoveInput { get; private set; }
    public BufferedInput<Attack> CurrentAttackInput;
    public AttackData  SpecialData; 
    public int CurrentAttackFrame { get; private set; }

    private AttackData curAttackData;
    public AttackData CurAttackData => curAttackData;
    private int LastAttackInputFrame { get; set; }
    internal BufferedInput<Attack> currentAttackCached;

    public AttackData.States curState { get; private set; } 
  
    private readonly List<BufferedInput<MovementInputResult>> _movementBuffer = new();
    private readonly List<BufferedInput<Attack>> _attackBuffer = new();
    
     [SerializeField] internal List<string> movementInputsVisual = new();
     [SerializeField] internal List<string> attackInputsVisual = new();

    private int _bufferCap;
    private int _bufferTime;

    public bool superPerformed;
   // private AttackType _currentFrameAttackInputs; 


   public Dictionary<AttackType, string> ImpactSoundNames = new()
   {
       { AttackType.Light, "lightimpact" },
       { AttackType.Medium, "mediumimpact" },
       { AttackType.Heavy, "heavyimpact" },
       { AttackType.Special, "heavyimpact" },
       { AttackType.SuperHeavy, "heavyimpact" },
       { AttackType.SuperMedium, "mediumimpact" },

   };
    private void AddMovementInput(MovementInputResult result)
    {
        if (_movementBuffer.Count >= _bufferCap)
            _movementBuffer.RemoveAt(0);
        _movementBuffer.Add(new BufferedInput<MovementInputResult>(result, Time.frameCount,false));
    }

    private void AddAttackInput(AttackType type)
    {
        if(type == AttackType.None) return;
        var input = ReturnAttack(type, CurrentMoveInput);

        if (_attackBuffer.Count >= _bufferCap)
            _attackBuffer.RemoveAt(0);

        _attackBuffer.Add(new BufferedInput<Attack>(input, Time.frameCount,false));
    }

    private bool CancelCheck(PlayerController player)
    {
        // if (player.InputReader.currentAttackCached.Input.Priority != -1 &&
        //     player.InputReader.CurrentAttackInput.Input.Priority != -1)
        // {
        //   Debug.Log($"Cancel check {player.canCancel} and input priority = {player.InputReader.LastAttackInput.Priority} vs last hit {player.InputReader.currentAttackCached.Input.Priority} and cur input != none {player.InputReader.currentAttackCached.Input.Priority != -1}  ");
        // }
        if (player.canCancel && player.InputReader.curAttackData.Attack.Priority > player.InputReader.currentAttackCached.Input.Priority  &&  player.InputReader.currentAttackCached.Input.Priority != -1 &&  player.InputReader.CurrentAttackInput.Input.Priority != -1)
        {
            return true;
        }
        return false;
    }

    private AttackData.States CheckState(PlayerBaseState lastState)
    {
       // Debug.Log(lastState);
        var state = player._playerStateManager.AirborneStates.Contains(lastState) ? AttackData.States.Airborne :
            player._playerStateManager.StandingStates.Contains(lastState) ? AttackData.States.Standing :
            player._playerStateManager.CrouchingStates.Contains(lastState) ? AttackData.States.Crouching : curState;
      //  Debug.Log(state.ToString());
        return state;

    }


    private void Awake()
    {
        player = GetComponent<PlayerController>();
        _bufferTime = 5;
        _bufferCap = 10;
        player.PlayerAttackAction += AddAttackInput;
    }

    private void OnDestroy()
    {
        player.PlayerAttackAction -= AddAttackInput;
    }

    private void Update()
    {
        if (PauseManager.Instance && PauseManager.Instance.IsPaused)
            return;
        curState = CheckState(player._playerStateManager.currentState);
        CheckMovementInput();
        UpdateInputBuffers();
        
  
        if (CancelCheck(player) && !player.Animations.IsActiveFrame && !player.Animations.CancelActive )
        {
            player.Animations.CancelActive = true;
            Debug.Log("Cancelling Active");
        }
    }

    public BufferedInput<Attack> GetBufferedAttack()
    {
        var curFrame = Time.frameCount;
        var newAttack = new BufferedInput<Attack> ();

        
        
        
        for (var i = _attackBuffer.Count - 1; i >= 0; i--)
        {
            var input = _attackBuffer[i];

            if (_attackBuffer[^1].CurFrame - input.CurFrame > 5)
                break;
            newAttack.Input.Type |= input.Input.Type;
            newAttack =  CheckForSuper(newAttack);
            if (  newAttack.Input.Type  != AttackType.None)   newAttack.Input.Type  = GetAttackPriority( newAttack.Input.Type );
            if (newAttack.Input.Move == MovementInputResult.None) newAttack.Input.Move = input.Input.Move;
            newAttack.IsBeingUsed = true;
            _attackBuffer.RemoveAt(i);
        }
        if (newAttack.Input.Type != AttackType.None)
        {
            LastAttackInputFrame = curFrame;
            if (newAttack.Input.Type is AttackType.Special )
            {
                SpecialData.Attack = newAttack.Input;
            }
        }
        Debug.Log(newAttack.Input);
        return newAttack;
    }

    private BufferedInput<Attack> CheckForSuper(BufferedInput<Attack> newAttack)
    {
//        Debug.Log(newAttack.Input.ToString());
        if (!newAttack.Input.Type.ToString().Contains("Super") && newAttack.Input.Type != AttackType.None) return newAttack;
         Debug.Log("Checking");
         Debug.Log(player.superMeter);
         curAttackData = player.CharacterData.characterAttacks.ReturnAttackData(newAttack.Input, player.InputReader.curState);
        var superChargeNeeded = curAttackData.SuperChargeNeeded;
        if (superChargeNeeded == 0) superChargeNeeded = curAttackData.SuperChargeNeeded;
        Debug.Log(superChargeNeeded);

       
        if (player.superMeter >= superChargeNeeded )
        {
            print("Super triggered");
            player.superMeter -= superChargeNeeded;
            superPerformed = true;
            player.canCancel = false;
            GameManager.OnRefresh?.Invoke();
       //     player.Animations.Animator.SetBool(player.Animations.Super, true);
          
        }
        else
        {
            var flagToRemove = newAttack.Input.Type;
            flagToRemove = ~ AttackType.Special;
            newAttack.Input.Type =  ~ flagToRemove;
            print( newAttack.Input.Type );
        }
        return newAttack;
    }

    public AttackType GetAttackPriority(AttackType type)
    {
        var activeFlags = Enum.GetValues(typeof(AttackType)).Cast<AttackType>().Where(attackType => attackType != AttackType.None && (type & attackType) == attackType);
        
        var priorityAttack = -2000;
        var output = AttackType.None;
        
        foreach (var flag in activeFlags)
        {
//            Debug.Log(flag.ToString());
            if (Attackpriority.TryGetValue(flag, out var priority) &&  priority > priorityAttack)
            {
                priorityAttack = priority;
                output = flag;
            }
        }
        if (output == AttackType.None)
        {
            Debug.LogError("Attack priority unknown");
        }

       // _player.Animations.SetAttackingHash(output);
//        Debug.Log(output.ToString());
        return output;
        
    } 
    private void UpdateInputBuffers()
    {
        var curFrame = Time.frameCount;
        _movementBuffer.RemoveAll(i => curFrame - i.CurFrame > _bufferTime && !i.IsBeingUsed);
        _attackBuffer.RemoveAll(i => curFrame - i.CurFrame > _bufferTime && !i.IsBeingUsed);

        CurrentMoveInput = _movementBuffer.Count > 0 ? _movementBuffer[^1].Input : MovementInputResult.None;
        if (CurrentAttackInput.Input.Type == AttackType.None && _attackBuffer.Count > 0)
        {
            CurrentAttackInput = GetBufferedAttack();
            CurrentAttackFrame = curFrame;
        }        
     
        movementInputsVisual.Clear();
        foreach (var input in _movementBuffer)
        {
            movementInputsVisual.Add($"{input.Input} (F{input.CurFrame})");
        }
        
        // attackInputsVisual.Clear();
        // foreach (var input in _attackBuffer)
        // {
        //     attackInputsVisual.Add($"{input.Input.ToString()} (F{input.CurFrame})");
        // }
    }



    private void CheckMovementInput()
    {
        //checking the movement inputted 
        var lookup = new Dictionary<(float, float), MovementInputResult>
        {
            [(0, 0)] = MovementInputResult.None,
            [(0, 1)] = MovementInputResult.Up,
            [(0, -1)] = MovementInputResult.Down,
            [(1, 0)] = !player.Reversed ? MovementInputResult.Forward : MovementInputResult.Backward,
            [(-1, 0)] = !player.Reversed ? MovementInputResult.Backward : MovementInputResult.Forward,
            [(1, 1)] = MovementInputResult.UpRight,
            [(-1, 1)] = MovementInputResult.UpLeft,
            [(1, -1)] = MovementInputResult.DownRight,
            [(-1, -1)] = MovementInputResult.DownLeft
        };
        AddMovementInput(lookup[(player.PlayerMove.x, player.PlayerMove.y)]);
    }

    private Attack ReturnAttack(AttackType attackType, MovementInputResult movementInput)
    {
        var attack = new Attack(attackType, movementInput);
//        Debug.Log("Attack");
        return attack;

    }

    public MovementInputResult GetValidMoveInput()
    {
        if (CurrentMoveInput != MovementInputResult.None) return CurrentMoveInput;
        var validInput = _movementBuffer.FindLast(i => i.Input != MovementInputResult.None);
        CurrentMoveInput = validInput.Input;
//      print(currentMoveInput);
        return validInput.Input;

    }
    public IEnumerator holdCurrentInput()
    {
        CurrentAttackInput.IsBeingUsed = true;
        curAttackData = player.CharacterData.characterAttacks.ReturnAttackData(player.InputReader.CurrentAttackInput.Input, player.InputReader.curState);
        yield return new WaitUntil(() => !player.IsAttacking );
        CurrentAttackInput = new BufferedInput<Attack>();
        
    }

}