using System;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    GameObject LastselectedObject;
    GameObject nextTarget;

    InputSystemUIInputModule inputModule; 
    EventSystem eventSystem;
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
            SceneManager.activeSceneChanged -= SceneManagerOnactiveSceneChanged;

        }
    }

    private void SceneManagerOnactiveSceneChanged(Scene last, Scene nextScene)
    {
        nextTarget = null;
        LastselectedObject = null;
        eventSystem = EventSystem.current;
        inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule != null) inputModule.move.action.performed -= ActionOnperformed;
        inputModule.move.action.performed += ActionOnperformed;
        if (!LastselectedObject) LastselectedObject = eventSystem.firstSelectedGameObject;
    }


    void Start()
    {
        inputModule.deselectOnBackgroundClick = true;
    }

    private void ActionOnperformed(InputAction.CallbackContext ctx)
    {
 //       Debug.Log(ctx.phase);
//        Debug.Log(ctx.ReadValue<Vector2>());
        if(ctx.ReadValue<Vector2>() == Vector2.zero) return;
        if (eventSystem.currentSelectedGameObject && LastselectedObject != eventSystem.currentSelectedGameObject)
        {
            LastselectedObject = eventSystem.currentSelectedGameObject;
        }
        if(!eventSystem.currentSelectedGameObject && LastselectedObject) eventSystem.SetSelectedGameObject(LastselectedObject);

        lastInput = ctx.ReadValue<Vector2>();
        var nullCheck = CheckForNextTarget();
        if (!nullCheck && LastselectedObject) 
        {
            print($"{nextTarget} target found");

            nextTarget = CheckForNextTarget();
//            Debug.Log(nextTarget);
        }
    }

    private GameObject CheckForNextTarget()
    {
        return lastInput.y switch
        {
            < 0 => LastselectedObject.GetComponent<Selectable>().navigation.selectOnDown?.gameObject,
            > 0 => LastselectedObject.GetComponent<Selectable>().navigation.selectOnUp?.gameObject,
            _ => lastInput.x switch
            {
                < 0 => LastselectedObject.GetComponent<Selectable>().navigation.selectOnLeft?.gameObject,
                > 0 => LastselectedObject.GetComponent<Selectable>().navigation.selectOnRight?.gameObject,
                _ => LastselectedObject
            }

        };
    }

    public void SelectObject(Selectable selectable)
    {
        print($" {selectable} Selected and music played ");
        nextTarget = selectable.gameObject;
        instance.eventSystem.SetSelectedGameObject(nextTarget);
        SoundManager.instance?.PlayOneShot(SoundManager.instance.soundData.ReturnEventReference(SoundData.SoundType.Interface, "uiinteract"), transform.position);
    }

    public void DeselectObject()
    {
        SoundManager.instance?.PlayOneShot(SoundManager.instance.soundData.ReturnEventReference(SoundData.SoundType.Interface, "uiinteract"), transform.position);
        instance.eventSystem.SetSelectedGameObject(null);
    }
}