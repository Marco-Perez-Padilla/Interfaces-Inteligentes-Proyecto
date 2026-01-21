using UnityEngine;

/**
 * @file: VisualChasing.cs
 * @brief: Script de persecución para NPCs que implementa la mecánica "Weeping Angel": el NPC persigue
 * al jugador pero se detiene si este lo está mirando. La persecución puede configurarse por tiempo
 * fijo o mientras el jugador esté en la zona trigger.
 *
 * Notas: Si chaseDuration < 1s, la persecución se detiene al salir del trigger. Si chaseDuration >= 1s,
 * la persecución continúa hasta que termine el tiempo independientemente del trigger. El script añade
 * automáticamente un Rigidbody al NPC si no lo tiene.
 */
public class VisualChasing : MonoBehaviour
{
    [Header("References")]
    public TriggerNotificator triggerZone;          // Zona trigger que activa la persecución
    public Transform player;                        // Transform del jugador

    [Header("Movement")]
    public float npcSpeedWhileChasing = 1.5f;       // Velocidad de persecución
    public float rotationSpeed = 360f;              // Velocidad de rotación hacia el jugador
    public float viewAngle = 60f;                   // Ángulo de visión del jugador (para detección)

    [Header("Chase Control")]
    public float chaseDuration = 5f;                // Duración de la persecución en segundos

    private Rigidbody rb;
    private bool chasing = false;
    private float chaseTimer = 0f;

    void Awake()
    {
        // Añadir Rigidbody si no existe
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
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
        triggerZone.OnPlayerExited -= StopChase;
    }

    // Cuenta regresiva del temporizador de persecución
    void Update()
    {
        // Solo cuenta el tiempo si hay temporizador
        if (!chasing || chaseDuration < 1f)
            return;

        chaseTimer -= Time.deltaTime;
        if (chaseTimer <= 0f)
            StopChase();
    }

    // Lógica de movimiento y persecución
    void FixedUpdate()
    {
        if (!chasing || player == null)
            return;

        Vector3 toPlayer = player.position - transform.position;
        Vector3 direction = toPlayer.normalized;

        // Verificar si el jugador está mirando al NPC
        bool playerLooking = IsPlayerLookingAtNPC(transform, player, viewAngle);

        if (playerLooking)
        {
            // Mecánica Weeping Angel: si te mira, te quedas quieto
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            // Movimiento hacia el jugador
            rb.linearVelocity = direction * npcSpeedWhileChasing;

            // Rotación hacia el jugador
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.RotateTowards(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            ));
        }
    }

    // Callback cuando el jugador entra en el trigger
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

    // Iniciar persecución
    public void StartChase() 
    {
        chasing = true;
        
        if (chaseDuration >= 1f)
            chaseTimer = chaseDuration;
    }

    // Detener persecución
    public void StopChase()
    {
        chasing = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Detectar si el jugador está mirando al NPC
    bool IsPlayerLookingAtNPC(Transform npc, Transform player, float viewAngle = 60f)
    {
        Vector3 toNPC = npc.position - player.position;
        Vector3 playerForward = player.forward;

        float angle = Vector3.Angle(playerForward, toNPC);
        return angle < viewAngle / 2f;
    }
}