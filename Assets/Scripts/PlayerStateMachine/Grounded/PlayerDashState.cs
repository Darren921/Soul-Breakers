using System.Collections;
using UnityEngine;
[System.Serializable]
public class PlayerDashState : PlayerMovingState
{
    [field: SerializeField] protected InputReader.MovementInputResult Dir;
    [field: SerializeField] protected  Vector3 DashDir;
    [field: SerializeField] protected Vector3 NewDashVelo;
    [field: SerializeField]  protected  float DashTime ;
    [field: SerializeField] protected  float DashDistance;
    [field: SerializeField]  protected bool IsDashing;
   private float _jumpVelocity;
     private Coroutine _dashCoroutine;

    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        Debug.Log("Entering Dash State");
        player.Animations. Animator?.SetTrigger(player.Animations.Dashing);
        GetDashValues(player);
        SetUpDash(player);
        player.StartCoroutine(Dash(player));
    }

    protected void GetDashValues(PlayerController player)
    {
        DashTime = player.CharacterData.dashTime;
        DashDistance = player.CharacterData.dashDistance;
        _jumpVelocity = player.CharacterData.dashVertHeight;
        
    }
    

    protected virtual void SetUpDash(PlayerController player)
    {
        Dir  = player.DashDir;
        Debug.Log(Dir);
        Debug.Log("PlayerDashState EnterState");

        if (Dir != InputReader.MovementInputResult.Backward)
        {
            Debug.Log(Dir);
            return;
        }
        DashDir =  !player.Reversed ? Vector3.left: Vector3.right;
      
        
        NewDashVelo = DashDir * (DashDistance / DashTime);
    }

    private IEnumerator Dash(PlayerController player)
    {
        Debug.Log("PlayerDashState Dash");
        IsDashing = true;
        player.rb.linearVelocity = new Vector3(NewDashVelo.x, _jumpVelocity, 0);
        Debug.Log(player.rb.linearVelocity);
        yield return new WaitForSeconds(DashTime);
        IsDashing = false;
      
    }

    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
    }

    protected override void ApplyVelocity(PlayerController player)
    {
    }


    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        //grab the last inputs given 
        if (IsDashing  || !player.GravityManager.IsGrounded) return;

    Debug.Log("HEH"); 
        playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.Attack | PlayerStateManager.PlayerStateTypes.Walking);

    }

    

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
        if (_dashCoroutine != null)
        {
            player.StopCoroutine(_dashCoroutine);
            _dashCoroutine = null;
        }
        player.IsDashing = false;
        player.Animations.Animator.ResetTrigger(player.Animations.Dashing);
        Debug.Log("PlayerDashState ExitState");
    }
}
