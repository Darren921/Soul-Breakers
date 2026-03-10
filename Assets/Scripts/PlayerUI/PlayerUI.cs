using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private PlayerController _playerController;

   [SerializeField] private Slider _healthSlider;
   [SerializeField] private Slider _superMeterSlider;
    private void Awake()
    {
        HitDetection.OnPlayerHit += UpdateHealth;
        HitDetection.OnPlayerHit += UpdateSuperMeter;

        GameManager.OnRefresh += UpdateHealth;
        GameManager.OnRefresh += UpdateSuperMeter;

        _playerController = GetComponent<PlayerController>();
      
    }

    private void UpdateSuperMeter()
    {
        _superMeterSlider.value = _playerController.superMeter;
    }

    private void OnDestroy()
    {
        HitDetection.OnPlayerHit -= UpdateHealth;  
        HitDetection.OnPlayerHit -= UpdateSuperMeter;

    }

    private void Start()
    {
        if (_playerController.CharacterData != null)
        {
            _healthSlider.maxValue = _playerController.CharacterData.health;
            _healthSlider.value = _playerController.CharacterData.health;
            _superMeterSlider.maxValue = 100;
        }

        _playerController.Health = _playerController.CharacterData.health;
       
        UpdateHealth();
    }



    private void UpdateHealth()
    {
        _healthSlider.value = _playerController.Health;
    }
}