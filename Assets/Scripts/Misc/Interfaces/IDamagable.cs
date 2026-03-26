using UnityEngine;

public interface IDamageable
{
    void TakeDamage(InputReader.Attack cachedAttack,bool isProjectile);
    
}