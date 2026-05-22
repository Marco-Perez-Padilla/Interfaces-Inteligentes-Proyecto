using UnityEngine;

/**
 * @file: NPCChasingEvent.cs
 * @brief: Controla el comportamiento de persecución de un NPC cuando el jugador entra en una zona trigger.
 *
 * Notas:
 * - El NPC comenzará a perseguir al jugador cuando este entre en la zona de activación.
 * - El NPC tendrá una velocidad de persecución basada en la velocidad del jugador.
 * - El NPC dejará de perseguir al jugador después de un tiempo determinado.
 */

public class NPCChasingEvent : MonoBehaviour
{
    [Header("References")]
    public TriggerNotificator triggerZone;
    public Transform player;

    [Header("Movement")]
    public float speedMultiplier = 1.5f;
    public float rotationSpeed = 360f;

    [Header("Chase Control")]
    public float chaseDuration = 5f;
    public float speedTreshold = 1f;

    private Rigidbody rb;
    private bool chasing = false;
    private float chaseTimer = 0f;
    private float npcSpeed = 0f;

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
        {
            npcSpeed = speedTreshold;
            StartChase();
        }
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

    /// <summary>
    /// Controla el temporizador de persecución y detiene la persecución cuando el tiempo se agota.
    /// </summary>
    void Update()
    {
        if (!chasing || chaseDuration < 1f) return;
        chaseTimer -= Time.deltaTime;
        if (chaseTimer <= 0f) StopChase();
    }

    /// <summary>
    /// Mueve al NPC hacia el jugador y lo rota para que mire en la dirección del movimiento.
    /// </summary>
    void FixedUpdate()
    {
        if (!chasing || player == null) return;

        Vector3 direction = (player.position - transform.position);
        direction.y = 0f;
        direction.Normalize();

        Vector3 newPosition = rb.position + direction * npcSpeed * Time.fixedDeltaTime;
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

    ///  <summary>
    ///  Inicia la persecución cuando el jugador entra en la zona trigger, ajustando la velocidad del NPC según la velocidad del jugador.
    ///  </summary>
    void OnPlayerEnteredTrigger()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            player = p.transform;
        }

        float playerSpeedAtEntry = GetPlayerSpeed();
        npcSpeed = Mathf.Max(playerSpeedAtEntry * speedMultiplier, speedTreshold);
        StartChase();
    }

    ///  <summary>
    ///  Inicia la persecución y resetea el temporizador.
    /// </summary>
    void StartChase()
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
        chaseTimer = 0f;
        rb.linearVelocity = Vector3.zero;
    }

    /// <summary>
    /// Obtiene la velocidad actual del jugador. Si no se puede obtener, devuelve un valor predeterminado.
    /// </summary>
    float GetPlayerSpeed()
    {
        if (player == null) return 5f;
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
            return Mathf.Max(playerRb.linearVelocity.magnitude, 0.1f);
        return 5f;
    }
}