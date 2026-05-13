using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[CustomEditor(typeof(CharacterSODataBase), true), CanEditMultipleObjects]
public class PlayerSOEditor : Editor
{
    [SerializeField] private VisualTreeAsset mainVisualTree;
    [SerializeField] private VisualTreeAsset listVisualTree;
    private SerializedProperty _listProperty;
    private TemplateContainer _list;
    private ObjectField characterRef;


    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        mainVisualTree.CloneTree(root);
        var listView = root.Q<ListView>("ListView");
        _listProperty = serializedObject.FindProperty("characterSoList");
        if (listView == null) Debug.LogError("listview property is null");
        if (_listProperty == null) Debug.LogError("list property is null");
        listView.bindingPath = _listProperty.propertyPath;


        listView.makeItem = () =>
        {
            _list = listVisualTree.Instantiate();
            return _list;
        };

        listView.bindItem = BindItem;
        
        return root;
    }

    private void BindItem(VisualElement element, int index)
    {
        var elementProperty = _listProperty.GetArrayElementAtIndex(index);
        characterRef = element.Q<ObjectField>("CharacterSO");
        var propertiesContainer = element.Q<VisualElement>("CharacterProperties");
        var foldoutElement = element.Q<Foldout>("CharacterSheet");
        if (characterRef == null) Debug.LogError("character ref is null");
        if (propertiesContainer == null) Debug.LogError("propertiesContainer is null");
        

        characterRef?.BindProperty(elementProperty);

        characterRef?.UnregisterValueChangedCallback(evt => OnObjectChanged(evt, propertiesContainer, foldoutElement));
        characterRef?.RegisterValueChangedCallback(evt => OnObjectChanged(evt , propertiesContainer,foldoutElement) );
        UpdateListElements(propertiesContainer, elementProperty.objectReferenceValue as CharacterSO, foldoutElement);
    }


    private void UpdateListElements(VisualElement element, CharacterSO characterSo, Foldout foldoutElement)
    {
        element.Clear();
        if (characterSo == null || characterRef == null )
        {
            foldoutElement.text = "No Character";
        }
        else
        {
            var charSo = new SerializedObject(characterSo);
            var iterator = charSo.GetIterator();
            if (iterator.NextVisible(true))
            {
                while (iterator.NextVisible(false)){
                    if (iterator.propertyPath == "characterName")
                    {
                        foldoutElement.text = iterator.stringValue == "" ? "empty":  foldoutElement.text = iterator.stringValue;
                    }
                    var field = new PropertyField(iterator.Copy());
                    element.Add(field);
                }

                element.style.display = DisplayStyle.Flex;
            }
            else
            {
                element.style.display = DisplayStyle.None;
            }
        }
    }

    private void OnObjectChanged(ChangeEvent<Object> evt, VisualElement element, Foldout foldout)
    {
        var characterSo = evt.newValue as CharacterSO;
        UpdateListElements(element, characterSo,foldout);
    }
}