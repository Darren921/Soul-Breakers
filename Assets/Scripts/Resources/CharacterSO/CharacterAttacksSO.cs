using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CharacterAttacksSO", menuName = "Scriptable Objects/CharacterAttacksSO")]
    public class CharacterAttacksSo : ScriptableObject
    {
        #region DefaultAttacks
        public AttackData[] DefaultLightAttacks = {
            new (new InputReader.Attack(InputReader.AttackType.Light)),
            new (new InputReader.Attack(InputReader.AttackType.Light), AttackData.Tags.Low , AttackData.States.Crouching ),
            new (new InputReader.Attack(InputReader.AttackType.Light), AttackData.Tags.High , AttackData.States.Airborne)
        };
        public AttackData[] DefaultMedAttacks = 
        {
            new (new InputReader.Attack(InputReader.AttackType.Medium)),
            new (new InputReader.Attack(InputReader.AttackType.Medium), AttackData.Tags.Low , AttackData.States.Crouching ),
            new (new InputReader.Attack(InputReader.AttackType.Medium), AttackData.Tags.High , AttackData.States.Airborne)
        };
        public AttackData[] DefaultHeavyAttacks = 
        {
            new (new InputReader.Attack(InputReader.AttackType.Heavy)),
            new (new InputReader.Attack(InputReader.AttackType.Heavy), AttackData.Tags.Low , AttackData.States.Crouching ),
            new (new InputReader.Attack(InputReader.AttackType.Heavy), AttackData.Tags.High , AttackData.States.Airborne)
        };

        

        #endregion
        
        public List<AttackData> Attacks;
       
        public AttackData ReturnAttackData(InputReader.Attack attack, AttackData.States state)
        { 
//            Debug.Log(attack.Type);
//            Debug.Log(state);
            var attackUsed = Attacks.Find( data => data.Attack.Move == attack.Move && (attack.Type & data.Attack.Type) == attack.Type && data.State == state) ;
            if (attackUsed.Equals(new AttackData()))
            {
//                Debug.Log(attackUsed.Attack.Type);
                attackUsed = attack.Type switch
                {
                    InputReader.AttackType.Light => DefaultLightAttacks.FirstOrDefault((data => data.State == state)),
                    InputReader.AttackType.Medium => DefaultMedAttacks.FirstOrDefault((data => data.State == state)),
                    InputReader.AttackType.Heavy => DefaultHeavyAttacks.FirstOrDefault((data => data.State == state)),
                    InputReader.AttackType.Grab =>  DefaultLightAttacks.FirstOrDefault((data => data.State == state)),
                    _ => throw new ArgumentOutOfRangeException(nameof(attack),"check the following" )
                };
            }
            return attackUsed; 
        }
    }



    [Serializable]
    public struct AttackData : IEquatable<AttackData>
    { 
        public enum Tags
        {
            Low,
            Mid,
            High,
        }
        public enum States
        {
            Standing,
            Airborne, 
            Crouching,
            Invulnerable,
        }
        public enum AttackTags
        {
            UnBlockableAir,
            UnBlockableGround,
        }

        public InputReader.Attack Attack;
        public Tags Tag;
        public States State;
        public float Damage;
        public Vector3 Knockback;
        public float HitStun;
        public float BlockStun;
        public string AnimationName; 
        private int _animHash; 
        public int AnimHash 
        {
            get 
            {
                if (_animHash == 0 && !string.IsNullOrEmpty(AnimationName))
                {
                    _animHash = Animator.StringToHash(AnimationName);
                }
                return _animHash;
            }
        }
        public AttackData( InputReader.Attack attack , Tags tag = Tags.Mid, States state = States.Standing, float damage = 0, Vector3 knockback = new(),    float hitStun = 0, float blockStun = 0, string animName = "" )
        {
            Attack = attack;
            Tag = tag;
            State = state;
            AnimationName = animName.ToLower();
            _animHash = 0; 
            Damage = damage;
            Knockback = knockback;
            HitStun = hitStun;
            BlockStun = blockStun;
        }
        public bool Equals(AttackData other)
        {
            return Attack.Equals(other.Attack) && Tag == other.Tag && Damage.Equals(other.Damage) && Knockback.Equals(other.Knockback) && HitStun.Equals(other.HitStun) && BlockStun.Equals(other.BlockStun) && State.Equals(other.State);
        }

        public override bool Equals(object obj)
        {
            return obj is AttackData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Attack, (int)Tag, Damage, Knockback, HitStun, BlockStun,State);
        }

       

  
}