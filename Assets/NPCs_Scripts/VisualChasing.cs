using UnityEngine;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void OnEnable()
    {
        if (triggerZone != null)
        {
            triggerZone.OnPlayerEntered += OnPlayerEnteredTrigger;
            triggerZone.OnPlayerExited  += StopChase;
        }
    }

    void OnDisable()
    {
        if (triggerZone != null)
        {
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
            StopChase();
    }

    void FixedUpdate()
    {
        if (!chasing || player == null)
            return;

        Vector3 toPlayer = player.position - transform.position;
        Vector3 direction = toPlayer.normalized;

        bool playerLooking = IsPlayerLookingAtNPC(transform, player, viewAngle);

        if (playerLooking)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            rb.velocity = direction * npcSpeedWhileChasing;

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
        rb.angularVelocity = Vector3.zero;
    }

    bool IsPlayerLookingAtNPC(Transform npc, Transform player, float viewAngle = 60f)
    {
        Vector3 toNPC = npc.position - player.position;
        Vector3 playerForward = player.forward;

        float angle = Vector3.Angle(playerForward, toNPC);
        return angle < viewAngle / 2f;
    }
}
