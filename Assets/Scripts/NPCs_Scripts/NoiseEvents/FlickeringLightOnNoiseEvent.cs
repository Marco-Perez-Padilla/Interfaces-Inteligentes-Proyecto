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

    /// <summary>
    /// Configura la luz controlada y almacena su intensidad base para poder restaurarla después del parpadeo. Si no se asigna una luz en el Inspector, intenta obtener una del mismo GameObject.
    /// Esto asegura que el script funcione correctamente incluso si el desarrollador olvida asignar la luz, evitando errores y permitiendo que el parpadeo se realice con la luz predeterminada del objeto.
    /// </summary>
    void Start()
    {
        if (controlledLight == null)
            controlledLight = GetComponent<Light>();

        baseIntensity = controlledLight.intensity;
    }

    /// <summary>
    /// Suscribe al método OnNoiseHeard al evento OnAnyNoiseDetected del NoiseDetector.
    /// </summary>
    void OnEnable()  => NoiseDetector.OnAnyNoiseDetected += OnNoiseHeard;
    /// <summary>
    /// Desuscribe al método OnNoiseHeard del evento OnAnyNoiseDetected del NoiseDetector.
    /// </summary>
    void OnDisable() => NoiseDetector.OnAnyNoiseDetected -= OnNoiseHeard;

    /// <summary>
    /// Actualiza el estado de la luz parpadeante según el temporizador y la intensidad de ruido.
    /// </summary>
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

    /// <summary>
    /// Maneja el evento de ruido detectado. Si el ruido ocurre dentro del radio de reacción, inicia el parpadeo de la luz.
    /// </summary>
    void OnNoiseHeard(Vector3 noisePos, float intensity)
    {
        float distance = Vector3.Distance(transform.position, noisePos);
        if (distance > reactionRadius) return;

        flickering = true;
        flickerTimer = flickerDuration;
    }
}