using UnityEngine;

/// <summary>
/// Script de inicialización automática para prefabs de NPCs enemigos.
/// Se encarga de asignar el tag "Enemy", añadir el trigger collider de contacto
/// con el jugador, localizar el TriggerNotificator más cercano y añadir
/// dinámicamente un comportamiento de IA aleatorio entre los disponibles.
///
/// Debe añadirse al prefab del NPC. No requiere configuración manual en el Inspector.
/// </summary>
public class EnemyInitializer : MonoBehaviour
{
  [Header("Collider de contacto con el jugador")]
  [Tooltip("Radio del SphereCollider trigger que detecta al jugador al chocar.")]
  [SerializeField] private float contactTriggerRadius = 0.6f;

  [Header("Comportamiento de persecución (NPCChasingEvent)")]
  [SerializeField] private float chasingSpeedMultiplier = 1.5f;
  [SerializeField] private float chasingRotationSpeed = 360f;
  [SerializeField] private float chasingDuration = 5f;
  [SerializeField] private float chasingSpeedThreshold = 1f;

  [Header("Comportamiento visual (VisualChasing)")]
  [SerializeField] private float visualChasingSpeed = 1.5f;
  [SerializeField] private float visualChasingRotationSpeed = 360f;
  [SerializeField] private float visualChasingViewAngle = 60f;
  [SerializeField] private float visualChasingDuration = 5f;

  /// <summary>
  /// Número total de comportamientos disponibles para asignación aleatoria.
  /// Actualizar este valor al añadir nuevos comportamientos al switch.
  /// </summary>
  private const int BEHAVIOR_COUNT = 2;

  void Start()
  {
    AssignEnemyTag();
    AddContactTriggerCollider();

    TriggerNotificator nearestTrigger = FindNearestTriggerNotificator();
    Transform playerTransform = FindPlayerTransform();

    AssignRandomBehavior(nearestTrigger, playerTransform);
  }

  /// <summary>
  /// Asigna el tag "Enemy" al GameObject.
  /// El tag debe existir previamente en Project Settings → Tags and Layers.
  /// </summary>
  private void AssignEnemyTag()
  {
    gameObject.tag = "Enemy";
  }

  /// <summary>
  /// Añade un SphereCollider configurado como trigger para detectar
  /// la colisión física con el jugador. No reemplaza colliders existentes.
  /// </summary>
  private void AddContactTriggerCollider()
  {
    SphereCollider contactCollider = gameObject.AddComponent<SphereCollider>();
    contactCollider.isTrigger = true;
    contactCollider.radius = contactTriggerRadius;
  }

  /// <summary>
  /// Recorre todos los TriggerNotificator de la escena y devuelve
  /// el que esté más cerca de la posición actual del NPC.
  /// </summary>
  /// <returns>El TriggerNotificator más cercano, o null si no hay ninguno.</returns>
  private TriggerNotificator FindNearestTriggerNotificator()
  {
    TriggerNotificator[] allTriggers = FindObjectsByType<TriggerNotificator>(FindObjectsSortMode.None);

    if (allTriggers.Length == 0)
    {
      Debug.LogWarning($"[EnemyInitializer] No se encontró ningún TriggerNotificator en la escena. NPC: {gameObject.name}");
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

  /// <summary>
  /// Busca el Transform del jugador mediante su tag "Player".
  /// </summary>
  /// <returns>El Transform del jugador, o null si no existe en la escena.</returns>
  private Transform FindPlayerTransform()
  {
    GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

    if (playerObject == null)
    {
      Debug.LogWarning($"[EnemyInitializer] No se encontró ningún GameObject con tag \"Player\". NPC: {gameObject.name}");
      return null;
    }

    return playerObject.transform;
  }

  /// <summary>
  /// Elige un comportamiento aleatorio entre los disponibles y añade
  /// el componente correspondiente, configurándolo con las referencias necesarias.
  /// Al añadir un nuevo comportamiento: incrementar BEHAVIOR_COUNT y añadir su case.
  /// </summary>
  /// <param name="nearestTrigger">TriggerNotificator más cercano al NPC.</param>
  /// <param name="playerTransform">Transform del jugador.</param>
  private void AssignRandomBehavior(TriggerNotificator nearestTrigger, Transform playerTransform)
  {
    int behaviorIndex = Random.Range(0, BEHAVIOR_COUNT);

    switch (behaviorIndex)
    {
      case 0:
        AssignChasingBehavior(nearestTrigger, playerTransform);
        break;

      case 1:
        AssignVisualChasingBehavior(nearestTrigger, playerTransform);
        break;

      default:
        Debug.LogError($"[EnemyInitializer] Índice de comportamiento no contemplado: {behaviorIndex}. NPC: {gameObject.name}");
        break;
    }
  }

  /// <summary>
  /// Añade y configura NPCChasingEvent: persecución directa activada por trigger.
  /// </summary>
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

  /// <summary>
  /// Añade y configura VisualChasing: persecución tipo Weeping Angel.
  /// </summary>
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