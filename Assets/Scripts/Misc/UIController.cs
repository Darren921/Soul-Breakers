using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    Selectable[] selectables;
    GameObject LastselectedObject;
    GameObject nextTarget;

    InputSystemUIInputModule inputModule;
    [SerializeField] EventSystem eventSystem;
    public static UIController instance;

    private Vector2 lastInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            SceneManager.activeSceneChanged += SceneManagerOnactiveSceneChanged;
            DontDestroyOnLoad(this);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void SceneManagerOnactiveSceneChanged(Scene last, Scene nextScene)
    {
        nextTarget = null;
        LastselectedObject = null;
        eventSystem = EventSystem.current;
        inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        inputModule.move.action.performed += ActionOnperformed;
        if (!LastselectedObject) LastselectedObject = eventSystem.firstSelectedGameObject;
    }


    void Start()
    {
        inputModule.deselectOnBackgroundClick = true;
    }

    private void ActionOnperformed(InputAction.CallbackContext ctx)
    {
        Debug.Log(ctx.phase);
        Debug.Log(ctx.ReadValue<Vector2>());

        if (eventSystem.currentSelectedGameObject && LastselectedObject != eventSystem.currentSelectedGameObject)
        {
            LastselectedObject = eventSystem.currentSelectedGameObject;
        }

        var nullCheck = CheckForNextTarget();
        if (!nullCheck && LastselectedObject) 
        {
//                print("Other target found");

            nextTarget = CheckForNextTarget();
            Debug.Log(nextTarget);
        }
        if (nextTarget) eventSystem.SetSelectedGameObject(nextTarget);
    }

    private GameObject CheckForNextTarget()
    {
        return lastInput.y switch
        {
            < 0 => LastselectedObject.GetComponent<Selectable>().navigation.selectOnDown.gameObject,
            > 0 => LastselectedObject.GetComponent<Selectable>().navigation.selectOnUp.gameObject,
            _ => lastInput.x switch
            {
                < 0 => LastselectedObject.GetComponent<Selectable>().navigation.selectOnLeft.gameObject,
                > 0 => LastselectedObject.GetComponent<Selectable>().navigation.selectOnRight.gameObject,
                _ => LastselectedObject
            }
        };
    }

    public void SelectObject(Selectable selectable)
    {
        print(selectable.name);
        nextTarget = selectable.gameObject;
        instance.eventSystem.SetSelectedGameObject(nextTarget);
    }

    public void DeselectObject()
    {
        instance.eventSystem.SetSelectedGameObject(null);
    }
}