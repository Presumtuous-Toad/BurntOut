using UnityEditor;
using UnityEngine;

/// <summary>
/// Enemy Abstract class that every Enemy type should inherit
/// </summary>
public abstract class Enemy : MonoBehaviour, IDamageable
{
    public enum EnemyState
    {
        Alive,
        Dead
    }

    public Vector3 position;

    public EnemyState state = EnemyState.Alive;

    // This can be changed to IHealth Interface if we have entities that need 
    public int Health { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Interaction with the water hose and taking damage needs to be made.

    public void TakeDamage(int damage)
    {
        if (state == EnemyState.Alive)
            Health -= damage;
    }
}
