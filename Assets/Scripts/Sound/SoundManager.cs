using System;
using System.Collections.Generic;
using System.IO;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;


public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }
    private EventInstance musicEventRef;
    [SerializeField] internal SoundDataBase soundData; 
    private EventReference lastEventRef;
    private List<EventInstance> _eventInstances = new List<EventInstance>();
    private EventInstance currentPlaying;

    private void Start()
    {
        // Singleton Init 
        if (instance == null)
        {
            instance = this;
         //  PlayMusic(soundData.ReturnEventReference(SoundData.SoundType.Music, "MainTrack"));
           SceneManager.activeSceneChanged += OnSceneChanged;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene lastScene , Scene nextScene)
    {
        //search each song and check if song is scene specific 
        foreach (var sound in soundData.Sounds)
        {
            if (!sound.IsSceneSpecific) continue;

            if ((sound.SceneBound & 1 << soundData.SceneList.IndexOf(nextScene.name)) == 0) continue;
            PlayMusic(sound.SoundEvtRef);
            return;
        }
    } 
    // How to use Play Music 
    // PlayMusic(soundData.ReturnEventReference(SoundType , Sound Name)
    // PlayMusic(EventReference)
    
    // Other classes 
    // SoundManger.instance.PlayMusic(SoundManger.instance.soundData.ReturnEventReference(SoundType , Sound Name)
    public void PlayMusic(EventReference musicEventReference) 
    {
       // if the current song is player it disregards it 
        if(lastEventRef.Guid == musicEventReference.Guid) return;
        //Stops the last song, then plays a new one 
        currentPlaying.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currentPlaying = RuntimeManager.CreateInstance(musicEventReference);
        currentPlaying.start();
        currentPlaying.release();
        lastEventRef = musicEventReference;
    }

  
    public EventInstance CreateEventInstance(EventReference eventRef)
    {
        //creates a new event Instance (used to track time based  sounds in other systems, IE footsteps, Reloading)
        var eventInstance = RuntimeManager.CreateInstance(eventRef);
        _eventInstances.Add(eventInstance);
        return eventInstance;
    }

    // How to use PlayOneShot 
    // PlayOneShot(soundData.ReturnEventReference(SoundType , Sound Name)
    
    // Other classes
    // SoundManger.instance.PlayOneShot(SoundManger.instance.soundData.ReturnEventReference(SoundType , Sound Name)
    public void PlayOneShot(EventReference sound, Vector3 position )
    {
        //Play a sound one time 
        RuntimeManager.PlayOneShot(sound, position);
    }

    

  
}
