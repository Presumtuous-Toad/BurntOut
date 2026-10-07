using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    private int currentHealth;
    public Slider healthBar;

    public int Health { get => currentHealth; set => currentHealth=value; }

    private void Start()
    {
        currentHealth = maxHealth;

        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth;

        // health below 0 play death event 
        if (currentHealth < 0)
        {
            Death();
        }
    }

    public void Death()
    {
        // TO DO: Load Death canvas, start menu, etc 
    }

    private void OnCollisionEnter(Collision collision)
    {
        // determine collision type
        if(collision.gameObject.CompareTag("Enemy"))
        {
            int damage = collision.gameObject.GetComponent<Enemy>().DealDamage();
            TakeDamage(damage);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // determine collision type
        if (collision.gameObject.CompareTag("Enemy"))
        {
            int damage = collision.gameObject.GetComponent<Enemy>().DealDamage();
            if (damage > 0) TakeDamage(damage);
        }
    }

}
