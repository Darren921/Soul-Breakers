using System;
using System.Collections;
using UnityEngine;

public class PlayerKnockBack : MonoBehaviour
{
    internal bool _isBeingKnockedBack;
    private const float KnockBackTime = 0.1f;
    private Vector3 _hitDirectionForce;
    internal PlayerController _player;
    private bool isOther;

    private Vector3 hitDir;
    private Vector3 hitForce;




    public IEnumerator KnockBackHitPlayer(PlayerController player, bool b)
    {
//      Debug.Log(player.gameObject.name + " is being knocked back");
      _player = player;
        hitForce = !b ? ReturnHitForce(player.HitDetection.otherPlayer,b) : ReturnHitForce(player, b);
        hitDir = ReturnHitDir(player.HitDetection.otherPlayer);

        _hitDirectionForce = new Vector3(hitDir.x * hitForce.x, hitForce.y, 0);
        //Use this to knock back the other player 
        _isBeingKnockedBack = true;
        yield return new WaitForSeconds(KnockBackTime);
        _isBeingKnockedBack = false;
    }

    private void FixedUpdate()
    {
        if (_isBeingKnockedBack)
        {
            if (!isOther)
            {
        //        Debug.Log(_player.name + " is being knocked back");
                _player.rb.MovePosition( _player.transform.position + _hitDirectionForce * Time.fixedDeltaTime);
            }
            else
            {
    //            if(!_player.HitDetection.otherPlayer)return;
                Debug.Log(_player.name + " is being knocked back");
                _player.rb.MovePosition( _player.HitDetection.otherPlayer.transform.position + _hitDirectionForce * Time.fixedDeltaTime);
            }
        }

    }
    private Vector3 ReturnHitDir(PlayerController player)
    {
        // depending on the players direction return the direction 
        return !player.Reversed ? Vector3.right : Vector3.left;
    }

    public IEnumerator KnockBackAttackingPlayer(PlayerController player, bool b)
    {
   //     Debug.Log("KnockBackAttackingPlayer");
        isOther = true;
        _isBeingKnockedBack = true;
        //Use this to knock back the attacking player 
        var hitDir = ReturnHitDir(player.HitDetection.otherPlayer);
        var hitForce = ReturnHitForce(player,b);
        _hitDirectionForce = new Vector3(hitDir.x * hitForce.x, hitForce.y, 0);
        yield return new WaitForSeconds(KnockBackTime);
        _isBeingKnockedBack = false;
        isOther = false;
    }

    public IEnumerator KnockBackCurrentPlayer(bool reversed,Vector3 Force,float time  )
    {
        _player = GetComponent<PlayerController>();

 //       Debug.Log("KnockBackCurrentPlayer");
        isOther = false;
        _isBeingKnockedBack = true;
        var dir = !reversed ? Vector3.right : Vector3.left;
        _hitDirectionForce = new Vector3(dir.x * Force.x, dir.y * Force.y, 0);
        yield return new WaitForSeconds(time);
        _isBeingKnockedBack = false;
    }

    private Vector3 ReturnHitForce(PlayerController player, bool b)
    {
//        Debug.Log(player.HitDetection.projectileData.Knockback + player.name);
        //Depending on the attack type return a knockback force value  (note mod this to add directional values later)
        if (b)
        {
           return  player.HitDetection.projectileData.Knockback;

        }

        return player.InputReader.CurAttackData.Knockback;

    }
}
