using UnityEngine;

/**
 * @file: FlickeringLightOnNoiseEvent.cs
 * @brief: Hace que una luz parpadee cuando se detecta ruido en su proximidad usando un NoiseDetector.
 *
 * Notas:
 * - La intensidad de la luz varía entre baseIntensity y maxIntensity utilizando Perlin Noise.
 * - Solo reacciona si el ruido ocurre dentro de reactionRadius.
 * - Se puede configurar flickerDuration y flickerSpeed para ajustar la duración y frecuencia del parpadeo.
 * - Si controlledLight no se asigna en el Inspector, se busca automáticamente en el mismo GameObject.
 */
public class FlickeringLightOnNoiseEvent : MonoBehaviour
{
    [Header("Reaction")]
    public float reactionRadius = 5f;
    public float flickerDuration = 3f;
    public float flickerSpeed = 50f;

    [Header("Light Settings")]
    public Light controlledLight;
    public float maxIntensity = 10f;

    private float baseIntensity;
    private float flickerTimer = 0f;
    private bool flickering = false;

    void Start()
    {
        if (controlledLight == null)
            controlledLight = GetComponent<Light>();

        baseIntensity = controlledLight.intensity;
    }

    void OnEnable()  => NoiseDetector.OnAnyNoiseDetected += OnNoiseHeard;
    void OnDisable() => NoiseDetector.OnAnyNoiseDetected -= OnNoiseHeard;

    void Update()
    {
        if (!flickering) return;

        flickerTimer -= Time.deltaTime;

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        controlledLight.intensity = Mathf.Lerp(0, maxIntensity, noise);

        if (flickerTimer <= 0f)
        {
            flickering = false;
            controlledLight.intensity = baseIntensity;
        }
    }

    void OnNoiseHeard(Vector3 noisePos, float intensity)
    {
        float distance = Vector3.Distance(transform.position, noisePos);
        if (distance > reactionRadius) return;

        flickering = true;
        flickerTimer = flickerDuration;
    }
}