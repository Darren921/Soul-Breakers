using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UIElements.Toggle;


[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    [SerializeField] private VisualTreeAsset visualTree;

    private Toggle dToggle;
    private Toggle vToggle;

    private List<Foldout> debugVars;
    private List<Foldout> vars;

    //private List<Toggle> sectionToggles;
    private List<VisualElement> Sections;
    private MaskField maskField;
    private List<string> sectionNames = new List<string>();
// variables 
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        visualTree.CloneTree(root);
        var allElements = root.Q<VisualElement>();

        
        dToggle = root.Q<Toggle>("ToggleDebug");
        vToggle = root.Q<Toggle>("ToggleVariables");
        debugVars = allElements.Query<Foldout>().Where(element => element.name.Contains("Debug")).ToList();
        vars = allElements.Query<Foldout>().Where(element => element.name.Contains("Vars")).ToList();
        
        Sections = allElements.Query().Where(element => element.name.Contains("Section")).ToList();

        vToggle.value = true;
        
        GetSectionNames();

        maskField = root.Q<MaskField>("InfoShown");
        maskField.choices = sectionNames;

        var initialValue = (1 << sectionNames.Count) - 1; 
        
        maskField.value = initialValue; 
        
        
        UpdateDebugMode(dToggle.value);
        UpdateVariableMode(vToggle.value);
        
        maskField.RegisterValueChangedCallback(OnMaskFieldChange );
        dToggle.RegisterValueChangedCallback(evt => { UpdateDebugMode(evt.newValue); });
        vToggle.RegisterValueChangedCallback(evt => { UpdateVariableMode(evt.newValue); });
        return root;
    }

    private void OnMaskFieldChange(ChangeEvent<int> evt)
    {
        var selected = evt.newValue ;
        // Debug.Log(selected);
        for (var i = 0; i < Sections.Count; i++)
        {
            var section = Sections[i];
        // Debug.Log(selected & (1 << i));
            section.style.display = (selected & (1 << i)) != 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void GetSectionNames()
    {
        foreach (var element in Sections.Where(element => !sectionNames.Contains(element.name)))
        {
            sectionNames.Add(element.name);
            
        }
    }

    private void UpdateDebugMode(bool isDebugMode)
    {
        foreach (var debugSection in debugVars)
        {
            debugSection.style.display = isDebugMode ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void UpdateVariableMode(bool isVariableMode)
    {
        foreach (var var in vars)
        {
            var.style.display = isVariableMode ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}