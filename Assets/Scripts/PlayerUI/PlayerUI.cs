using System;
using UnityEngine;
using UnityEngine.InputSystem;
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

   private bool isUIOn = true;
    private void Awake()
    {
        HitDetection.OnPlayerHit += UpdateHealth;
        HitDetection.OnPlayerHit += UpdateSuperMeter;

        GameManager.OnRefresh += UpdateHealth;
        GameManager.OnRefresh += UpdateSuperMeter;

        _playerController = GetComponent<PlayerController>();
    }

 

    private void ToggleUIOnperformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("UI On performed");
        if (ctx.phase == InputActionPhase.Performed)
        {
            isUIOn = !isUIOn;
//            Debug.Log(isUIOn);
            GameManager.ToggleUIAction?.Invoke(isUIOn);
        }

        
    }

    private void UpdateUIVisibility(bool isOn)
    {
        _healthSlider.gameObject.SetActive(isOn);
        _superMeterSlider.gameObject.SetActive(isOn);
        Layer2Super.gameObject.SetActive(isOn);
        Layer3Super.gameObject.SetActive(isOn);
        SuperLights.gameObject.SetActive(isOn);
        SuperNumber.gameObject.SetActive(isOn);
    }

    private void UpdateSuperMeter()
    {
        switch (_playerController.superMeter)
        {
            case <= 100:
                SetActiveBar(false, false);
                SetSuperSprites(0);
                _superMeterSlider.value = _playerController.superMeter;
                SetSuperSprites(0);
                break;
            case <= 200:
                SetActiveBar(true, false);
                _superMeterSlider.value = _superMeterSlider.maxValue;
                Layer2Super.fillAmount = (_playerController.superMeter - 100) / 100f ;
                SetSuperSprites(1);
                break;
            case <= 300:
                Layer2Super.fillAmount = 1; 
                SetActiveBar(true, true);
                Layer3Super.fillAmount = (_playerController.superMeter - 200) / 100f;
                SetSuperSprites(2);
                break;
            case >= 300:
                Layer3Super.fillAmount = 1;
                SetSuperSprites(3);
                break;
        }
    }

    private void SetSuperSprites(int spriteIndex)
    {
        SuperNumber.sprite = NumberSprites[spriteIndex];
        SuperLights.sprite = lightSprites[spriteIndex];
    }

    private void SetActiveBar(bool layer2, bool layer3)
    {
        Layer2Super?.gameObject?.SetActive(layer2);
        Layer3Super?.gameObject?.SetActive(layer3);
    }

    private void OnDestroy()
    {
        HitDetection.OnPlayerHit -= UpdateHealth;  
        HitDetection.OnPlayerHit -= UpdateSuperMeter;
        GameManager.OnRefresh -= UpdateSuperMeter;
        _playerController._controls.UI.ToggleUI.performed -= ToggleUIOnperformed; 


    }

    private void Start()
    {
        if (_playerController.CharacterData != null)
        {
            _healthSlider.maxValue = _playerController.CharacterData.health;
            _healthSlider.value = _playerController.CharacterData.health;
            _superMeterSlider.maxValue = 100;
        }
        if(_playerController._controls != null) _playerController._controls.UI.ToggleUI.performed += ToggleUIOnperformed; 
        GameManager.ToggleUIAction += ToggleUIAction;

        _playerController.Health = _playerController.CharacterData.health;
       
        UpdateHealth();
    }

    public void ToggleUIAction(bool obj)
    {
        Debug.Log(obj);
        UpdateUIVisibility(obj);
        if (isUIOn)
        {
            HitDetection.OnPlayerHit += UpdateHealth;
            HitDetection.OnPlayerHit += UpdateSuperMeter;
            GameManager.OnRefresh += UpdateHealth;
            GameManager.OnRefresh += UpdateSuperMeter;
            UpdateHealth();
            UpdateSuperMeter();
        }
        else
        {
            HitDetection.OnPlayerHit -= UpdateHealth;  
            HitDetection.OnPlayerHit -= UpdateSuperMeter;
            GameManager.OnRefresh -= UpdateSuperMeter;
            GameManager.OnRefresh -= UpdateHealth;
        }    }


    private void UpdateHealth()
    {
        _healthSlider.value = _playerController.Health;
    }
}