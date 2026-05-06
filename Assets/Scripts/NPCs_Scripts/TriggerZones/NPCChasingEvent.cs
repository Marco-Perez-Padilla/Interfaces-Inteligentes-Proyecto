using UnityEngine;

/// <summary>
/// Evento de persecución directa donde un NPC persigue al jugador al entrar
/// en una zona trigger. La velocidad del NPC se adapta dinámicamente a la velocidad
/// del jugador en el momento de activación.
///
/// Notas:
/// - Si chaseDuration menor que 1s, la persecución se detiene al salir del trigger.
/// - Si chaseDuration mayor o igual a 1s, la persecución continúa hasta que termina el temporizador.
/// - El NPC siempre mantiene una velocidad mínima definida por speedThreshold.
/// - La suscripción al trigger se hace en Start() para garantizar que triggerZone
///   ya ha sido asignado por EnemyInitializer antes de suscribirse.
/// </summary>
public class NPCChasingEvent : MonoBehaviour
{
    [Header("References")]
    public TriggerNotificator triggerZone;   // Zona trigger que inicia la persecución
    public Transform player;                 // Transform del jugador

    [Header("Movement")]
    public float speedMultiplier = 1.5f;     // Multiplicador de velocidad respecto al jugador
    public float rotationSpeed = 360f;     // Velocidad de rotación hacia el jugador

    [Header("Chase Control")]
    public float chaseDuration = 5f;        // Duración de la persecución en segundos
    public float speedTreshold = 1f;        // Velocidad mínima garantizada del NPC

    private Rigidbody npcRigidbody;
    private bool chasing = false;
    private float chaseTimer = 0f;
    private float npcSpeed = 0f;

    void Awake()
    {
        npcRigidbody = GetComponent<Rigidbody>();
        npcRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    /// <summary>
    /// Suscripción en Start() para garantizar que EnemyInitializer ya ha asignado
    /// triggerZone antes de intentar suscribirse a sus eventos.
    /// </summary>
    void Start()
    {
        if (triggerZone == null) return;

        triggerZone.OnPlayerEntered += OnPlayerEnteredTrigger;

        if (chaseDuration < 1f)
            triggerZone.OnPlayerExited += StopChase;
    }

    /// <summary>
    /// Desuscripción en OnDestroy() para evitar referencias colgantes
    /// cuando el NPC es destruido.
    /// </summary>
    void OnDestroy()
    {
        if (triggerZone == null) return;

        triggerZone.OnPlayerEntered -= OnPlayerEnteredTrigger;
        triggerZone.OnPlayerExited -= StopChase;
    }

    /// <summary>
    /// Cuenta regresiva del temporizador de persecución.
    /// Solo activo si la persecución es temporal (chaseDuration mayor o igual a 1s).
    /// </summary>
    void Update()
    {
        if (!chasing || chaseDuration < 1f)
            return;

        chaseTimer -= Time.deltaTime;
        if (chaseTimer <= 0f)
            StopChase();
    }

    /// <summary>
    /// Movimiento y rotación del NPC hacia el jugador mediante Rigidbody.
    /// </summary>
    void FixedUpdate()
    {
        if (!chasing || player == null)
            return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        npcRigidbody.linearVelocity = directionToPlayer * npcSpeed;

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        npcRigidbody.MoveRotation(Quaternion.RotateTowards(
            npcRigidbody.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        ));
    }

    /// <summary>
    /// Callback al entrar el jugador en el trigger.
    /// Busca al jugador si no está asignado y ajusta la velocidad del NPC.
    /// </summary>
    void OnPlayerEnteredTrigger()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null) return;
            player = playerObject.transform;
        }

        float playerSpeedAtEntry = GetPlayerSpeed();
        npcSpeed = Mathf.Max(playerSpeedAtEntry * speedMultiplier, speedTreshold);

        StartChase();
    }

    /// <summary>
    /// Inicia la persecución y resetea el temporizador si corresponde.
    /// </summary>
    void StartChase()
    {
        chasing = true;

        if (chaseDuration >= 1f)
            chaseTimer = chaseDuration;
    }

    /// <summary>
    /// Detiene la persecución y para el Rigidbody completamente.
    /// </summary>
    public void StopChase()
    {
        chasing = false;
        chaseTimer = 0f;

        npcRigidbody.linearVelocity = Vector3.zero;
    }

    /// <summary>
    /// Obtiene la velocidad actual del jugador mediante su Rigidbody.
    /// Devuelve un valor por defecto si el jugador no tiene Rigidbody.
    /// </summary>
    float GetPlayerSpeed()
    {
        if (player == null) return 5f;

        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        if (playerRigidbody != null)
            return Mathf.Max(playerRigidbody.linearVelocity.magnitude, 0.1f);

        return 5f;
    }
}