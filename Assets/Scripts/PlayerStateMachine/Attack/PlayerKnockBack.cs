using System;
using System.Collections;
using UnityEngine;

public class PlayerKnockBack : MonoBehaviour
{
    internal bool _isBeingKnockedBack;
    private const float KnockBackTime = 0.1f;
    private Vector3 _hitDirectionForce;
    internal PlayerController _playerController;
    private bool isOther;

    private Vector3 hitDir;
    private Vector3 hitForce;

   
    
    public IEnumerator KnockBackOtherPlayer(PlayerController player, bool b)
    {
        _playerController = player;
      
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
            _playerController.rb.MovePosition(!isOther ? _playerController.transform.position + _hitDirectionForce  * Time.fixedDeltaTime : _playerController.HitDetection.otherPlayer.transform.position + _hitDirectionForce  * Time.fixedDeltaTime);
        }

    }
    private Vector3 ReturnHitDir(PlayerController player)
    {
        // depending on the players direction return the direction 
        return !player.Reversed ? Vector3.right : Vector3.left;
    }

    public IEnumerator KnockBackThisPlayer(PlayerController player, bool b)
    {
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
    
    public void  SetOtherPlayer(PlayerController player)
    {
        
    }

    private Vector3 ReturnHitForce(PlayerController player, bool b)
    {
        Debug.Log(player.HitDetection.projectileData.Knockback + player.name);
        //Depending on the attack type return a knockback force value  (note mod this to add directional values later)
        if (b)
        {
           return  player.HitDetection.projectileData.Knockback;

        }

        return player.InputReader.CurAttackData.Knockback;

    }
}
