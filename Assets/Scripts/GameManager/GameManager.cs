using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public class ReadOnlyAttribute : PropertyAttribute { } 
    [SerializeField] internal List<PlayerController> players;
   [SerializeField] private CharacterSODataBase characterDatabase;
   private readonly List<InputDevice> _availableDevices = new (); 
   private const int MinDistance = 1;

    #region Win Screen Setting
   [Header ("Win Screen Settings")]
   [SerializeField] private GameObject GameOverScreen;
   [SerializeField] private Sprite _p1WinSprite, _p2WinSprite, _drawSprite; 
   [SerializeField] private Image WinSplashScreen;
   [SerializeField] private Button restartButton;
   private PlayerController _winner; 
   
   #endregion
 
    #region Round Timer
    [Header("Round Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private bool isLowTime;
    [SerializeField]private float _roundTimer ;
    private float _currentRoundTimer;
    private int _roundTimerInt; 
    private Action LowTimeAction;
    private bool activated;
    [SerializeField] private TMP_ColorGradient  lowTime;
    [SerializeField] private bool RoundTimer;
    #endregion
    
    
    
    #region Animation 
    private CinemachineCamera AnimationCamera; 
    [SerializeField] private Animator CameraAnims;
    [SerializeField] private Animator UIAnim;
    #endregion
    private void Awake()
    {
        AnimationCamera = CameraAnims.GetComponent<CinemachineCamera>();

        Application.targetFrameRate = 60;
        _currentRoundTimer = _roundTimer;
        UpdateRoundTimer();
        StartGameDebug();

        // CHANGE THIS TO ACCEPT INPUT FROM CHARACTER SELECTION, THIS HURTS TO LEAVE
        foreach (var player in players)
        {
            player.CharacterData = characterDatabase.defaultCharacterSo;
        }
        Time.timeScale = 1;
        HitDetection.OnDeath += OnRoundEnd;
        LowTimeAction += SwapColor; 
        ConnectDeviceToPlayer();
    }
    private void Start()
    {
        // StartCoroutine(IntroAnim());
    }

    //This is purely for Debug Mode, skips cutscene 
    private void StartGameDebug()
    {
        AnimationCamera.enabled = false;
        UIAnim.Play("slide in");
        StartCoroutine(StartTimer());
    }

    #region RoundTimer

    private IEnumerator StartTimer()
    {
        while (_currentRoundTimer > 0)
        {
            _currentRoundTimer -= Time.deltaTime;
            UpdateRoundTimer();
            yield return null;

        }
        OnRoundEnd();
    }
    private void UpdateRoundTimer()
    {
        timerText.text =  Mathf.RoundToInt( _currentRoundTimer).ToString();
        if (_currentRoundTimer <= 10 && !activated)
        {
            activated = true;
            print("nice");
            LowTimeAction?.Invoke();
        }
    }
    private void SwapColor()
    {
        timerText.colorGradientPreset = lowTime;
        LowTimeAction -= SwapColor;
    }
    #endregion

    #region RoundEnd
    private void OnRoundEnd()
    {
        foreach (var player in players)
        {
            if(player.Animations.Animator is not null)  player.Animations.Animator.enabled = false;
            player.hitBox.SetActive(false);
            if (player.isDead)
            {
                player.gameObject.SetActive(false);
            }
        }
        DisplayEndScreen();
    }

    private void DisplayEndScreen()
    {
        _winner = players.FirstOrDefault(player => !player.isDead);
        if (Mathf.Approximately(players[0].Health, players[1].Health)) _winner = null;
        WinSplashScreen.sprite = _winner is null ? _drawSprite : _winner == players[0] ? _p1WinSprite : _p2WinSprite; 
        GameOverScreen.gameObject.SetActive(true);
       if(UIController.instance) UIController.instance?.SelectObject(restartButton);
        Time.timeScale = 0;
    }
    

    #endregion
   
    #region Connection
    private void OnAdd(InputDevice device)
    {
        if (!_availableDevices.Contains(device)) _availableDevices.Add(device);
    }
    private void ConnectDeviceToPlayer()
    {
        // temp method to add devices to a pool in order to connect them to a player 
        foreach (var device in InputSystem.devices.Where(device => device is Gamepad or Keyboard))
        {
            _availableDevices.Add(device);
        }
        OnConnect();
        InputSystem.onDeviceChange += (device, change) =>
        {
            //May need to add removed, disconnected 
            switch (change)
            {
                case InputDeviceChange.Added:
                    OnAdd(device);
                    OnConnect();
                    break;
                case InputDeviceChange.Reconnected:
                    OnConnect();
                    break;
                case InputDeviceChange.Removed:
                    break;
                case InputDeviceChange.Disconnected:
                    break;
            }
        };
    }

    private void OnConnect()
    {
        ConnectPlayer();
    }
    
    private void ConnectPlayer()
    {
        for (var i = 0; i < players.Count; i++)
        {
            if (i < _availableDevices.Count)
            {
                players[i].InitializePlayer(_availableDevices[i]);
      //          Debug.Log($"Assigned {_availableDevices[i].name} to Player {i + 1}");
            }
            else
            {
                if(!_availableDevices.Contains( Keyboard.current) ) players[i].InitializePlayer(Keyboard.current);
                else players[i].InitializePlayer(new Gamepad());
            }

        }
    }

    #endregion
    
    private void Update()
    { 
        CheckIfReversed();
    }
    private void OnDestroy()
    {
        HitDetection.OnDeath -= OnRoundEnd;
    }

    #region ChangePlayerDirection
    private void CheckIfReversed()
    {
        //depending on the distance between players, and if they are grounded, reverse (flip) the player 
        var distance = Mathf.Abs(players[0].transform.position.x - players[1].transform.position.x);

        if (distance < MinDistance)
            return;
            
        if (players[1].transform.position.x < players[0].transform.position.x)
        {
            players[0].Reversed = true;
            players[1].Reversed = false;
        }
        else
        {
            players[0].Reversed = false;
            players[1].Reversed = true;
        }

        foreach (var player in players)
        {
            UpdatePlayerDirection(player);
        }
    }
    private static void UpdatePlayerDirection(PlayerController player)
    {
        if (!player.IsGrounded) return;
        var targetYRotation = player.Reversed ? 270 : 90f;
        var rotation = player.transform.eulerAngles;
        rotation.y = targetYRotation;
        player.transform.eulerAngles = rotation;
    }
    #endregion



    #region Animation
        private IEnumerator IntroAnim()
        {
            FreezePlayer();
            yield return new WaitForSeconds(18);
            UIAnim.Play("slide in");
            yield return new WaitForSeconds(1);
            UIAnim.Play("Countdown");
            yield return new WaitForSeconds(1.5f);
            UnFreezePlayer();
            StartCoroutine(StartTimer());
        }

        private void FreezePlayer()
        {
            foreach (var player in players )
            {
                player.rb.isKinematic = true;
            }
        }
        private void UnFreezePlayer()
        {
            foreach (var player in players )
            {
                player.rb.isKinematic = false;
            }
      
        }
        #endregion
  
}
