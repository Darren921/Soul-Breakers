using System;
using System.Collections.Generic;
using System.IO;
using FMODUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
[CustomEditor(typeof(SoundManager))]

public class MusicPlayerEditor : Editor
{
    
    private List<string> sceneList = new List<string>();
    private List<string> musicEvents = new List<string>();
 

   
    public VisualTreeAsset visualTree;
    public override VisualElement CreateInspectorGUI()
    {
       // GetSceneList();
     //   var root = new VisualElement();
     //   visualTree.CloneTree(root);
        
        
        
        // SceneListDropdown = root.Q<DropdownField>("SceneList");
        // SceneListDropdown.choices = sceneList;
        // MusicEventDropdown = root.Q<DropdownField>("MusicList");
        // MusicEventDropdown.choices = musicEvents;

        return base.CreateInspectorGUI();
    }
    
}
