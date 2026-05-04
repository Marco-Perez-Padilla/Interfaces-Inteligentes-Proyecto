using UnityEngine;

/// <summary>
/// Script de inicialización automática para prefabs de NPCs enemigos.
/// Asigna tag, collider de contacto, Rigidbody, TriggerNotificator más cercano
/// y un comportamiento de IA aleatorio entre los disponibles.
///
/// EnemySpawner puede llamar a OverrideNearestTrigger() antes de que Start()
/// ejecute la búsqueda automática, evitando una búsqueda redundante.
/// </summary>
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

  /// <summary>Número de comportamientos disponibles. Actualizar al añadir nuevos.</summary>
  private const int BEHAVIOR_COUNT = 2;

  private TriggerNotificator overriddenTrigger = null;

  /// <summary>
  /// Permite a EnemySpawner inyectar el TriggerNotificator ya calculado,
  /// evitando que Start() haga una búsqueda redundante en escena.
  /// </summary>
  public void OverrideNearestTrigger(TriggerNotificator trigger)
  {
    overriddenTrigger = trigger;
  }

  void Start()
  {
    AssignEnemyTag();
    AddContactTriggerCollider();
    EnsureRigidbody();

    TriggerNotificator nearestTrigger = overriddenTrigger ?? FindNearestTriggerNotificator();
    Transform playerTransform = FindPlayerTransform();

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
    if (GetComponent<Rigidbody>() != null)
      return;

    Rigidbody npcRigidbody = gameObject.AddComponent<Rigidbody>();
    npcRigidbody.useGravity = false;
    npcRigidbody.freezeRotation = true;
    npcRigidbody.constraints = RigidbodyConstraints.FreezePositionY
                               | RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ;
  }

  private TriggerNotificator FindNearestTriggerNotificator()
  {
    TriggerNotificator[] allTriggers = FindObjectsByType<TriggerNotificator>(FindObjectsSortMode.None);

    if (allTriggers.Length == 0)
    {
      Debug.LogWarning($"[EnemyInitializer] Sin TriggerNotificators en escena. NPC: {gameObject.name}");
      return null;
    }

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

    if (playerObject == null)
    {
      Debug.LogWarning($"[EnemyInitializer] Sin jugador en escena. NPC: {gameObject.name}");
      return null;
    }

    return playerObject.transform;
  }

  private void AssignRandomBehavior(TriggerNotificator nearestTrigger, Transform playerTransform)
  {
    int behaviorIndex = Random.Range(0, BEHAVIOR_COUNT);

    switch (behaviorIndex)
    {
      case 0: AssignChasingBehavior(nearestTrigger, playerTransform); break;
      case 1: AssignVisualChasingBehavior(nearestTrigger, playerTransform); break;

      default:
        Debug.LogError($"[EnemyInitializer] Índice de comportamiento sin case: {behaviorIndex}.");
        break;
    }
  }

  private void AssignChasingBehavior(TriggerNotificator nearestTrigger, Transform playerTransform)
  {
    NPCChasingEvent chasingBehavior = gameObject.AddComponent<NPCChasingEvent>();

    chasingBehavior.triggerZone = nearestTrigger;
    chasingBehavior.player = playerTransform;
    chasingBehavior.speedMultiplier = chasingSpeedMultiplier;
    chasingBehavior.rotationSpeed = chasingRotationSpeed;
    chasingBehavior.chaseDuration = chasingDuration;
    chasingBehavior.speedTreshold = chasingSpeedThreshold;
  }

  private void AssignVisualChasingBehavior(TriggerNotificator nearestTrigger, Transform playerTransform)
  {
    VisualChasing visualBehavior = gameObject.AddComponent<VisualChasing>();

    visualBehavior.triggerZone = nearestTrigger;
    visualBehavior.player = playerTransform;
    visualBehavior.npcSpeedWhileChasing = visualChasingSpeed;
    visualBehavior.rotationSpeed = visualChasingRotationSpeed;
    visualBehavior.viewAngle = visualChasingViewAngle;
    visualBehavior.chaseDuration = visualChasingDuration;
  }
}