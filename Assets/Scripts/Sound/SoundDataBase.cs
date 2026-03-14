using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SoundDataBase", menuName = "Scriptable Objects/SoundDataBase")]
public class SoundDataBase : ScriptableObject
{
    public List<SoundData> Sounds = new List<SoundData>();
    public List<string> SceneList = new List<string>();
   
    
  


    
    private void OnValidate()
    {
        GetSceneList();
        if (Sounds.Count <= 0) return;
        foreach (var sound in Sounds)
        {
            SplitTypeAndName(sound);
        }
    }
    
    // Splits Type and Name from Fmod Paths
    // Note That YOU NEED a folder with the sound type name and place your sound in there 
    private static void SplitTypeAndName(SoundData sound)
    {
#if UNITY_EDITOR
        var splits = sound.SoundEvtRef.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (splits.Length == 3)
        {
            var soundTypeString = splits[1];
            Enum.TryParse(soundTypeString, out SoundData.SoundType SoundType);
            sound.soundType = SoundType;
        }
        sound.SoundName = sound.SoundEvtRef.Path.Split('/').Last().Replace(" ", "").ToLower();
#endif
    }

    //Return the sound given, how to use in SoundManager 
    public EventReference ReturnEventReference(SoundData.SoundType soundType, string soundName)
    {
        var eventRef = new EventReference();
        eventRef = Sounds.Find(data => data.soundType == soundType && string.Equals(data.SoundName, soundName, StringComparison.CurrentCultureIgnoreCase)).SoundEvtRef;
        return  eventRef;
    }

    //Gets all the scenes possible in build index 
    private void GetSceneList()
    {
        SceneList.Clear();
        SceneList.Insert(0,"None");
        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var sceneIndex = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            if (!SceneList.Contains(sceneIndex))
            {
                SceneList.Add(sceneIndex);
            }
        }
    }


}

[Serializable]
public class SoundData
{
    public enum  SoundType
    {
        None,
        Music, 
        SFX,
        Interface,
        VO
    }
    public EventReference SoundEvtRef;
    public SoundType soundType;
    public string SoundName;
    public bool IsSceneSpecific;
    public int SceneBound;
    public SoundData( SoundType type, EventReference soundEvtRef ,string soundName, bool isSceneSpecific,int sceneBound )
    {
        soundType = type;
        SoundName = soundName;
        SoundEvtRef =  soundEvtRef;
        IsSceneSpecific = isSceneSpecific;
        SceneBound = sceneBound;
    }
}
