using UnityEngine;
using UnityEngine.VFX;

public class FireGrowth : MonoBehaviour
{
    private VisualEffect visualEffect;

    public float Duration;

    public float StartColliderRadius = 0.2f;
    public float EndColliderRadius = 1.75f;
    private SphereCollider fireCollider;

    // paricle modifications over time
    public float StartParticleRadius;
    public float EndParticleRadius;
    public AnimationCurve StartSizeOverLife;
    public AnimationCurve EndSizeOverLife;
    public Vector2 StartLifeRand;
    public Vector2 EndLifeRand;

    private bool isComplete = false;
    private float timeElapsed;
    
    private AnimationCurve currentSizeOverLife;

    void Start()
    {
        visualEffect = GetComponent<VisualEffect>();
        fireCollider = GetComponentInParent<SphereCollider>();

        currentSizeOverLife = new AnimationCurve(StartSizeOverLife.keys);
    }

    void Update()
    {
        if(!isComplete)
        {
            if (timeElapsed < Duration)
            {
                float t = timeElapsed / Duration;
                UpdateColliderRadius(t);
                UpdateSizeOverTime(t);
                UpdateLifeRandom(t);
                UpdateParticleRadius(t);
                timeElapsed += Time.deltaTime;
            }
            else
            {
                fireCollider.radius = EndColliderRadius;
                visualEffect.SetAnimationCurve("SizeOverTime", EndSizeOverLife);
                visualEffect.SetVector2("LifeTimeRand", EndLifeRand);
                visualEffect.SetFloat("SphereRadius", EndParticleRadius);
                isComplete = true;
            }
        }
        else
        {
            // disable script are fully size
            // can remove isComplete and move this but keeping for now
            this.enabled = false;
        }
    }

    private void UpdateColliderRadius(float t)
    {
        float currentRadius = Mathf.Lerp(StartColliderRadius, EndColliderRadius, t);
        fireCollider.radius = currentRadius;
    }

    void UpdateParticleRadius(float t)
    {
        float currentRadius = Mathf.Lerp(StartParticleRadius, EndParticleRadius, t);
        visualEffect.SetFloat("SphereRadius", currentRadius);
    }

    private void UpdateSizeOverTime(float t)
    {
        Keyframe[] keys = currentSizeOverLife.keys; 
        keys[0].value = Mathf.Lerp(StartSizeOverLife[0].value, EndSizeOverLife[0].value, t);
        keys[1].value = Mathf.Lerp(StartSizeOverLife[1].value, EndSizeOverLife[1].value, t);
        currentSizeOverLife.keys = keys;
        visualEffect.SetAnimationCurve("SizeOverTime", currentSizeOverLife);
    }

    private void UpdateLifeRandom(float t)
    {
        Vector2 currentLifeRand = Vector2.Lerp(StartLifeRand, EndLifeRand, t);
        visualEffect.SetVector2("LifeTimeRand", currentLifeRand);
    }
}
