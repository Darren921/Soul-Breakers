using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
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

   [Header ("Win Screen Settings")]
   [SerializeField] private GameObject GameOverScreen;
   [SerializeField] private Sprite _p1WinSprite, _p2WinSprite; 
   [SerializeField] private Image WinSplashScreen;
    
   [Header("Round Timer")]
   [SerializeField] private TextMeshProUGUI timerText;
   [SerializeField] private bool isLowTime;
   private float _roundTimer = 90;
   private float _currentRoundTimer;
   private int _roundTimerInt; 
   private Action LowTimeAction;
   private bool activated;
   [SerializeField] private TMP_ColorGradient normal, lowTime;
   [SerializeField] private bool RoundTimer;
   [SerializeField] private Animator CameraAnims;
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject Player2;
    [SerializeField] private Animator UIAnim;
    private void Awake()
    {
        
        _currentRoundTimer = _roundTimer;
        
        StartCoroutine(IntroAnim());
        // CHANGE THIS TO ACCEPT INPUT FROM CHARACTER SELECTION, THIS HURTS TO LEAVE
        foreach (var player in players)
        {
            player.CharacterData = characterDatabase.defaultCharacterSo;
        }

        UpdateRoundTimer();
        Time.timeScale = 1;
        
        HitDetection.OnDeath += OnRoundEnd;
        Application.targetFrameRate = 60;
        LowTimeAction += SwapColor;
        
    //    Time.timeScale = 0.1f;
    
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
                    OnDisconnect();
                    break;
            }
        };
    }


    private void SwapColor()
    {
        timerText.colorGradientPreset = lowTime;
        LowTimeAction -= SwapColor;
    }

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


    private void OnDestroy()
    {
        HitDetection.OnDeath -= OnRoundEnd;
        
    }
    
    private void OnRoundEnd()
    {
        foreach (var player in players)
        {
            if(player.Animator is not null)  player.Animator.enabled = false;
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
        var winner = players.Where(player => player.Health > 0 ).OrderByDescending(player => player.Health).FirstOrDefault();
        GameOverScreen.gameObject.SetActive(true);
        WinSplashScreen.sprite = winner == players[0] ? _p1WinSprite : _p2WinSprite;
        Time.timeScale = 0;
    }


    private void OnAdd(InputDevice device)
    {
        if (!_availableDevices.Contains(device)) _availableDevices.Add(device);
    }

    private void OnConnect()
    {
        //var input1 = deviceIndex.GetValueOrDefault(players[0]);
        //players[0].InitializePlayer(input1);
        //var input2 = deviceIndex.GetValueOrDefault(players[1]);
        //players[1].InitializePlayer(input2);
        
        //temp method to give a player controls depending on device connected first 
        ConnectPlayer();
    }

    private void OnDisconnect()
    {
        foreach (var player in players)
        {
    //        if(player)
        }
    }

    private void ConnectPlayer()
    {
        for (var i = 0; i < players.Count; i++)
        {
            if (i < _availableDevices.Count)
            {
                players[i].InitializePlayer(_availableDevices[i]);
                Debug.Log($"Assigned {_availableDevices[i].name} to Player {i + 1}");
            }
            else
            {
                if(!_availableDevices.Contains( Keyboard.current) ) players[i].InitializePlayer(Keyboard.current);
                else players[i].InitializePlayer(new Gamepad());
//                Debug.LogWarning($"No input device available for Player {i + 1}");
            }

        }
    }


    // Update is called once per frame
        private void Update()
        {
          CheckIfReversed();
          
        }

       

      

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

            UpdatePlayerDirection(players[0]);
            UpdatePlayerDirection(players[1]);
        }

        private static void UpdatePlayerDirection(PlayerController player)
        {
            if (!player.IsGrounded) return;
            var targetYRotation = player.Reversed ? 270 : 90f;
            var rotation = player.transform.eulerAngles;
            rotation.y = targetYRotation;
            player.transform.eulerAngles = rotation;

        }
    private IEnumerator IntroAnim()
    {
        players[0].GetComponent<Rigidbody>().isKinematic = true;
        players[1].GetComponent<Rigidbody>().isKinematic = true;
        
        
        yield return new WaitForSeconds(18);
        UIAnim.Play("slide in");
        players[0].GetComponent<Animator>().Play("newtransition");
        players[1].GetComponent<Animator>().Play("newtransition");
        yield return new WaitForSeconds(1);
        UIAnim.Play("Countdown");
        
        yield return new WaitForSeconds(1.5f);
        players[0].GetComponent<Rigidbody>().isKinematic = false;
        players[1].GetComponent<Rigidbody>().isKinematic = false;
        
        StartCoroutine(StartTimer());
    }
   
}
