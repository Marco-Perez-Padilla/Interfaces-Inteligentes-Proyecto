using UnityEngine;

/**
 * @class CartMovement
 * @brief Controla el movimiento 3D de la vagoneta.
 *
 * Incluye:
 * - Yaw (dirección)
 * - Roll (curvas)
 * - Pitch (pendientes)
 * - Velocidad dependiente de pendiente
 */
public class CartMovement : MonoBehaviour
{
    [Header("References")]
    public PathGenerator pathGenerator;
    public CameraShake cameraShake;

    [Header("Base Movement")]
    public float baseSpeed = 2f;
    public float rotationSmooth = 6f;

    [Header("Speed by Slope")]
    public float slopeAcceleration = 2f;
    public float minSpeed = 1f;
    public float maxSpeed = 6f;

    [Header("Roll Settings")]
    public float maxRollAngle = 8f;
    public float rollSmooth = 6f;

    [Header("Pitch Settings")]
    public float maxPitchAngle = 10f;
    public float pitchSmooth = 5f;

    private int index = 1;
    private float currentSpeed;
    private Vector3 lastDir;
    private float roll;
    private float pitch;

    void Start()
    {
        transform.position = pathGenerator.pathPoints[0];
        lastDir = transform.forward;
        currentSpeed = baseSpeed;
    }

    void Update()
    {
        if (index >= pathGenerator.pathPoints.Count) return;

        Vector3 target = pathGenerator.pathPoints[index];
        Vector3 dir = (target - transform.position).normalized;

        UpdateSpeed(dir);
        UpdateRotation(dir);

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            currentSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
            index++;

        lastDir = dir;

        UpdateCameraVibration(dir);
    }

    void UpdateSpeed(Vector3 dir)
    {
        currentSpeed += -dir.y * slopeAcceleration * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);
    }

    void UpdateRotation(Vector3 dir)
    {
        Quaternion yaw = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z));

        float targetPitch = -Mathf.Asin(dir.y) * Mathf.Rad2Deg;
        targetPitch = Mathf.Clamp(targetPitch, -maxPitchAngle, maxPitchAngle);
        pitch = Mathf.Lerp(pitch, targetPitch, pitchSmooth * Time.deltaTime);

        float curve = Vector3.SignedAngle(lastDir, dir, Vector3.up) / 45f;
        float targetRoll = -Mathf.Clamp(curve, -1f, 1f) * maxRollAngle;
        roll = Mathf.Lerp(roll, targetRoll, rollSmooth * Time.deltaTime);

        transform.rotation = yaw * Quaternion.Euler(pitch, 0f, roll);
    }

    void UpdateCameraVibration(Vector3 dir)
    {
        if (cameraShake == null || !cameraShake.enableShake) return;

        float curvature = Mathf.Abs(Vector3.SignedAngle(lastDir, dir, Vector3.up)) / 90f;
        float speedFactor = currentSpeed / maxSpeed;

        cameraShake.dynamicFactor = Mathf.Clamp01(curvature + speedFactor);
    }
}
