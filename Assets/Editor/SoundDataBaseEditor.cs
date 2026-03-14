using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


[CustomEditor(typeof(SoundDataBase))]
public class SoundDataBaseEditor : Editor
{
    public VisualTreeAsset visualTree;
    public override VisualElement CreateInspectorGUI()
    {
        
        var root = new VisualElement();
        
        visualTree.CloneTree(root);

        var space = "\n";
        var MultiColumnList = root.Q<MultiColumnListView>("MultiColumnList");
        MultiColumnList.tooltip = "Basic How to use" + space + space +
                                  "Find the magnifying glass in the event section select your event," +
                                  "the system will generate a sound name/type based on the event name and event folder name. " + space + space +
                                  "NOTE THAT if you can't access the magnifying glass stretch the event section till it's accessible"+ space + space +
                                  "Only USE SCENEBASED AND SCENE NAME IF IT IS A MUSIC TRACK AND IT'S PLAYED IN A CERTAIN SCENE" ; 
        var so = new SerializedObject(serializedObject.targetObject);
        var soundDataBase = (SoundDataBase)serializedObject.targetObject;
      
        MultiColumnList.columns["sceneName"].makeCell += () =>
        {

            var MaskField = new MaskField
            {
                choices = soundDataBase.SceneList
            };
            return MaskField;
        };
        MultiColumnList.Bind(so);
        return root;
    }

   
}
