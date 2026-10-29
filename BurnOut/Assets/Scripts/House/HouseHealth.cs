using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HouseHealth : MonoBehaviour, IDamageable
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
        SceneManager.LoadScene(2);
    }
}
