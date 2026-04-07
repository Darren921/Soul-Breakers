using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public Action SoundVolumeUpdateAction; 
    
    private Bus MasterBus;
    private Bus InterfaceBus;
    private Bus SFXBus;
    private Bus MusicBus;

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
        MasterBus = RuntimeManager.GetBus("bus:/");
        SFXBus = RuntimeManager.GetBus("bus:/SFX");
        MusicBus = RuntimeManager.GetBus("bus:/Music");
        InterfaceBus = RuntimeManager.GetBus("bus:/Interface");

        SoundVolumeUpdateAction += UpdateSoundVolumse;
        SoundVolumeUpdateAction?.Invoke();
    }
    

    public void UpdateSoundVolumse()
    {
        RuntimeManager.GetBus("bus:/").setVolume(soundData.CurrentVolume.curMasterVolume);
    RuntimeManager.GetBus("bus:/Music").setVolume(soundData.CurrentVolume.curMusicVolume);
    RuntimeManager.GetBus("bus:/Interface").setVolume(soundData.CurrentVolume.curInterfaceVolume);
    RuntimeManager.GetBus("bus:/SFX").setVolume(soundData.CurrentVolume.curSFXVolume);

    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene lastScene , Scene nextScene)
    {
       
        UpdateSoundVolumse();
        var hasSceneSpecific = soundData.Sounds.Where(sound => sound.IsSceneSpecific).ToList();
       
        //search each song and check if song is scene specific 
        foreach (var sound in hasSceneSpecific)
        {
            Debug.Log(sound.SceneBound);
            Debug.Log(soundData.SceneList.IndexOf(nextScene.name));
            if ((sound.SceneBound & 1 << soundData.SceneList.IndexOf(nextScene.name)) == 0) continue;
            Debug.Log("Played");
            PlayMusic(sound.SoundEvtRef);
            return;
        }
        Debug.Log("Stopped");
        StopMusic();
        lastEventRef = new EventReference();


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

    public void StopMusic()
    {
        currentPlaying.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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
