using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "CharacterSO", menuName = "Scriptable Objects/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    public CharacterAttacksSo characterAttacks;
    public float normGravScale;
    public float FallingGravScale;

    // All character data is here, add and remove as needed 
    
    [Header ("Health")]
    public int health;
    [Header ("Misc")]
    public string characterName;
    [Header("Walk and Run Speed")]
    public int walkSpeed;
    public int runSpeed;
    
    
    public float jumpHeight;
    public int airDashCharges;
    public int jumpCharges;

    public float dashDistance;
    public float dashTime; 
    public float dashVertHeight;
    
    
    private void OnValidate()
    {
         characterAttacks = Resources.FindObjectsOfTypeAll<CharacterAttacksSo>().ToList().Find(so =>   so.name.Contains(characterName));
    }
}