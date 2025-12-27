using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NPCChaser : MonoBehaviour
{
    [Header("References")]
    public TriggerNotificator triggerZone;
    public Transform player;

    [Header("Movement")]
    public float speedMultiplier = 0.9f;          
    public float rotationSpeed = 360f;   
    
    [Header("Chase Control")]
    public float chaseDuration = 5f;
    public float playerSpeedThreshold = 3f;

    private Rigidbody rb;
    private bool chasing = false;
    private float chaseTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void OnEnable()
    {
        if (triggerZone != null) {
            triggerZone.OnPlayerEntered += OnPlayerEnteredTrigger;
            triggerZone.OnPlayerExited  += StopChase;
        }
    }

    void OnDisable()
    {
        if (triggerZone != null) {
            triggerZone.OnPlayerEntered -= OnPlayerEnteredTrigger;
            triggerZone.OnPlayerExited  -= StopChase;
        }
    }

    void Update()
    {
        if (!chasing || chaseDuration <= 0f)
            return;

        chaseTimer -= Time.deltaTime;
        if (chaseTimer <= 0f)
        {
            StopChase();
        }
    }

    void FixedUpdate()
    {
        if (!chasing || player == null)
            return;

        Vector3 toPlayer = player.position - transform.position;
        // Si la persecusión es en cuesta entonces eliminar la siguiente línea
        toPlayer.y = 0f;

        Vector3 direction = toPlayer.normalized;

        // Movement towards the player
        float playerSpeed = GetPlayerSpeed();

        // Explicación: A más lento vaya el jugador, más rápido va el npc
        //if (playerSpeed < playerSpeedThreshold)
        //{
            // NPC se vuelve más rápido según qué tan lento vaya el jugador
            //float speedFactor = 1f + (playerSpeedThreshold - playerSpeed); // +1 unidad/s por cada unidad por debajo
            //npcSpeed = Mathf.Max(playerSpeed * speedMultiplier * speedFactor, npcMinSpeed);
        //}

        float npcSpeed = playerSpeed * speedMultiplier;
        rb.velocity = direction * npcSpeed;

        // Rotation towards the player
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.RotateTowards(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        ));
    }

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

    void StartChase()
    {
        chasing = true;
        if (chaseDuration > 0f)
            chaseTimer = chaseDuration;
    }

    public void StopChase()
    {
        chasing = false;
        rb.velocity = Vector3.zero;
    }

    float GetPlayerSpeed()
    {
        if (player == null) return 5f;

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
            return Mathf.Max(playerRb.velocity.magnitude, 0.1f);

        return 5f;
    }
}
