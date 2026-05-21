using UnityEngine;

public class EnemyInitializer : MonoBehaviour
{
    [Header("Collider de contacto")]
    [SerializeField] private float contactTriggerRadius = 0.6f;

    [Header("NPCChasingEvent")]
    [SerializeField] private float chasingSpeedMultiplier = 1.5f;
    [SerializeField] private float chasingRotationSpeed = 360f;
    [SerializeField] private float chasingDuration = 5f;
    [SerializeField] private float chasingSpeedThreshold = 1f;

    [Header("VisualChasing")]
    [SerializeField] private float visualChasingSpeed = 1.5f;
    [SerializeField] private float visualChasingRotationSpeed = 360f;
    [SerializeField] private float visualChasingViewAngle = 60f;
    [SerializeField] private float visualChasingDuration = 5f;

    private const int BEHAVIOR_COUNT = 2;

    private TriggerNotificator overriddenTrigger = null;
    private Transform overriddenPlayer = null;

    public void OverrideNearestTrigger(TriggerNotificator trigger)
    {
        overriddenTrigger = trigger;
    }

    public void OverridePlayerTransform(Transform player)
    {
        overriddenPlayer = player;
    }

    void Start()
    {
        AssignEnemyTag();
        AddContactTriggerCollider();
        EnsureRigidbody();
        DisableRootMotion();

        TriggerNotificator nearestTrigger = overriddenTrigger ?? FindNearestTriggerNotificator();
        Transform playerTransform = overriddenPlayer ?? FindPlayerTransform();

        if (playerTransform == null)
            Debug.LogWarning($"[EnemyInitializer] Sin jugador. NPC: {gameObject.name}.");

        AssignRandomBehavior(nearestTrigger, playerTransform);
    }

    private void AssignEnemyTag()
    {
        gameObject.tag = "Enemy";
    }

    private void AddContactTriggerCollider()
    {
        SphereCollider contactCollider = gameObject.AddComponent<SphereCollider>();
        contactCollider.isTrigger = true;
        contactCollider.radius = contactTriggerRadius;
    }

    private void EnsureRigidbody()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true; // MovePosition + detección de colisiones
        rb.freezeRotation = true;
    }

    private void DisableRootMotion()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
            animator.applyRootMotion = false;
    }

    private TriggerNotificator FindNearestTriggerNotificator()
    {
        TriggerNotificator[] allTriggers = FindObjectsByType<TriggerNotificator>(FindObjectsSortMode.None);
        if (allTriggers.Length == 0) return null;

        TriggerNotificator nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (TriggerNotificator candidate in allTriggers)
        {
            float distance = Vector3.Distance(transform.position, candidate.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = candidate;
            }
        }
        return nearest;
    }

    private Transform FindPlayerTransform()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        return playerObject != null ? playerObject.transform : null;
    }

    private void AssignRandomBehavior(TriggerNotificator nearestTrigger, Transform playerTransform)
    {
        int behaviorIndex = Random.Range(0, BEHAVIOR_COUNT);
        switch (behaviorIndex)
        {
            case 0: AssignChasingBehavior(nearestTrigger, playerTransform); break;
            case 1: AssignVisualChasingBehavior(nearestTrigger, playerTransform); break;
        }
    }

    private void AssignChasingBehavior(TriggerNotificator nearestTrigger, Transform playerTransform)
    {
        NPCChasingEvent chasing = gameObject.AddComponent<NPCChasingEvent>();
        chasing.triggerZone = nearestTrigger;
        chasing.player = playerTransform;
        chasing.speedMultiplier = chasingSpeedMultiplier;
        chasing.rotationSpeed = chasingRotationSpeed;
        chasing.chaseDuration = chasingDuration;
        chasing.speedTreshold = chasingSpeedThreshold;
    }

    private void AssignVisualChasingBehavior(TriggerNotificator nearestTrigger, Transform playerTransform)
    {
        VisualChasing visual = gameObject.AddComponent<VisualChasing>();
        visual.triggerZone = nearestTrigger;
        visual.player = playerTransform;
        visual.npcSpeedWhileChasing = visualChasingSpeed;
        visual.rotationSpeed = visualChasingRotationSpeed;
        visual.viewAngle = visualChasingViewAngle;
        visual.chaseDuration = visualChasingDuration;
    }
}