using System.Collections;
using UnityEngine;
[System.Serializable]
public class PlayerAirDashState : PlayerDashState
{
    [field: SerializeField] private int _airDashCharges;

    internal override void EnterState(PlayerStateManager playerStateManager, PlayerController player)
    {
        _airDashCharges = player.CharacterData.airDashCharges;
        _airDashCharges--;
        player.StartCoroutine(AirDash(player));
        SetUpDash(player);
//      Debug.Log(newDashVelo);
    }

    protected override void SetUpDash(PlayerController player)
    {
        Dir = player.DashDir;
        //   Debug.Log(dir);
        //    Debug.Log("PlayerDashState EnterState");

        DashDir = Dir switch
        {
            InputReader.MovementInputResult.Forward or InputReader.MovementInputResult.None or InputReader.MovementInputResult.Up => !player.Reversed ? Vector3.right : Vector3.left,
            InputReader.MovementInputResult.Backward => !player.Reversed ? Vector3.left : Vector3.right,
            InputReader.MovementInputResult.UpLeft => Vector3.left,
            InputReader.MovementInputResult.UpRight => Vector3.right,
            _ => DashDir
        };

        if (!player.Reversed)
        {
            player.Animations.Animator.SetFloat(player.Animations.DashDir,DashDir == Vector3.left  ? 0 : 1 );  
        }
        else
        {
            player.Animations.Animator.SetFloat(player.Animations.DashDir,DashDir == Vector3.left  ? 1 : 0);
        }
     
        
        
        player. Animations.Animator?.SetTrigger(player.Animations.AirDashing);
        GetDashValues(player);
        
        // Debug.Log(DashDir);
            NewDashVelo = DashDir *   (DashDistance / DashTime);
       
     
    }
    
    private IEnumerator AirDash(PlayerController player)
    {
        player.GravityManager.ResetVelocity();
        //    Debug.Log("PlayerDashState Dash");
        IsDashing = true;
        // player.rb.useGravity = false;
        // player.rb.linearVelocity = new Vector3(NewDashVelo.x, 0, 0);
        yield return new WaitForSeconds(DashTime);
        IsDashing = false;
    }

    internal override void UpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
        //      if(!player.isGrounded ||   isDashing || player.Dashing) return;

        if (_airDashCharges > 0 && player.IsDashing && !IsDashing && player.AtDashHeight)
        {
            //  Debug.Log("PlayerDashState Dash again");
            _airDashCharges--;
            player.GravityManager.ResetVelocity();
            player.StartCoroutine(AirDash(player));
        }

        if (player.GravityManager.IsGrounded)
        {
            playerStateManager.CheckForTransition(PlayerStateManager.PlayerStateTypes.Neutral | PlayerStateManager.PlayerStateTypes.Walking);
        }
        // Debug.Log(player.GravityManager.GetVelocity());
    }

    internal override void FixedUpdateState(PlayerStateManager playerStateManager, PlayerController player)
    {
//        Debug.Log(player.gravityManager.GetVelocity());

        if (player.IsDashing)
        {
            player.rb.MovePosition(player.transform.position + new Vector3(NewDashVelo.x * 1.3f,0,0) * Time.fixedDeltaTime);
//            Debug.Log( player.rb.linearVelocity);

        }
        if (!player.GravityManager.IsGrounded && !IsDashing)
        {
            player.GravityManager.ApplyGravity(player);
            player.rb.MovePosition(player.transform.position + new Vector3(player.rb.linearVelocity.x, player.GravityManager.GetVelocity(),0) * Time.fixedDeltaTime);
           // player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, player.GravityManager.GetVelocity(), 0);
        }
    }

    internal override void ExitState(PlayerStateManager playerStateManager, PlayerController player)
    {
        player.IsDashing = false;
        player.Animations.ResetAttackingTrigger();
        player.Animations.Animator.ResetTrigger(player.Animations.Dashing);
    }
}