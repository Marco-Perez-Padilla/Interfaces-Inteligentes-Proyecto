using UnityEngine;

public class FlickeringLightOnNoiseEvent : MonoBehaviour
{
    [Header("References")]
    public NoiseDetector noiseDetector;

    [Header("Reaction")]
    public float reactionRadius = 5f;
    public float flickerDuration = 1.5f;
    public float flickerSpeed = 20f;

    [Header("Light Settings")]
    public Light controlledLight;
    public float baseIntensity = 1f;
    public float maxIntensity = 3f;

    private float flickerTimer = 0f;
    private bool flickering = false;

    void OnEnable()
    {
        if (noiseDetector != null)
            noiseDetector.OnNoiseDetected += OnNoiseHeard;
    }

    void OnDisable()
    {
        if (noiseDetector != null)
            noiseDetector.OnNoiseDetected -= OnNoiseHeard;
    }

    void Awake()
    {
        if (controlledLight == null)
            controlledLight = GetComponent<Light>();

        baseIntensity = controlledLight.intensity;
    }

    void Update()
    {
        if (!flickering)
            return;

        flickerTimer -= Time.deltaTime;

        // Flickering with constant noise
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        controlledLight.intensity = Mathf.Lerp(baseIntensity, maxIntensity, noise);

        if (flickerTimer <= 0f)
        {
            flickering = false;
            controlledLight.intensity = baseIntensity;
        }
    }

    public void OnNoiseHeard(Vector3 noisePos, float intensity)
    {
        float distance = Vector3.Distance(transform.position, noisePos);
        if (distance > reactionRadius)
            return;

        flickering = true;
        flickerTimer = flickerDuration;
    }
}
