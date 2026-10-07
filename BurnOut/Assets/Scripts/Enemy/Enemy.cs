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

    [field: Header("Damage Settings")]
    [field: SerializeField] public float dmgCooldown { get; set; } 
    [field: SerializeField] public float timeSinceDmg { get; set; }
    // This can be changed to IHealth Interface if we have entities that need 
    [field: SerializeField] public int Health { get; set; } 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        timeSinceDmg = dmgCooldown;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        timeSinceDmg += Time.deltaTime;
    }

    // Interaction with the water hose and taking damage needs to be made.

    public void TakeDamage(int damage)
    {
        if (state == EnemyState.Alive && timeSinceDmg > dmgCooldown) 
        {
            Health -= damage;
            timeSinceDmg = 0;
            Debug.Log("TOOK DAMAGE");
        }

        if (Health <= 0)
        {
            state = EnemyState.Dead;
            gameObject.SetActive(false);
        }
    }
}
