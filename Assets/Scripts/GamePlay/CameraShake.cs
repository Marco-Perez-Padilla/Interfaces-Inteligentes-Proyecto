using UnityEngine;

<<<<<<< HEAD
/// <summary>
/// Vibración de cámara usando ruido Perlin.
/// El desplazamiento en Y es siempre positivo (hacia arriba) para evitar
/// que la cámara baje por debajo del borde de la vagoneta.
/// </summary>
=======
/**
 * @class CameraShake
 * @brief Vibración de cámara usando ruido Perlin.
 */
>>>>>>> flashlight
public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public bool enableShake = false;
    public float baseIntensity = 0.02f;
    public float frequency = 18f;

    [Range(0f, 1f)]
    public float dynamicFactor;

<<<<<<< HEAD
    /// <summary>Posición local de referencia desde la que se aplica el shake.</summary>
    private Vector3 originalLocalPos;

    /// <summary>Acumulador de tiempo para el ruido Perlin.</summary>
=======
    private Vector3 originalLocalPos;
>>>>>>> flashlight
    private float time;

    void Start()
    {
        originalLocalPos = transform.localPosition;
    }

<<<<<<< HEAD
    /// <summary>
    /// Actualiza la posición local de referencia para el shake.
    /// Llamar antes de activar enableShake para que parta de la posición correcta.
    /// </summary>
=======
>>>>>>> flashlight
    public void ResetOriginalPosition()
    {
        originalLocalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        if (!enableShake)
        {
            transform.localPosition = originalLocalPos;
            return;
        }

        time += Time.deltaTime * frequency;
        float intensity = baseIntensity * (1f + dynamicFactor);

        float x = (Mathf.PerlinNoise(time, 0f) - 0.5f) * intensity;
<<<<<<< HEAD

        // Abs en Y para que el shake solo desplace hacia arriba,
        // nunca hacia abajo, evitando ver por debajo del borde de la vagoneta.
        float y = Mathf.Abs(Mathf.PerlinNoise(0f, time) - 0.5f) * intensity;

        transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);
    }
}
=======
        float y = (Mathf.PerlinNoise(0f, time) - 0.5f) * intensity;

        transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);
    }
}
>>>>>>> flashlight
