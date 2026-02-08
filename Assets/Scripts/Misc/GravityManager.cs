using System;
using Unity.VisualScripting;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    private float Velocity { get; set; }
    private RaycastHit Hit;
    private LayerMask _groundLayerMask;
    private PlayerController _player;
    [SerializeField] Collider _playerCollider;  
    [SerializeField] internal bool IsGrounded;
    private Vector3 groundPoint;
    private void Awake()
    {
        _groundLayerMask = LayerMask.GetMask("Ground");
        _player = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
     
    }

 
    /*
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            if (Velocity < 0) ResetVelocity();
        }           
        IsGrounded = other.gameObject.CompareTag("Ground");

    }

    private void OnCollisionExit(Collision other)
    {
        IsGrounded = false; 
        print("reset");
        _player.SetFrictionBox(false);    }
        */


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            IsGrounded = true;
            ResetVelocity();
        }
    }

    private void OnTriggerStay(Collider other)  
    {
//        print(other.gameObject.CompareTag("Ground"));
        if (other.gameObject.CompareTag("Ground"))
        {
            if (Velocity <= 0) ResetVelocity();
        }           
        IsGrounded = other.gameObject.CompareTag("Ground");

        
//        Debug.Log(other.gameObject.tag);
    }

    private void OnTriggerExit(Collider other)
    {
        IsGrounded = false;
//        print("reset");
        _player.SetFrictionBox(false);
    }
    public void ApplyGravity(PlayerController player)
    {
        //Applies the custom gravity based on grav scale
        if(player.PlayerKnockBack._isBeingKnockedBack) return;
        if (Velocity > 0.1f) Velocity += Physics.gravity.y * player.GravScale * Time.fixedDeltaTime;
        if (Velocity < 0.1f) Velocity += Physics.gravity.y * player.CharacterData.FallingGravScale * Time.fixedDeltaTime;
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