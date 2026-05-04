using UnityEngine;

/// <summary>
/// Script de persecución para NPCs que implementa la mecánica Weeping Angel:
/// el NPC persigue al jugador pero se detiene si este lo está mirando.
///
/// Notas:
/// - Si chaseDuration menor que 1s, la persecución se detiene al salir del trigger.
/// - Si chaseDuration mayor o igual a 1s, continúa hasta que termine el tiempo.
/// - La suscripción al trigger se hace en Start() para garantizar que triggerZone
///   ya ha sido asignado por EnemyInitializer antes de suscribirse.
/// - El Rigidbody se obtiene en Awake(); EnemyInitializer debe garantizar
///   que existe antes de añadir este componente.
/// </summary>
public class VisualChasing : MonoBehaviour
{
    [Header("References")]
    public TriggerNotificator triggerZone;         // Zona trigger que activa la persecución
    public Transform player;              // Transform del jugador

    [Header("Movement")]
    public float npcSpeedWhileChasing = 1.5f;      // Velocidad de persecución
    public float rotationSpeed = 360f;       // Velocidad de rotación hacia el jugador
    public float viewAngle = 60f;        // Ángulo de visión del jugador para detección

    [Header("Chase Control")]
    public float chaseDuration = 5f;               // Duración de la persecución en segundos

    private Rigidbody npcRigidbody;
    private bool chasing = false;
    private float chaseTimer = 0f;

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
    /// Solo activo si hay temporizador configurado.
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
    /// Lógica de movimiento: si el jugador mira al NPC, se detiene (Weeping Angel).
    /// Si no lo mira, avanza hacia él.
    /// </summary>
    void FixedUpdate()
    {
        if (!chasing || player == null)
            return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        bool playerIsLooking = IsPlayerLookingAtNPC(transform, player, viewAngle);

        if (playerIsLooking)
        {
            npcRigidbody.linearVelocity = Vector3.zero;
            npcRigidbody.angularVelocity = Vector3.zero;
        }
        else
        {
            npcRigidbody.linearVelocity = directionToPlayer * npcSpeedWhileChasing;

            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            npcRigidbody.MoveRotation(Quaternion.RotateTowards(
                npcRigidbody.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            ));
        }
    }

    /// <summary>
    /// Callback cuando el jugador entra en el trigger.
    /// Busca al jugador si no está asignado e inicia la persecución.
    /// </summary>
    void OnPlayerEnteredTrigger()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null) return;
            player = playerObject.transform;
        }

        StartChase();
    }

    /// <summary>
    /// Inicia la persecución y resetea el temporizador si corresponde.
    /// </summary>
    public void StartChase()
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

        npcRigidbody.linearVelocity = Vector3.zero;
        npcRigidbody.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Determina si el jugador está mirando al NPC comprobando el ángulo
    /// entre el forward del jugador y el vector hacia el NPC.
    /// </summary>
    /// <param name="npcTransform">Transform del NPC.</param>
    /// <param name="playerTransform">Transform del jugador.</param>
    /// <param name="fieldOfViewAngle">Ángulo total del campo de visión en grados.</param>
    /// <returns>True si el jugador tiene al NPC dentro de su campo de visión.</returns>
    bool IsPlayerLookingAtNPC(Transform npcTransform, Transform playerTransform, float fieldOfViewAngle)
    {
        Vector3 toNPC = npcTransform.position - playerTransform.position;
        float angleBetween = Vector3.Angle(playerTransform.forward, toNPC);

        return angleBetween < fieldOfViewAngle / 2f;
    }
}