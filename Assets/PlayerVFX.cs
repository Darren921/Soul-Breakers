using System;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFX : MonoBehaviour
{
   [SerializeField] VisualEffect JumpEffect;

   private VFXEventAttribute eventAttribute;
   private GameManager gameManager;

   private void Start()
   {
      gameManager = FindFirstObjectByType<GameManager>();
      eventAttribute = JumpEffect.CreateVFXEventAttribute();
   }

   public void PlayJumpEffect(PlayerController player)
   {
//      Debug.Log("PlayJumpEffect");
      JumpEffect.SendEvent(player == gameManager.players[0] ? "PlayJumpEffectP1" : "PlayJumpEffectP2", eventAttribute);
   }
}
