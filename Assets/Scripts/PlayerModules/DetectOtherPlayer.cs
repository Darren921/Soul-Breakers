using System;
using UnityEngine;

public class DetectOtherPlayer : MonoBehaviour
{
    private bool curPlayerMoving;
    private bool targetPlayerMoving;
    PlayerController player;
    BoxCollider BoxCollider;
    private RaycastHit Hit;
    private float PushForce;
    internal bool intersecting;
    private Bounds intersectionBounds;
    private BoxCollider otherPlayersCollider;
    private float rayDistance;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        BoxCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        rayDistance = 0.3f;
    }

    private void Update()
    {
        Debug.DrawRay(
            !player.Reversed
                ? new Vector3(BoxCollider.bounds.max.x, BoxCollider.bounds.center.y, BoxCollider.bounds.center.z)
                : new Vector3(BoxCollider.bounds.min.x, BoxCollider.bounds.center.y, BoxCollider.bounds.center.z),
            !player.Reversed ? Vector3.right * rayDistance : Vector3.left * rayDistance, Color.red);
        //    Debug.DrawRay(player.Reversed ? new Vector3(BoxCollider.bounds.center.x, BoxCollider.bounds.center.y, BoxCollider.bounds.center.z) : new Vector3(BoxCollider.bounds.min.x, BoxCollider.bounds.center.y, BoxCollider.bounds.center.z), -BoxCollider.transform.right * rayDistance, Color.black);
    }

    private void FixedUpdate()
    {
        if (Physics.Raycast(!player.Reversed
                    ? new Vector3(BoxCollider.bounds.max.x, BoxCollider.bounds.center.y, BoxCollider.bounds.center.z)
                    : new Vector3(BoxCollider.bounds.min.x, BoxCollider.bounds.center.y, BoxCollider.bounds.center.z)
                , !player.Reversed ? Vector3.right : Vector3.left, out Hit, rayDistance, LayerMask.GetMask("PushBox"),
                QueryTriggerInteraction.Collide))
        {
//                Debug.Log($" Rounding hit = {Mathf.Round((Hit.point - transform.position).normalized.x)}, Raw hit {(Hit.point - transform.position).normalized.x} player move {(!player.Reversed ? player.PlayerMove.x : -player.PlayerMove.x)}");
            if (Hit.collider != BoxCollider)
            {
                player.PlayersColliding = Mathf.Approximately(player.PlayerMove.x,
                    Mathf.Round((Hit.point - transform.position).normalized.x));
            }
        }
        else
        {
            player.PlayersColliding = false;
        }


        if (player.IsAttacking) return;
        CheckCollision(player.HitDetection.otherPlayer);
        PushPlayer(player.HitDetection.otherPlayer);
    }

    private void PushPlayer(PlayerController otherPlayer)
    {
        otherPlayersCollider = player.HitDetection.otherPlayer.GetComponentInChildren<DetectOtherPlayer>().BoxCollider;

        
        if (player.PlayersColliding && player.PlayerMove.y == 0 && otherPlayer.PlayerMove.y == 0 && !intersecting)
        {
           Debug.Log("Pushing player via raycast");
            player.rb.MovePosition(player.transform.position + (!player.Reversed ? Vector3.right * PushForce : Vector3.left * PushForce) * Time.fixedDeltaTime);
            otherPlayer.rb.MovePosition(otherPlayer.transform.position + (!player.Reversed ? Vector3.right * PushForce : Vector3.left * PushForce) * Time.fixedDeltaTime);
        }

        else if (player._detector.intersecting && !player.GravityManager.IsGrounded && !otherPlayer.GravityManager.IsGrounded)
        {
            Debug.Log("Airborne collision");
            player.rb.MovePosition(player.transform.position + (!player.Reversed ? new Vector3(-1 * PushForce,player.GravityManager.GetVelocity()) : new Vector3(1 * PushForce,player.GravityManager.GetVelocity())) * Time.fixedDeltaTime);
            otherPlayer.rb.MovePosition(otherPlayer.transform.position +
                                        (!player.Reversed
                                            ? new Vector3(1 * PushForce, player.GravityManager.GetVelocity())
                                            : new Vector3(-1 * PushForce, player.GravityManager.GetVelocity())) *
                                        Time.fixedDeltaTime);
        }
        
        else if (intersecting && !otherPlayer.GravityManager.IsGrounded && player.GravityManager.IsGrounded || intersecting && otherPlayer.GravityManager.IsGrounded && !player.GravityManager.IsGrounded)
        {
          Debug.Log("one player moving && intersecting");
            intersectionBounds.size = new Vector3(intersectionBounds.size.x + 0.2f, 0, 0);
            player.rb.MovePosition(player.transform.position +
                                   (!player.Reversed ? -intersectionBounds.size : intersectionBounds.size));
            otherPlayer.rb.MovePosition(otherPlayer.transform.position +
                                        (!otherPlayer.Reversed ? -intersectionBounds.size : intersectionBounds.size));
        }
        else if (intersecting && !targetPlayerMoving && !curPlayerMoving && player.GravityManager.IsGrounded && otherPlayer.GravityManager.IsGrounded)
        {
            Debug.Log("no player moving && intersecting");
            intersectionBounds.size = new Vector3(intersectionBounds.size.x + 0.1f, 0, 0);
            player.rb.MovePosition(player.transform.position + (!player.Reversed ? -intersectionBounds.size : intersectionBounds.size));
            otherPlayer.rb.MovePosition(otherPlayer.transform.position + (!otherPlayer.Reversed ? -intersectionBounds.size : intersectionBounds.size));
        }
    }

    private void CheckCollision(PlayerController OtherPlayer)
    {
        otherPlayersCollider = player.HitDetection.otherPlayer.GetComponentInChildren<DetectOtherPlayer>().BoxCollider;
        curPlayerMoving = player.PlayerMove.magnitude > 0 && !player.IsCrouching;
        targetPlayerMoving = OtherPlayer.PlayerMove.magnitude > 0 && !player.IsCrouching;

        if (player.AtBorder || OtherPlayer.AtBorder)
        {
            PushForce = 0;
            return;
        }
        PushForce = curPlayerMoving switch
        {
            true when targetPlayerMoving => 0.5f,
            _ => PushForce
        };
      
        if (!curPlayerMoving && targetPlayerMoving || curPlayerMoving && !targetPlayerMoving )
        {
            PushForce = 0.75f;
        }

     

        if (BoxCollider.bounds.Intersects(otherPlayersCollider.bounds))
        {
            intersecting = true;
            intersectionBounds.SetMinMax(Vector3.Max(BoxCollider.bounds.min, otherPlayersCollider.bounds.min),
                Vector3.Min(BoxCollider.bounds.max, otherPlayersCollider.bounds.max));
//            Debug.Log(intersectionBounds.size);
            //           Debug.Log("Intersecting");
        }
        else
        {
            intersecting = false;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        player.SetFrictionBox(false);
    }
}