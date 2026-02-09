using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class HitDetection : MonoBehaviour, IDamageable
{
    
    private PlayerController _player;
    [SerializeField] internal PlayerController otherPlayer;
    
    public static event Action OnDeath;
    public static event Action OnPlayerHit;
    internal bool _hit;
    internal  bool _damageDone;
    
    internal bool Blocking;

    private void Awake()
    {
        _player = gameObject.GetComponentInParent<PlayerController>();
    }

    private void CheckHit(Collider hitCollider)
    {
        
    }

   
    public void resetHit()
    {
        _hit = false;
        _damageDone = false;
    }
    private void OnTriggerStay(Collider other)
    {
     
        
        
        
        if (other.gameObject.CompareTag("HitBox") && otherPlayer.Animations.IsActiveFrame && other.gameObject.activeInHierarchy && !_hit  )
        {
//            print("Hit");
            _hit = true;
            Blocking = CheckBlocking();
            otherPlayer.hitBox.gameObject.SetActive(false);
//            print(Blocking);
            SwitchState(Blocking ? PlayerStateManager.PlayerStateTypes.Blocking : PlayerStateManager.PlayerStateTypes.HitStun);
            if (!_damageDone)
            {
              //  print("Damage taken");
                TakeDamage(otherPlayer.CharacterData.characterAttacks.ReturnAttackData(otherPlayer.InputReader.LastAttackInput,otherPlayer.InputReader.curState).Damage);
            }
        }

        
        
        if (other.gameObject.CompareTag("Wall"))
        {
            _player.AtBorder = true;
        }
    }

    private void SwitchState(PlayerStateManager.PlayerStateTypes newState)
    {
//        Debug.Log(otherPlayer.InputReader.LastAttackInput.Type);
        if (otherPlayer.InputReader.LastAttackInput.Type != InputReader.AttackType.Grab)
        {
            _player._playerStateManager.SwitchState(newState);
        }
        else
        {
            // This is temp and 
            print("Grabbed");
            otherPlayer.Animations.Animator.SetBool(_player.Animations.Grab, true);
            _player.Animations.Animator.SetBool(_player.Animations.Grabbed,true);
             _player._playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Grab);
             otherPlayer._playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Grab);

            // _player.PlayerHitDetection.TakeDamage(otherPlayer.CharacterData.characterAttacks.ReturnAttackData(otherPlayer.InputReader.LastAttackInput,otherPlayer.InputReader.curState).Damage);
        }
     
   
    }

    private bool CheckBlocking()
    {
        
        if (_player._playerStateManager.currentState == _player._playerStateManager.States[PlayerStateManager.PlayerStateTypes.Walking] ||  _player._playerStateManager.currentState == _player._playerStateManager.States[PlayerStateManager.PlayerStateTypes.Crouching] || _player._playerStateManager.currentState ==  _player._playerStateManager.States[PlayerStateManager.PlayerStateTypes.Jumping] 
            && _player.InputReader.CurrentMoveInput is InputReader.MovementInputResult.Backward or InputReader.MovementInputResult.DownLeft or InputReader.MovementInputResult.UpLeft)
        {
            switch (_player.InputReader.curState)
            {
                case AttackData.States.Standing:
                    if (otherPlayer.InputReader.curState != AttackData.States.Crouching) return true;
                    break;
                case AttackData.States.Crouching:
                    if(otherPlayer.InputReader.curState != AttackData.States.Airborne) return true;
                    break;
                case AttackData.States.Airborne:
                    return true;
                default:
                    return false;
            }
            
        }
//        print("Skippped");
        return false;
    }

    private void OnTriggerExit(Collider other)
    {
        _player.AtBorder = false;
    }

  
    public void TakeDamage(float damage)
    {
        _damageDone = true;
        _player.Animations.Animator.SetBool(_player.Animations.Hit, true);
        // deal damage and active death event to trigger end of game 
        _player.Health -=  Blocking ? damage * 0.25f : damage;
        OnPlayerHit?.Invoke();
        otherPlayer.StartCoroutine(!_player.AtBorder ? otherPlayer.PlayerKnockBack.KnockBackOtherPlayer(_player) : _player.PlayerKnockBack.KnockBackThisPlayer(otherPlayer));
        if (_player.Health <= 0) OnDeath?.Invoke();
        
    }

    
}