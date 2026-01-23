using UnityEngine;

/**
 * @file: NPCChasingEvent.cs
 * @brief: Evento de persecución directa donde un NPC persigue al jugador al entrar
 * en una zona trigger. La velocidad del NPC se adapta dinámicamente a la velocidad
 * del jugador en el momento de activación.
 *
 * Notas:
 * - Si chaseDuration < 1s, la persecución se detiene al salir del trigger.
 * - Si chaseDuration >= 1s, la persecución continúa hasta que termina el temporizador.
 * - El NPC siempre mantiene una velocidad mínima definida por speedTreshold.
 */
public class NPCChasingEvent : MonoBehaviour
{
    [Header("References")]
    public TriggerNotificator triggerZone;   // Zona trigger que inicia la persecución
    public Transform player;                 // Transform del jugador

    [Header("Movement")]
    public float speedMultiplier = 1.5f;     // Multiplicador de velocidad respecto al jugador
    public float rotationSpeed = 360f;       // Velocidad de rotación hacia el jugador

    [Header("Chase Control")]
    public float chaseDuration = 5f;         // Duración de la persecución en segundos
    public float speedTreshold = 1f;         // Velocidad mínima garantizada del NPC

    private Rigidbody rb;
    private bool chasing = false;
    private float chaseTimer = 0f;
    private float npcSpeed = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Evita rotaciones físicas no deseadas
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    // Suscripción a eventos del trigger
    void OnEnable()
    {
        if (triggerZone == null) return;

        triggerZone.OnPlayerEntered += OnPlayerEnteredTrigger;

        // Solo escuchamos la salida si NO hay temporizador
        if (chaseDuration < 1f)
            triggerZone.OnPlayerExited += StopChase;
    }

    // Desuscripción de eventos del trigger
    void OnDisable()
    {
        if (triggerZone == null) return;

        triggerZone.OnPlayerEntered -= OnPlayerEnteredTrigger;
        triggerZone.OnPlayerExited  -= StopChase;
    }

    // Cuenta regresiva del temporizador de persecución
    void Update()
    {
        // Solo cuenta el tiempo si la persecución es temporal
        if (!chasing || chaseDuration < 1f)
            return;

        chaseTimer -= Time.deltaTime;
        if (chaseTimer <= 0f)
            StopChase();
    }

    // Movimiento y rotación del NPC hacia el jugador
    void FixedUpdate()
    {
        if (!chasing || player == null)
            return;

        // Dirección normalizada hacia el jugador
        Vector3 direction = (player.position - transform.position).normalized;

        // Movimiento directo usando Rigidbody
        rb.linearVelocity = direction * npcSpeed;

        // Rotación suave hacia el jugador
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.RotateTowards(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        ));
    }

    // Callback al entrar el jugador en el trigger
    void OnPlayerEnteredTrigger()
    {
        // Buscar automáticamente al jugador si no está asignado
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            player = p.transform;
        }

        // Captura la velocidad del jugador en el instante de entrada
        float playerSpeedAtEntry = GetPlayerSpeed();

        // Ajusta la velocidad del NPC según el jugador
        npcSpeed = Mathf.Max(playerSpeedAtEntry * speedMultiplier, speedTreshold);

        StartChase();
    }

    // Iniciar persecución
    void StartChase()
    {
        chasing = true;

        // Inicializar temporizador si procede
        if (chaseDuration >= 1f)
            chaseTimer = chaseDuration;
    }

    // Detener persecución
    public void StopChase()
    {
        chasing = false;
        chaseTimer = 0f;

        // Detener completamente el movimiento
        rb.linearVelocity = Vector3.zero;
    }

    // Obtener la velocidad actual del jugador
    float GetPlayerSpeed()
    {
        if (player == null) return 5f;

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
            return Mathf.Max(playerRb.linearVelocity.magnitude, 0.1f);

        // Fallback si el jugador no usa Rigidbody
        return 5f;
    }
}
