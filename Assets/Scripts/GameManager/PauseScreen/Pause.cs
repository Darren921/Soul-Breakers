using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject VolumeMenu;
    public static bool isPaused;

    private Controls _actions;

    private void Awake()
    {
        _actions = new Controls();
    }

    private void OnEnable()
    {
        _actions.Enable();
        _actions.UI.Pause.performed += OnPausePressed;
    }

    private void OnDisable()
    {
        _actions.UI.Pause.performed -= OnPausePressed;
        _actions.Disable();
    }

    private void Start()
    {
        pauseMenu.SetActive(false);
    }

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        isPaused = true;
        PauseManager.Instance?.SetPaused(true);
    }

    public void ActivateVolumeMenu()
    {
        VolumeMenu.SetActive(true);
    }

  

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        isPaused = false;
        PauseManager.Instance?.SetPaused(false);
    }
}