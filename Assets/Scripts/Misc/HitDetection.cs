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
    
    internal AttackData projectileData;
    internal bool Blocking;
    
    private Bounds _bounds;
    private void Awake()
    {
        _player = gameObject.GetComponentInParent<PlayerController>();
    }
    
    private void Update()
    {
        if (_player.AtBorder)
        {
            _player.rb.MovePosition(_player.transform.position +  new Vector3(!_player.Reversed ? _bounds.size.x : -_bounds.size.x, 0,0));
        }
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
          HandleHitDectection(false,other);
        }
        if (other.gameObject.CompareTag("Projectile"))
        {
            HandleHitDectection(true, other);
        }
        if (other.gameObject.CompareTag("Wall"))
        {
            _player.AtBorder = true;
            _bounds = other.bounds;
        }

      
    }

    private void HandleHitDectection(bool Projectile, Collider other )
    {
        _hit = true;
        Blocking = CheckBlocking(); 
        
        if(Projectile)   projectileData = other.GetComponent<PlayerProjectile>()._data;

        otherPlayer.hitBox.gameObject.SetActive(false);
//            print(Blocking);
        SwitchState(Blocking ? PlayerStateManager.PlayerStateTypes.Blocking : PlayerStateManager.PlayerStateTypes.HitStun);
        if (!_damageDone)
        {
            //  print("Damage taken");
            TakeDamage(!Projectile ?  otherPlayer.InputReader.CurrentAttackInput.Input : projectileData.Attack , Projectile);
        }
    }

    private void SwitchState(PlayerStateManager.PlayerStateTypes newState)
    {
        Debug.Log(otherPlayer.InputReader.CurrentAttackInput.Input.Type);
        if (otherPlayer.InputReader.CurrentAttackInput.Input.Type != InputReader.AttackType.Grab)
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
        
        if (_player._playerStateManager.currentState == _player._playerStateManager.States[PlayerStateManager.PlayerStateTypes.Walking] && _player.InputReader.CurrentMoveInput is InputReader.MovementInputResult.Backward or InputReader.MovementInputResult.DownLeft or InputReader.MovementInputResult.UpLeft 
            ||  _player._playerStateManager.currentState == _player._playerStateManager.States[PlayerStateManager.PlayerStateTypes.Crouching]  && _player.InputReader.CurrentMoveInput is InputReader.MovementInputResult.Backward or InputReader.MovementInputResult.DownLeft or InputReader.MovementInputResult.UpLeft  || _player._playerStateManager.currentState ==  _player._playerStateManager.States[PlayerStateManager.PlayerStateTypes.Jumping])
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


  
    public void TakeDamage(InputReader.Attack cachedAttack,bool isProjectile )
    {
        _damageDone = true;
        _player.Animations.Animator.SetBool(_player.Animations.Hit, true);
        if (!Blocking )
        {
         //   _player.Animations.Animator.Play("Hit", 0,0f );
            var otherPlayerSuperMeterCharge = otherPlayer.InputReader.CurAttackData.SuperAttackCharge;
            if (!isProjectile)
            {
                otherPlayer.Animations.DisableHitBox();
                
                otherPlayer.InputReader.currentAttackCached = new InputReader.BufferedInput<InputReader.Attack>(cachedAttack,Time.frameCount, false);
                otherPlayer.canCancel = true;
                
                otherPlayer.superMeter =   Mathf.Clamp(otherPlayer.superMeter += otherPlayerSuperMeterCharge, 0f , 300f );
            
            }
            else
            {
                otherPlayer.superMeter =    Mathf.Clamp(otherPlayer.superMeter += otherPlayerSuperMeterCharge, 0f , 300f );

            }
        }
        else
        {
            _player.Animations.Animator.Play("Blocking", 0,0 );
            _player.Animations.Animator.SetBool(_player.Animations.blocking, true);
        }
        var damage = otherPlayer.InputReader.CurAttackData.Damage; 
        var soundname =  _player.InputReader.ImpactSoundNames[cachedAttack.Type];
        SoundManager.instance?.PlayOneShot(SoundManager.instance.soundData.ReturnEventReference(SoundData.SoundType.SFX, soundname), transform.position);
        // deal damage and active death event to trigger end of game 
        _player.Health -=  Blocking ? damage * 0.25f : damage;
        OnPlayerHit?.Invoke();
        Debug.Log($"{projectileData.Knockback} knock away" );
        if (isProjectile)
        {
            otherPlayer.StartCoroutine(!_player.AtBorder ? otherPlayer.PlayerKnockBack.KnockBackOtherPlayer(_player, true) : _player.PlayerKnockBack.KnockBackThisPlayer(otherPlayer,true));

        }
        else
        {
            otherPlayer.StartCoroutine(!_player.AtBorder ? otherPlayer.PlayerKnockBack.KnockBackOtherPlayer(_player, false) : _player.PlayerKnockBack.KnockBackThisPlayer(otherPlayer,false));

        }
            
        if (_player.Health <= 0) OnDeath?.Invoke();
        
    }


  
}