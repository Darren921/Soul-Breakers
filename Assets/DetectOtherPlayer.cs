using System;
using UnityEngine;

public class DetectOtherPlayer : MonoBehaviour
{
    PlayerController player;

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
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
