using UnityEngine;

/**
 * @class CameraShake
 * @brief Aplica vibración de cámara dependiente de intensidad externa.
 *
 * La intensidad puede ser modificada dinámicamente (curvas, velocidad, eventos).
 * El efecto se aplica en espacio local usando ruido Perlin.
 */
public class CameraShake : MonoBehaviour
{
    // =========================
    // CONFIGURACIÓN BASE
    // =========================

    /** @brief Activa o desactiva la vibración */
    [Header("Base Settings")]
    public bool enableShake = false;

    /** @brief Intensidad base de la vibración */
    public float baseIntensity = 0.02f;

    /** @brief Frecuencia base */
    public float frequency = 18f;

    // =========================
    // MODULADORES
    // =========================

    /** @brief Intensidad dinámica (0–1) recibida externamente */
    [Range(0f, 1f)]
    public float dynamicFactor = 0f;

    // =========================
    // ESTADO INTERNO
    // =========================

    private Vector3 originalLocalPosition;
    private float noiseTime;

    /**
     * @brief Guarda la posición local inicial.
     */
    void Start()
    {
        originalLocalPosition = transform.localPosition;
    }

    /**
     * @brief Permite resetear el punto base al cambiar de parent.
     */
    public void ResetOriginalPosition()
    {
        originalLocalPosition = transform.localPosition;
    }

    /**
     * @brief Actualiza la vibración de cámara.
     */
    void LateUpdate()
    {
        if (!enableShake)
        {
            transform.localPosition = originalLocalPosition;
            return;
        }

        noiseTime += Time.deltaTime * frequency;

        float intensity = baseIntensity * (1f + dynamicFactor);

        float x = (Mathf.PerlinNoise(noiseTime, 0f) - 0.5f) * intensity;
        float y = (Mathf.PerlinNoise(0f, noiseTime) - 0.5f) * intensity;

        transform.localPosition = originalLocalPosition + new Vector3(x, y, 0f);
    }

    /**
     * @brief Asegura que no queda desplazada al desactivar.
     */
    void OnDisable()
    {
        transform.localPosition = originalLocalPosition;
    }
}
