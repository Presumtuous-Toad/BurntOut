using UnityEngine;

public class ParticleDamager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Fire Monster") || other.CompareTag("Enemy"))
        {
            print("Particle Hit " + other.name);
            Enemy enemyRef = other.GetComponent<Enemy>();
            enemyRef.TakeDamage(1);
        }
    }
}
