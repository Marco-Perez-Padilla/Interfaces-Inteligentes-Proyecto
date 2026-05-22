using UnityEngine;

/**
 * @file: VisualChasing.cs
 * @brief: Visualiza la persecución de un NPC hacia el jugador.
 *
 * Notas:
 * - La visualización solo es visible en la Scene View.
 * - El color de la representación se asigna automáticamente según el tipo de trigger.
 */

public class VisualChasing : MonoBehaviour
{
    [Header("References")]
    public TriggerNotificator triggerZone;
    public Transform player;

    [Header("Movement")]
    public float npcSpeedWhileChasing = 1.5f;
    public float rotationSpeed = 360f;
    public float viewAngle = 60f;

    [Header("Chase Control")]
    public float chaseDuration = 5f;

    private Rigidbody rb;
    private bool chasing = false;
    private float chaseTimer = 0f;

    /// <summary>
    /// Configura el Rigidbody para que solo gire en el eje Y.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    /// <summary>
    /// Se suscribe a los eventos del TriggerNotificator para iniciar y detener la persecución.
    /// También inicia la persecución inmediatamente si el jugador ya está presente.
    /// </summary>
    void Start()
    {
        if (triggerZone != null)
        {
            triggerZone.OnPlayerEntered += OnPlayerEnteredTrigger;
            if (chaseDuration < 1f)
                triggerZone.OnPlayerExited += StopChase;
        }

        // Fallback
        if (player != null)
            StartChase();
    }

    /// <summary>
    /// Se asegura de desuscribirse de los eventos para evitar errores si el objeto es destruido mientras los eventos aún están activos.
    /// </summary>
    void OnDestroy()
    {
        if (triggerZone == null) return;
        triggerZone.OnPlayerEntered -= OnPlayerEnteredTrigger;
        triggerZone.OnPlayerExited -= StopChase;
    }

    ///  <summary>
    ///  Controla la duración de la persecución y detiene la persecución cuando el temporizador se agota.
    ///  </summary>
    void Update()
    {
        if (!chasing || chaseDuration < 1f) return;
        chaseTimer -= Time.deltaTime;
        if (chaseTimer <= 0f) StopChase();
    }

    /// <summary>
    /// Controla el movimiento del NPC hacia el jugador mientras lo persigue, y detiene el movimiento si el jugador lo está mirando.
    /// </summary>
    void FixedUpdate()
    {
        if (!chasing || player == null) return;

        Vector3 direction = (player.position - transform.position);
        direction.y = 0f;
        direction.Normalize();

        bool playerLooking = IsPlayerLookingAtNPC();

        if (playerLooking)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            Vector3 newPosition = rb.position + direction * npcSpeedWhileChasing * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.MoveRotation(Quaternion.RotateTowards(
                    rb.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                ));
            }
        }
    }

    /// <summary>
    /// Se llama cuando el jugador entra en el trigger para iniciar la persecución. Si el jugador no está asignado, intenta encontrarlo por su etiqueta.
    /// </summary>
    void OnPlayerEnteredTrigger()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            player = p.transform;
        }
        StartChase();
    }

    ///  <summary>
    ///  Inicia la persecución, reiniciando el temporizador si la persecución ya estaba activa.
    ///  </summary>
    public void StartChase()
    {
        chasing = true;
        if (chaseDuration >= 1f)
            chaseTimer = chaseDuration;
    }

    ///  <summary>
    ///  Detiene la persecución y resetea el temporizador.
    /// </summary>
    public void StopChase()
    {
        chasing = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Determina si el jugador está mirando al NPC dentro del ángulo de visión especificado.
    /// </summary>
    bool IsPlayerLookingAtNPC()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 toNPC = transform.position - cam.transform.position;
        float angle = Vector3.Angle(cam.transform.forward, toNPC);
        return angle < viewAngle / 2f;
    }
}