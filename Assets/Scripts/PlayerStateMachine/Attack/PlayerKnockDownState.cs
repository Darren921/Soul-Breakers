using System.Collections;
using UnityEngine;

public class PlayerKnockDownState : PlayerBaseState
{
    bool getUp ;
    bool complete ;
    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
       player.OnDisablePlayer();
       player.StartCoroutine(GroundAndGetUp(player));
    }

    private IEnumerator GroundAndGetUp(PlayerController player)
    {
        player.Animations.Animator.Play("knockDown");
        yield return new WaitUntil(() =>player.Animations.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        player.Animations.Animator.enabled = false;
        getUp = true;
        player.Animations.Animator.enabled = true;
        player.Animations.Animator.Play("getUp");
        yield return new WaitUntil(() =>player.Animations.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
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
        player.OnEnablePlayer();
    }
}
