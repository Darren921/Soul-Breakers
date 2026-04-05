using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

[CustomPropertyDrawer(typeof(AttackData))]
public class CharacterAttackDrawer : PropertyDrawer
{
    private bool copyField;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {


        var root = new VisualElement();
        
        var StandardAttackVaris = new VisualElement();
        StandardAttackVaris.Add(new PropertyField(property.FindPropertyRelative("Attack"), "Attack"));
        StandardAttackVaris.Add(new PropertyField(property.FindPropertyRelative("Damage"), "Damage"));
        StandardAttackVaris.Add(new PropertyField(property.FindPropertyRelative("Tag"), "State Tag"));
        StandardAttackVaris.Add(new PropertyField(property.FindPropertyRelative("State"), "State"));
        StandardAttackVaris.Add(new PropertyField(property.FindPropertyRelative("Knockback"), "Knockback"));
        StandardAttackVaris.Add(new PropertyField(property.FindPropertyRelative("HitStun"), "HitStun"));
        StandardAttackVaris.Add(new PropertyField(property.FindPropertyRelative("BlockStun"), "BlockStun"));
        StandardAttackVaris.Add(new PropertyField(property.FindPropertyRelative("HitStop"), "HitStop"));
      
        root.Add(StandardAttackVaris);
        
        
        var animationVari = new VisualElement();
        animationVari.Add(new PropertyField(property.FindPropertyRelative("AnimationName"), "AnimationName"));
        animationVari.Add(new PropertyField(property.FindPropertyRelative("_animHash"), "_animHash"));
        root.Add(animationVari);

      
        
        var moveProp = property.FindPropertyRelative("HasMovement");
        
        root.Add(new PropertyField(moveProp, "has movement"));
        var movementSettings = new PopupWindow();
        movementSettings.Add(new PropertyField(property.FindPropertyRelative("IsBackwards"), "Reversed?"));
        movementSettings.Add(new PropertyField(property.FindPropertyRelative("MovementForce"), "move force "));
        movementSettings.Add(new PropertyField(property.FindPropertyRelative("MovementTime"), "move Time"));
        root.Add(movementSettings);
        
        var specialProp = property.FindPropertyRelative("IsSpecial");
        root.Add(new PropertyField(specialProp, "isSpecial"));
        
        var specialSettings = new VisualElement(); 
        var SpecialChargeNeeded =new PropertyField(property.FindPropertyRelative("SuperChargeNeeded"),"SuperChargeNeeded") ;
        var SpecialChargeGen = new PropertyField(property.FindPropertyRelative("SuperAttackCharge"), "superAttackCharge"); 
        specialSettings.Add(SpecialChargeNeeded);
        specialSettings.Add(SpecialChargeGen);
        root.Add(specialSettings);
        
        
       
        
       
        
    
        root.TrackPropertyValue(moveProp, (moveProperty) => movementSettings.style.display = moveProperty.boolValue ? DisplayStyle.Flex : DisplayStyle.None);
        root.TrackPropertyValue(specialProp, serializedProperty =>
        {
            SpecialChargeGen.style.display =  serializedProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
            SpecialChargeNeeded.style.display =   serializedProperty.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            
        });
        UpdateSpecialSettings(SpecialChargeGen, specialProp, SpecialChargeNeeded);
        movementSettings.style.display = moveProp.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
        return root;

    }

    private  void UpdateSpecialSettings(PropertyField SpecialChargeGen, SerializedProperty specialProp, PropertyField SpecialChargeNeeded)
    {
        SpecialChargeGen.style.display =  specialProp.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
        SpecialChargeNeeded.style.display =   specialProp.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
    }

   
}
