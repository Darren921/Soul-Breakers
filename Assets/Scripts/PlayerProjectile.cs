using System;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public PlayerController owner; 
    public AttackData _data;
    public Vector3 Direction;
    [SerializeField] private float Speed;
    private void Start()
    {
        Speed = 15;
        Direction = !owner.Reversed ? Vector3.right :  Vector3.left;
    }

    private void Update()
    {
        transform.Translate(Direction * (Speed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HurtBox"))
        {
            Destroy(gameObject, 0.001f);
        }
    }
}
