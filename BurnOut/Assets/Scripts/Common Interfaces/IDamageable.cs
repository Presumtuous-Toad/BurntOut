using UnityEngine;

/// <summary>
/// IDamageable Interface is applied to entities that have health and can take damage.
/// </summary>
public interface IDamageable
{
    public int Health { get; set; }

    void TakeDamage(int damage);
}
