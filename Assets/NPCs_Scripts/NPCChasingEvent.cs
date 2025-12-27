using UnityEngine;

public class NPCChaser : MonoBehaviour
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

        float playerSpeedAtEntry = GetPlayerSpeed();
        npcSpeed = Mathf.Max(playerSpeedAtEntry * speedMultiplier, speedTreshold);
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
