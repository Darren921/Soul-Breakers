using System;
using System.Collections;
using UnityEngine;

public class PlayerKnockBack : MonoBehaviour
{
    internal bool _isBeingKnockedBack;
    private const float KnockBackTime = 0.1f;
    private Vector3 _hitDirectionForce;
    private PlayerController _playerController;
    private bool isOther;

    private void Start()
    {
    }

    public IEnumerator KnockBackOtherPlayer(PlayerController player)
    {
        _playerController = player;
        var hitDir = ReturnHitDir(player.PlayerHitDetection.otherPlayer);
         var hitForce = ReturnHitForce(player.PlayerHitDetection.otherPlayer);
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
            _playerController.rb.MovePosition(!isOther ? _playerController.transform.position + _hitDirectionForce  * Time.fixedDeltaTime : _playerController.PlayerHitDetection.otherPlayer.transform.position + _hitDirectionForce  * Time.fixedDeltaTime);
        }

    }
    private Vector3 ReturnHitDir(PlayerController player)
    {
        // depending on the players direction return the direction 
        return !player.Reversed ? Vector3.right : Vector3.left;
    }

    public IEnumerator KnockBackThisPlayer(PlayerController player)
    {
        isOther = true;
        _isBeingKnockedBack = true;
        //Use this to knock back the attacking player 
        var hitDir = ReturnHitDir(player.PlayerHitDetection.otherPlayer);
        var hitForce = ReturnHitForce(player);
        _hitDirectionForce = new Vector3(hitDir.x * hitForce.x, hitForce.y, 0);
        yield return new WaitForSeconds(KnockBackTime);
        _isBeingKnockedBack = false;
        isOther = false;
    }


    private Vector3 ReturnHitForce(PlayerController player)
    {
        //Depending on the attack type return a knockback force value  (note mod this to add directional values later)
        var hitForceTemp = player.CharacterData.characterAttacks.ReturnAttackData(player.InputReader.LastAttackInput,player.InputReader.curState).Knockback;
//        print(hitForceTemp);
        return hitForceTemp;
    }
}
