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
   private const float MinDistance = 0.1f;
   public static  Action OnRefresh ;
   public static  Action<bool> ToggleUIAction ;
   private bool StandardConnectDone;
   private bool Intro;
   private bool EndGame; 
   public Dictionary<PlayerController , InputDevice> PlayersInputDevice { get; private set; } = new();
    #region Win Screen Setting
   [Header ("Win Screen Settings")]
   [SerializeField] private GameObject GameOverScreen;
   [SerializeField] private Sprite _p1WinSprite, _p2WinSprite, _drawSprite; 
   [SerializeField] private Image WinSplashScreen;
   [SerializeField] private Button restartButton;
   private PlayerController _winner;
   [SerializeField] GameObject Ghost;
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

  private (float, float) MapBorderLocationX;
 [SerializeField] private float MapBorderLocationX1, MapBorderLocationX2;
    private bool SinglePlayer; 
    private void Awake()
    {
        EndGame = false;
     //   Application.runInBackground = true;
        MapBorderLocationX = (MapBorderLocationX1, MapBorderLocationX2);
        Cursor.visible = true;
        AnimationCamera = CameraAnims.GetComponent<CinemachineCamera>();
        Application.targetFrameRate = 60;
        _currentRoundTimer = _roundTimer;
        UpdateRoundTimer();
       // StartGameDebug();
        ToggleUIAction += ToggleUIGlobal;
 
        Time.timeScale = 1;
        HitDetection.OnDeath += OnRoundEnd;
        LowTimeAction += SwapTextColor; 
        ConnectDeviceToPlayer();
    }

    private void ToggleUIGlobal(bool toggle)
    {
        players[0].playerUI.ToggleUIAction(toggle);
        timerText?.gameObject.SetActive(toggle);
    }

    private void Start()
    {
        Intro = true;
       AnimationCamera.enabled = true;
       StartCoroutine(IntroAnim());
    }

   // This is purely for Debug Mode, skips cutscene 
     // public void StartGameDebug()
     // {
     //     AnimationCamera.enabled = false;
     //     UIAnim.Play("slide in");
     //     StartCoroutine(StartTimer());
     // }

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
    private void SwapTextColor()
    {
        timerText.colorGradientPreset = lowTime;
        LowTimeAction -= SwapTextColor;
    }
    #endregion

    #region RoundEnd
    private void OnRoundEnd()
    {
        
        
        foreach (var player in players)
        {
           // if(player.Animations.Animator is not null)  player.Animations.Animator.enabled = false;
            player.hitBox.SetActive(false);
            if (player.isDead)
            {
                //player.gameObject.SetActive(false);
            }
        }
        DisplayEndScreen();
    }

    private void DisplayEndScreen()
    {


        
        
        
        _winner = players.Where(controller => !controller.isDead).OrderByDescending(c => c.Health).FirstOrDefault();
        //        Debug.Log(_winner);

        if (Mathf.Approximately(players[0].Health, players[1].Health)) _winner = null;
        WinSplashScreen.sprite = _winner is null ? _drawSprite : _winner == players[0] ? _p1WinSprite : _p2WinSprite;

        StartCoroutine(GameEnd());



        if (UIController.instance) UIController.instance?.SelectObject(restartButton);
        //Time.timeScale = 0;
    }
    
    

    #endregion
   
    #region Connection
    private void OnAdd(InputDevice device)
    {
        if (!_availableDevices.Contains(device)) _availableDevices.Add(device);
    }

    private void OnRemove(InputDevice device)
    {
        if(_availableDevices.Contains(device)) _availableDevices.Remove(device);
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
                    OnAdd(device);
                    OnConnect();
                    break;
                case InputDeviceChange.Disconnected:
                    OnRemove(device);
                    OnDisconnect(device);
                    break;
            }
        };
    }

    private void OnConnect()
    {
        ConnectPlayer();
        OnRefresh?.Invoke();
    }

    private void OnDisconnect(InputDevice device)
    {
        if (SinglePlayer)
        {
            SinglePlayer = false;
            PlayersInputDevice.Clear();
            _availableDevices.Remove(device);
            players[0].DisconnectPlayer();
            Debug.Log("Switching to multi ");
        }
        else
        {
            var disconnected = PlayersInputDevice.FirstOrDefault(pair => pair.Value == device).Key;
            disconnected.DisconnectPlayer();
            _availableDevices.Remove(device);
            PlayersInputDevice.Remove(disconnected);
            Debug.Log("Disconnecting  device");

        }
        ConnectPlayer();

    }
    
    private void ConnectPlayer()
    {
        if (_availableDevices.Count == 1)
        {
            players[0].InitializePlayer(_availableDevices[0]);
            if(PlayersInputDevice.ContainsKey(players[0])) return;
            PlayersInputDevice.Add( players[0], _availableDevices[0]);
            SinglePlayer = true;
            Debug.Log("One Player only");
            return;
        }
        if (StandardConnectDone)
        {
            foreach (var player in players)
            {
                if (player.PlayerConnected)
                {
                    player.DisconnectPlayer();
                    PlayersInputDevice.Remove(player);
                    Debug.Log("ResetStandardConnect");
                }
            }
        }
        var Gamepads = _availableDevices.OfType<Gamepad>().OrderBy(pair => pair.name).ToList();
        if (Gamepads.Count == 2)
        {
            foreach (var player in players)
            {
                if (!player.PlayerConnected && !PlayersInputDevice.ContainsKey(player))
                {
                    var UsableInputDevice = Gamepads.First(device => !PlayersInputDevice.ContainsValue(device));
                    Debug.Log(UsableInputDevice);
                    if (UsableInputDevice is not null)
                    {
                        PlayersInputDevice.Add(player, UsableInputDevice);
                        player.InitializePlayer(UsableInputDevice);
                    }
                }
            }
            Debug.Log("2Gamepad Connect");
        }
        else
        {
            
            foreach (var player in players)
            {
                if (!player.PlayerConnected && !PlayersInputDevice.ContainsKey(player))
                {
                    Debug.Log("Player not connected");
                    var UsableInputDevice = _availableDevices.First(device => !PlayersInputDevice.ContainsValue(device) && !PlayersInputDevice.ContainsKey(player));
                    Debug.Log(UsableInputDevice);
                    if (UsableInputDevice is not null)
                    {
                        PlayersInputDevice.Add(player, UsableInputDevice);
                        player.InitializePlayer(UsableInputDevice);
                    }
                }
                
            } 
            StandardConnectDone = true;
            Debug.Log("Standard Connect");
        }
     
    }

   

    #endregion
    
    
    private void Update()
    { 
        CheckIfReversed();
        if (Input.anyKey && Intro)
        {
            Intro = false;
            AnimationCamera.enabled = false;
            StopCoroutine(IntroAnim());

           
            StartCoroutine(Starting());

        }
    }
    private void OnDestroy()
    {
        HitDetection.OnDeath -= OnRoundEnd;
        ToggleUIAction -= ToggleUIGlobal;

    }

    #region ChangePlayerDirection
    private void CheckIfReversed()
    {
        if(players.Count < 2) return;
        //depending on the distance between players, and if they are grounded, reverse (flip) the player 
        var distance = Vector3.Distance(players[0].transform.position, players[1].transform.position);
//        Debug.Log(distance);
        foreach (var player in players)
        {
//           Debug.Log(player.transform.position.x);
   //       Debug.Log(MapBorderLocationX.Item2);
            if (player.transform.position.x <= MapBorderLocationX.Item1 || player.transform.position.x >= MapBorderLocationX.Item2)
            {
//               Debug.Log("Check Passed");
                player.AtBorder = !player.Reversed ? player.PlayerMove.x <= 0 : player.PlayerMove.x >= 0;
                return;
            }
            switch (distance)
            {
                case >= 19.5f:
                    player.AtBorder = !player.Reversed ? player.PlayerMove.x <= 0 : player.PlayerMove.x >= 0;
                    break;
                case <= 19.5f:
                    player.AtBorder = false;
                    break;
            }
        }
     
        
        if (distance < MinDistance)
            return;


        var leftMost = players.GroupBy(controller => controller.transform.position.x).OrderByDescending(group => group.Key );
        var selected = leftMost.First().Last();
        foreach (var player in players)
        {
            if (player != selected)
            {
                player.Reversed = false;
            }
        }
        selected.Reversed = true;
        foreach (var player in players)
        {
            UpdatePlayerDirection(player);
        }
    }
    private static void UpdatePlayerDirection(PlayerController player)
    {
        if (!player.GravityManager.IsGrounded) return;
        player.playerModel.transform.localScale = new Vector3(!player.Reversed ? 1 : -1, player.playerModel.transform.localScale.y,player.playerModel.transform.localScale.z );
    }
    #endregion
    


    #region Animation
        private IEnumerator IntroAnim()
        {
                Intro = true;
                FreezePlayer();
                yield return new WaitForSeconds(18);
                
                if (Intro)
                {
                    StartCoroutine(Starting());
                    Intro = false;
                    print("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                }
                Intro = false;

                
            
        }
        private IEnumerator Starting()
        {
            UIAnim.Play("slide in");
            yield return new WaitForSeconds(1);
            UIAnim.Play("Countdown");
            yield return new WaitForSeconds(1.5f);
            UnFreezePlayer();
            StartCoroutine(StartTimer());
        }
        private IEnumerator GameEnd()
        {
            if (_winner == players[0])
            {
                players[1].GetComponentInChildren<Animator>().Play("LucyDie");
                yield return new WaitForSeconds(2.5f);
                players[0].GetComponentInChildren<Animator>().Play("WinAnimation");
                yield return new WaitForSeconds(3.5f);
                GameOverScreen.gameObject.SetActive(true);
                UIAnim.Play("Win");
            }
            if (_winner == players[1])
            {
                yield return new WaitForSeconds(2.5f);
                Ghost.GetComponent<Animator>().Play("GhostWin");
                yield return new WaitForSeconds(3.5f);
                GameOverScreen.gameObject.SetActive(true);
                UIAnim.Play("Win");
            }


        }
        private void FreezePlayer()
        {
            foreach (var player in players )
            {
                player.OnDisablePlayer();
            }
        }
        private void UnFreezePlayer()
        {
            foreach (var player in players )
            {
                player.OnEnablePlayer();
            }
      
        }
        
        #endregion
  
}
