using UnityEngine;

/**
 * @class CameraShake
 * @brief Vibración de cámara usando ruido Perlin.
 */
public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public bool enableShake = false;
    public float baseIntensity = 0.02f;
    public float frequency = 18f;

    [Range(0f, 1f)]
    public float dynamicFactor;

    private Vector3 originalLocalPos;
    private float time;

    void Start()
    {
        originalLocalPos = transform.localPosition;
    }

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
        float y = (Mathf.PerlinNoise(0f, time) - 0.5f) * intensity;

        transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);
    }
}
