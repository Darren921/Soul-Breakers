using System.Collections;
using UnityEngine;

public class PlayerKnockDownState : PlayerBaseState
{
    bool complete ;
    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
       player.OnDisablePlayer();
       player.StartCoroutine(GroundAndGetUp(player));
    }

    private IEnumerator GroundAndGetUp(PlayerController player)
    {
        player.Animations.Animator.Play("knockDown",0,0f);
        yield return new WaitUntil(() =>player.Animations.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        player.Animations.Animator.Play("getUp",0,0f);
        yield return new WaitUntil(() =>player.Animations.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        Debug.Log("getUp");
        complete = true;;

    }

    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (complete)
        {
            playerStateManager.SwitchState(PlayerStateManager.PlayerStateTypes.Neutral);
        }
    }

    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        
    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
        complete = false;
        player.OnEnablePlayer();


    }
}
