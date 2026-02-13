using System;
using UnityEngine;

public class DetectOtherPlayer : MonoBehaviour
{
    PlayerController player;
    BoxCollider BoxCollider;
    private RaycastHit Hit;
    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
        BoxCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        Debug.DrawRay(!player.Reversed ? new Vector3(BoxCollider.bounds.max.x, BoxCollider.bounds.center.y, BoxCollider.bounds.max.z) : new Vector3(BoxCollider.bounds.min.x, BoxCollider.bounds.center.y, BoxCollider.bounds.center.z), (BoxCollider.transform.right) * 4 , Color.red);
        
            if(Physics.Raycast(!player.Reversed ? new Vector3(BoxCollider.bounds.max.x, BoxCollider.bounds.center.y, BoxCollider.bounds.max.z) : new Vector3(BoxCollider.bounds.min.x, BoxCollider.bounds.center.y, BoxCollider.bounds.center.z)
                   ,  BoxCollider.transform.right , out  Hit, 0.75f, LayerMask.GetMask("PushBox"),QueryTriggerInteraction.Collide ) && !player.JumpPressed)
            {
           
//                Debug.Log($" Rounding hit = {Mathf.Round((Hit.point - transform.position).normalized.x)}, Raw hit {(Hit.point - transform.position).normalized.x} player move {(!player.Reversed ? player.PlayerMove.x : -player.PlayerMove.x)}");
                player.PlayersColliding =  Mathf.Approximately(player.PlayerMove.x  , Mathf.Round( (Hit.point - transform.position).normalized.x)) ;
            }
            else
            {
                Debug.Log("Colliding false ");
                player.PlayersColliding = false;
            }
    }

    private void FixedUpdate()
    {
        if (player.PlayersColliding && !player.JumpPressed)
        {
            player.rb.MovePosition(player.transform.position + (!player.Reversed ? new Vector3(0.3f, 0, 0): new Vector3(-0.3f,0,0)) * Time.fixedDeltaTime);
           player.PlayerHitDetection.otherPlayer.rb.MovePosition(player.PlayerHitDetection.otherPlayer.transform.position + (!player.Reversed ? new Vector3(0.3f, 0, 0): new Vector3(-0.3f,0,0)) * Time.fixedDeltaTime);
        }
        // if (!player.GravityManager.IsGrounded && !player.HitStun)
        // {
        //     player.GravityManager.ApplyGravity(player);
        //     
        //     player.rb.MovePosition(player.transform.position + new Vector3(player.PlayerMove.x, player.GravityManager.GetVelocity() , 0f) * Time.fixedDeltaTime );
        //     //  player.rb.linearVelocity  = new Vector3(player.rb.linearVelocity.x,player.GravityManager.GetVelocity() ,0);
        // }
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PushBox"))
        {
            player.SetFrictionBox(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        player.SetFrictionBox(false);
    }
}
