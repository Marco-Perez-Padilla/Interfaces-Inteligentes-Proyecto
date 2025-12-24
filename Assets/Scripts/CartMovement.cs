using UnityEngine;

/**
 * @class CartMovement
 * @brief Controla movimiento, rotación, vibración e inclinación (roll) de la vagoneta.
 *
 * - Sigue un camino punto a punto
 * - Rota hacia la dirección de avance (yaw)
 * - Aplica inclinación lateral en curvas (roll)
 * - Informa a la cámara de curvatura y velocidad
 */
public class CartMovement : MonoBehaviour
{
    // =========================
    // REFERENCIAS
    // =========================

    [Header("References")]
    public PathGenerator pathGenerator;
    public CameraShake cameraShake;

    // =========================
    // MOVIMIENTO
    // =========================

    [Header("Movement Settings")]
    public float speed = 2f;
    public float rotationSpeed = 6f;

    // =========================
    // VIBRACIÓN
    // =========================

    [Header("Vibration Settings")]
    public float curveInfluence = 1.2f;
    public float speedInfluence = 0.6f;

    // =========================
    // ROLL (INCLINACIÓN)
    // =========================

    [Header("Roll Settings")]
    [Tooltip("Ángulo máximo de inclinación lateral")]
    public float maxRollAngle = 8f;

    [Tooltip("Velocidad de interpolación del roll")]
    public float rollSmoothness = 6f;

    // =========================
    // ESTADO INTERNO
    // =========================

    private int currentIndex = 0;
    private Vector3 lastDirection;
    private float currentRoll = 0f;

    /**
     * @brief Inicializa la vagoneta en el primer punto del camino.
     */
    void Start()
    {
        if (pathGenerator != null && pathGenerator.pathPoints.Count > 0)
        {
            transform.position = pathGenerator.pathPoints[0];
            currentIndex = 1;
        }

        lastDirection = transform.forward;
    }

    /**
     * @brief Actualiza movimiento, rotación, vibración y roll.
     */
    void Update()
    {
        if (pathGenerator == null) return;
        if (currentIndex >= pathGenerator.pathPoints.Count) return;

        MoveRotateAndRoll();
        UpdateCameraVibration();
    }

    /**
     * @brief Movimiento y rotación principal de la vagoneta.
     */
    void MoveRotateAndRoll()
    {
        Vector3 target = pathGenerator.pathPoints[currentIndex];
        Vector3 direction = (target - transform.position).normalized;

        // =====================
        // YAW (rotación base)
        // =====================
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetYaw = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetYaw,
                rotationSpeed * Time.deltaTime
            );
        }

        // =====================
        // ROLL (inclinación)
        // =====================

        float signedAngle = Vector3.SignedAngle(lastDirection, direction, Vector3.up);

        // Curvatura normalizada (-1 a 1)
        float curveFactor = Mathf.Clamp(signedAngle / 45f, -1f, 1f);

        float targetRoll = -curveFactor * maxRollAngle;

        currentRoll = Mathf.Lerp(
            currentRoll,
            targetRoll,
            rollSmoothness * Time.deltaTime
        );

        transform.rotation *= Quaternion.Euler(0f, 0f, currentRoll);

        // =====================
        // MOVIMIENTO
        // =====================

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            currentIndex++;
        }

        lastDirection = direction;
    }

    /**
     * @brief Ajusta la vibración según curvatura y velocidad.
     */
    void UpdateCameraVibration()
    {
        if (cameraShake == null || !cameraShake.enableShake)
            return;

        float curvature = Vector3.Angle(lastDirection, transform.forward) / 90f;
        curvature = Mathf.Clamp01(curvature);

        float speedFactor = Mathf.Clamp01(speed / 5f);

        cameraShake.dynamicFactor =
            curvature * curveInfluence +
            speedFactor * speedInfluence;
    }
}