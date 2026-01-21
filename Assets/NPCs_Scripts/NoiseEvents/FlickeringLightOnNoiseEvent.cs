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
    [Header("References")]
    public NoiseDetector noiseDetector;      // Detector de ruido al que se suscribe la luz

    [Header("Reaction")]
    public float reactionRadius = 5f;        // Radio de reacción al ruido
    public float flickerDuration = 1.5f;    // Duración del parpadeo en segundos
    public float flickerSpeed = 20f;        // Velocidad del parpadeo (Perlin Noise)

    [Header("Light Settings")]
    public Light controlledLight;           // Luz que será controlada
    public float baseIntensity = 1f;        // Intensidad normal de la luz
    public float maxIntensity = 3f;         // Intensidad máxima durante el parpadeo

    private float flickerTimer = 0f;        // Temporizador del parpadeo
    private bool flickering = false;        // Estado de parpadeo

    // Suscripción al evento de ruido al activar el script
    void OnEnable()
    {
        if (noiseDetector != null)
            noiseDetector.OnNoiseDetected += OnNoiseHeard;
    }

    // Desuscripción al evento al desactivar el script
    void OnDisable()
    {
        if (noiseDetector != null)
            noiseDetector.OnNoiseDetected -= OnNoiseHeard;
    }

    // Inicialización
    void Awake()
    {
        if (controlledLight == null)
            controlledLight = GetComponent<Light>();   // Busca automáticamente la luz si no se asignó

        baseIntensity = controlledLight.intensity;    // Guardar intensidad base
    }

    // Actualización cada frame para manejar el parpadeo
    void Update()
    {
        if (!flickering)
            return;

        flickerTimer -= Time.deltaTime;

        // Parpadeo utilizando Perlin Noise para variar suavemente la intensidad
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        controlledLight.intensity = Mathf.Lerp(baseIntensity, maxIntensity, noise);

        // Fin del parpadeo
        if (flickerTimer <= 0f)
        {
            flickering = false;
            controlledLight.intensity = baseIntensity;
        }
    }

    // Callback cuando se detecta ruido.
    public void OnNoiseHeard(Vector3 noisePos, float intensity)
    {
        float distance = Vector3.Distance(transform.position, noisePos);
        if (distance > reactionRadius)
            return;

        flickering = true;
        flickerTimer = flickerDuration;
    }
}
