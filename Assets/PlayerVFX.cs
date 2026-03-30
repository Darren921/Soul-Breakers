using System;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFX : MonoBehaviour
{
   [SerializeField] VisualEffect JumpEffect;
   [SerializeField] GameObject FireEffect;
   
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

   public void PlayFireEffect()
   {
      FireEffect?.SetActive(true);
   }
   public void StopFireEffect()
   {
      FireEffect?.SetActive(false);
   }
}
