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

    /*public InputReader.Attack Attack;
    public Tags Tag;
    public States State;
    public float Damage;
    public Vector3 Knockback;
    public float HitStun;
    public float BlockStun;
    public string AnimationName; 
    private int _animHash;
    public bool isSpecial;

    public float SuperAttackCharge;
    public float SuperChargeNeeded;*/

        var root = new VisualElement();
        root.Add(new PropertyField(property.FindPropertyRelative("Attack"), "Attack"));
        root.Add(new PropertyField(property.FindPropertyRelative("Damage"), "Damage"));
        root.Add(new PropertyField(property.FindPropertyRelative("Tag"), "State Tag"));
        root.Add(new PropertyField(property.FindPropertyRelative("Knockback"), "Knockback"));
        root.Add(new PropertyField(property.FindPropertyRelative("HitStun"), "HitStun"));
        root.Add(new PropertyField(property.FindPropertyRelative("BlockStun"), "BlockStun"));
        root.Add(new PropertyField(property.FindPropertyRelative("AnimationName"), "AnimationName"));
        root.Add(new PropertyField(property.FindPropertyRelative("_animHash"), "_animHash"));
   //     popup.Add(new PropertyField(property.FindPropertyRelative("isSpecial"), "isSpecial"));
        if (property.FindPropertyRelative("IsSpecial").boolValue)
        {
            root.Add(new PropertyField(property.FindPropertyRelative("SuperChargeNeeded"), "SuperChargeNeeded"));
        }
        else
        {
            root.Add(new PropertyField(property.FindPropertyRelative("SuperAttackCharge"), "SuperAttackCharge"));
        }

        /*
        var iterator = property.serializedObject.GetIterator();
        if (iterator.NextVisible(true))
        {
            while (iterator.NextVisible(true))
            {
                Debug.Log(iterator.propertyPath);
               
                    if (iterator.propertyPath == "isSpecial")
                    {
                        if (iterator.boolValue)
                        {
                            copyField = iterator.propertyPath != "SuperAttackCharge";
                        }
                        else
                        {
                            copyField = iterator.propertyPath != "SuperChargeNeeded";

                        }
                    }
                    
                if (copyField)
                {
                    var field = new PropertyField(iterator.Copy());
                    root.Add(field);
                }
            }
        }
        */

        return root;

    }
   
}
