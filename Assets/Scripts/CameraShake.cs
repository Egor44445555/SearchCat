using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [System.Serializable]
    public class ShakeProfile
    {
        public float duration = 0.5f;
        public float magnitude = 0.1f;
        public float frequency = 10f;
    }
    
    public ShakeProfile hitShake;
    Vector3 originalPosition;
    
    void Start()
    {
        originalPosition = transform.localPosition;
    }
    
    public void StartHitShake()
    {
        originalPosition = transform.localPosition;
        StartCoroutine(DoShake(hitShake));
    }
    
    public void StopShake()
    {
        StopAllCoroutines();
        transform.localPosition = originalPosition;
    }
    
    IEnumerator DoShake(ShakeProfile profile)
    {
        float elapsed = 0f;
        
        while (elapsed < profile.duration)
        {
            float currentMagnitude = profile.magnitude * (1 - (elapsed / profile.duration));            
            float x = Mathf.PerlinNoise(Time.time * profile.frequency, 0) * 2 - 1;
            float y = Mathf.PerlinNoise(0, Time.time * profile.frequency) * 2 - 1;
            
            x *= currentMagnitude;
            y *= currentMagnitude;
            
            transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = originalPosition;
    }
}