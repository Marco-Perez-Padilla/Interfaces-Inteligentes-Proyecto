using UnityEngine;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Start()
    {
        if (triggerZone != null)
        {
            triggerZone.OnPlayerEntered += OnPlayerEnteredTrigger;
            if (chaseDuration < 1f)
                triggerZone.OnPlayerExited += StopChase;
        }

        // Perseguir inmediatamente si tenemos player
        if (player != null)
        {
            npcSpeed = speedTreshold;
            StartChase();
        }
    }

    void OnDestroy()
    {
        if (triggerZone == null) return;
        triggerZone.OnPlayerEntered -= OnPlayerEnteredTrigger;
        triggerZone.OnPlayerExited -= StopChase;
    }

    void Update()
    {
        if (!chasing || chaseDuration < 1f) return;
        chaseTimer -= Time.deltaTime;
        if (chaseTimer <= 0f) StopChase();
    }

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
        if (chaseDuration >= 1f)
            chaseTimer = chaseDuration;
    }

    public void StopChase()
    {
        chasing = false;
        chaseTimer = 0f;
        rb.linearVelocity = Vector3.zero;
    }

    float GetPlayerSpeed()
    {
        if (player == null) return 5f;
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
            return Mathf.Max(playerRb.linearVelocity.magnitude, 0.1f);
        return 5f;
    }
}