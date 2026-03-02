using UnityEngine;

public class DestroyAfterParticles : MonoBehaviour
{
    new ParticleSystem particleSystem;
    AudioSource audioSource;

    void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particleSystem.Play();
        audioSource = GetComponent<AudioSource>();

        if (particleSystem == null)
        {
            particleSystem = GetComponentInChildren<ParticleSystem>();
        }
    }

    void OnEnable()
    {
        if (particleSystem == null)
        {
            particleSystem = GetComponent<ParticleSystem>();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            particleSystem.Clear(true);
            particleSystem.Play();
        }
    }

    void Update()
    {
        if (particleSystem != null && !particleSystem.IsAlive())
        {
            // Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        particleSystem = null;
        audioSource = null;
    }
}