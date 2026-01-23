using System;
using Unity.VisualScripting;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    private float Velocity { get; set; }
    private RaycastHit Hit;
    private LayerMask _groundLayerMask;
    private PlayerController _player;
    [SerializeField] internal bool IsGrounded;

    private void Awake()
    {
        _groundLayerMask = LayerMask.GetMask("Ground");
        _player = GetComponentInParent<PlayerController>();
    }


    private void OnTriggerStay(Collider other)
    {
        print(other.gameObject.CompareTag("Ground"));
        IsGrounded = other.gameObject.tag == "Ground";
        Debug.Log(other.gameObject.tag);
        if (!other.gameObject.CompareTag("Player")) return;
        _player.SetFrictionBox(true);
    }

    private void OnTriggerExit(Collider other)
    {
        IsGrounded = false;
        _player.SetFrictionBox(false);
    }
    // private void OnCollisionStay(Collision collision)
    // {
    //     print(collision.gameObject.CompareTag("Ground"));
    //     IsGrounded = collision.gameObject.tag == "Ground";
    //     Debug.Log(collision.gameObject.tag);
    //     if (!collision.gameObject.CompareTag("Player")) return; 
    //     _player.SetFrictionBox(true);
    // }


    // private void OnCollisionExit(Collision other)
    // {
    //     IsGrounded = false;
    //     _player.SetFrictionBox(false);
    // }

    public bool CheckGrounded(PlayerController player)
    {
        //checks if the player is grounded and updates the related bool 
        var grounded = Physics.Raycast(player.raycastPos.position, -player.transform.up, out Hit,
            player.RaycastDistance, _groundLayerMask);

        Debug.Log("CheckGrounded");
        Debug.DrawRay(player.raycastPos.position, -player.transform.up * player.RaycastDistance, Color.red);
        ;
        return grounded;
    }

    public void ApplyGravity(PlayerController player)
    {
        //Applies the custom gravity based on grav scale
        if (Velocity > 0.1f) Velocity += Physics.gravity.y * player.GravScale * Time.fixedDeltaTime;
        if (Velocity < 0.1f)
            Velocity += Physics.gravity.y * player.CharacterData.FallingGravScale * Time.fixedDeltaTime;
    }

    public float GetVelocity()
    {
        //gets the current velocity value 
        return Velocity;
    }

    public void ResetVelocity()
    {
        //resets the gravity's velocity 
        Velocity = 0;
    }

    public float SetJumpVelocity(PlayerController player)
    {
        //uses formula in order to get a constant jump height 
        var targetVelocity = !player.SuperJumpActive
            ? Mathf.Sqrt(player.JumpHeight * -2 * (Physics.gravity.y * player.GravScale))
            : Mathf.Sqrt((player.JumpHeight * 2) * -2 * (Physics.gravity.y * player.GravScale));
        return Velocity = targetVelocity;
    }
}