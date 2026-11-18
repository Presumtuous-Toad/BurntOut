using UnityEngine;

public class DmgArea : MonoBehaviour
{
    [SerializeField] private ParticleSystem hoseParticles;


    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IDamageable damegable) && hoseParticles.isEmitting)
        {
            Debug.Log("Trying to damage " + damegable.ToString());
            damegable.TakeDamage(1);
        }
    }
}
