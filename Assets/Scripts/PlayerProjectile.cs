using System;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public AttackData _data;
    public Vector3 Direction;
    public float Speed;
    private void Start()
    {
        Speed = 10;
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
