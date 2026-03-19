using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private PlayerController _playerController;

   [SerializeField] private Slider _healthSlider;
   [SerializeField] private Slider _superMeterSlider;
   [SerializeField] private Image Layer2Super, Layer3Super;
   [SerializeField] private Image SuperLights;
   [SerializeField] private Sprite[] lightSprites;
   
   [SerializeField] private Image SuperNumber;
   [SerializeField] private Sprite[] NumberSprites;

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
        switch (_playerController.superMeter)
        {
            case <= 100:
                Layer2Super.gameObject.SetActive(false);
                Layer3Super.gameObject.SetActive(false);
                _superMeterSlider.value = _playerController.superMeter;
                SuperNumber.sprite = NumberSprites[0];
                SuperLights.sprite = lightSprites[0];
                break;
            case <= 200:
                Layer2Super.gameObject.SetActive(true);
                Layer3Super.gameObject.SetActive(false);
                _superMeterSlider.value = _superMeterSlider.maxValue;
                Layer2Super.fillAmount = (_playerController.superMeter - 100) / 100f ;
                SuperNumber.sprite = NumberSprites[1];
                SuperLights.sprite = lightSprites[1];
                break;
            case <= 300:
                Layer2Super.fillAmount = 1; 
                Layer2Super.gameObject.SetActive(true);
                Layer3Super.gameObject.SetActive(true);
                Layer3Super.fillAmount = (_playerController.superMeter - 200) / 100f;
                SuperNumber.sprite = NumberSprites[2];
                SuperLights.sprite = lightSprites[2];
                break;
            case >= 300:
                Layer3Super.fillAmount = 1;
                SuperNumber.sprite = NumberSprites[3];
                SuperLights.sprite = lightSprites[3];
                break;
        }
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