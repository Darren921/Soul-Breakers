using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CharacterAttacksSO", menuName = "Scriptable Objects/CharacterAttacksSO")]
[Serializable]
    public class CharacterAttacksSo : ScriptableObject
    {
        private void OnValidate()
        {
            foreach (var attackData in SuperAttacks)
            {
                var data = attackData;
                data.IsSpecial = true;
            }
        }

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
        public AttackData[] GrabAttacks = 
        {
            new (new InputReader.Attack(InputReader.AttackType.Grab)),
            new (new InputReader.Attack(InputReader.AttackType.Grab)),
        };
        public AttackData[] SpecialAttacks;
        public AttackData[] SuperAttacks ;
        #endregion
        public AttackData[] CustomLightAttacks;
        public AttackData[] CustomMedAttacks;
        public AttackData[] CustomHeavyAttacks;

        public AttackData ReturnAttackData(InputReader.Attack attack, AttackData.States state)
        { 
 //         Debug.Log(attack.Type); 
//          Debug.Log(state);
            var attackUsed = attack.Type switch
            {
                InputReader.AttackType.Light => CustomLightAttacks.FirstOrDefault(data => data.Attack.Move == attack.Move && (attack.Type & data.Attack.Type) == attack.Type && data.State == state),
                InputReader.AttackType.Medium => CustomMedAttacks.FirstOrDefault(data => data.Attack.Move == attack.Move && (attack.Type & data.Attack.Type) == attack.Type && data.State == state),
                InputReader.AttackType.Heavy => CustomHeavyAttacks.FirstOrDefault(data => data.Attack.Move == attack.Move && (attack.Type & data.Attack.Type) == attack.Type && data.State == state),
                InputReader.AttackType.Grab => GrabAttacks.FirstOrDefault(data => (attack.Type & data.Attack.Type) == attack.Type && data.Attack.Move == attack.Move && data.State == state),
                InputReader.AttackType.Special => SpecialAttacks.FirstOrDefault(data => (attack.Type & data.Attack.Type) == attack.Type && data.Attack.Move == attack.Move && data.State == state),
                InputReader.AttackType.SuperLight or InputReader.AttackType.SuperMedium or InputReader.AttackType.SuperHeavy => SuperAttacks.FirstOrDefault(data => (attack.Type & data.Attack.Type) == attack.Type && data.Attack.Move == attack.Move),
                _ => new AttackData()  
            };
            if (attackUsed.Equals(new AttackData()))
            {
//              Debug.Log(attackUsed.Attack.Type);
 //             Debug.Log(state);
   //           Debug.Log(attack.Type);
                attackUsed = attack.Type switch
                {
                    InputReader.AttackType.Light => DefaultLightAttacks.FirstOrDefault(data => data.State == state),
                    InputReader.AttackType.Medium => DefaultMedAttacks.FirstOrDefault(data => data.State == state),
                    InputReader.AttackType.Heavy => DefaultHeavyAttacks.FirstOrDefault(data => data.State == state),
                    InputReader.AttackType.Special => SpecialAttacks.FirstOrDefault(data => data.State == state),
                    InputReader.AttackType.Grab => GrabAttacks.FirstOrDefault(data => data.State == state),
                    InputReader.AttackType.SuperLight or InputReader.AttackType.SuperMedium or InputReader.AttackType.SuperHeavy => SuperAttacks.FirstOrDefault(data => (attack.Type & data.Attack.Type) == attack.Type &&  data.State == state),

                    _ => throw new ArgumentOutOfRangeException(nameof(attack),"check the following" )
                };
            }
//            Debug.Log($"{SpecialAttacks.FirstOrDefault(data => data.State == state).State} + {SpecialAttacks.FirstOrDefault(data => data.State == state ).AnimationName}") ;
//          Debug.Log(attackUsed.Attack.Type);
            if (attackUsed.AnimationName == string.Empty) Debug.LogWarning("No animation found");
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
        [Serializable]
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
        public bool IsSpecial;

        public float SuperAttackCharge;
        public float SuperChargeNeeded;

        
        public int AnimHash 
        {
            get 
            {
                if (_animHash == 0 && !string.IsNullOrEmpty(AnimationName))
                {
                    Debug.Log(AnimationName);
                    _animHash = Animator.StringToHash(AnimationName);
                    
                }
                return _animHash;
            }
        }
        public AttackData( InputReader.Attack attack , Tags tag = Tags.Mid, States state = States.Standing, float damage = 0, Vector3 knockback = new(),    float hitStun = 0, float blockStun = 0, string animName = "" ,float superAttackCharge = 10 , float superChargeNeeded = 0, bool isSpecial = false) 
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
            SuperAttackCharge = superAttackCharge;
            SuperChargeNeeded = superChargeNeeded;
            IsSpecial = isSpecial;
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