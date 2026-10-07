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

    // related to enemy dealing damage to player 
    public int damage;
    public float dealDamageCooldown = 0.2f;         
    private float currentDealDamageCooldown = 0; 

    public EnemyState state = EnemyState.Alive;

    // This can be changed to IHealth Interface if we have entities that need 
    public int Health { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    protected void Update()
    {
        float deltaTime = Time.deltaTime;

        if(currentDealDamageCooldown > 0)
        {
            currentDealDamageCooldown -= deltaTime;
        }
    }

    // Interaction with the water hose and taking damage needs to be made.

    public void TakeDamage(int damage)
    {
        if (state == EnemyState.Alive)
            Health -= damage;
    }

    public int DealDamage()
    {
        if(currentDealDamageCooldown <= 0)
        {
            currentDealDamageCooldown = dealDamageCooldown;
            return damage;
        }

        return 0;
    }
}
