using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawner central de enemigos. Se suscribe a todos los TriggerNotificator
/// y NoiseDetector de la escena para instanciar enemigos bajo dos condiciones:
///
///   1. El jugador entra en una TriggerZone → aparece un enemigo que lo persigue.
///   2. El jugador hace mucho ruido en una NoiseZone → aparecen varios enemigos.
///
/// La suscripción a triggers se hace en Start() para garantizar que
/// PathSurfaceBuilder ya los ha generado. Las referencias a los delegados
/// se guardan en un diccionario para poder desuscribirse correctamente.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
  [Header("Prefabs de enemigos")]
  [Tooltip("Prefabs de NPC disponibles. Deben tener EnemyInitializer.")]
  [SerializeField] private GameObject[] enemyPrefabs;

  [Header("Spawn por TriggerZone")]
  [Tooltip("Cuántos enemigos aparecen al entrar en una trigger zone.")]
  [SerializeField] private int enemiesPerTrigger = 1;

  [Tooltip("Radio de offset aleatorio respecto al trigger al spawnear.")]
  [SerializeField] private float triggerSpawnOffsetRadius = 2f;

  [Header("Spawn por Ruido")]
  [Tooltip("Cuántos enemigos aparecen al detectarse ruido alto.")]
  [SerializeField] private int enemiesPerNoise = 2;

  [Tooltip("Radio de spawn alrededor del jugador al detectarse ruido.")]
  [SerializeField] private float noiseSpawnRadius = 5f;

  [Tooltip("Radio de exclusión interna: ningún enemigo aparece más cerca que esto del jugador.")]
  [SerializeField] private float noiseInnerExclusionRadius = 2f;

  [Tooltip("Cooldown en segundos entre spawns por ruido.")]
  [SerializeField] private float noiseSpawnCooldown = 2f;

  [Tooltip("Transform de la vagoneta para calcular el cono de exclusión frontal.")]
  [SerializeField] private Transform vagonetaTransform;

  [Tooltip("Ángulo del cono de exclusión frontal de la vagoneta en grados.")]
  [Range(0f, 180f)]
  [SerializeField] private float coneExclusionAngle = 60f;

  [Header("Offset en Y")]
  [Tooltip("Offset vertical al instanciar para no spawnear dentro del suelo.")]
  [SerializeField] private float spawnHeightOffset = 0.1f;

  [Header("Debug")]
  [Tooltip("Si true, spawnea enemigos al inicio para poder verlos sin esperar eventos.")]
  [SerializeField] private bool spawnOnStartForDebug = false;

  [Tooltip("Cuántos enemigos spawnear al inicio en modo debug.")]
  [SerializeField] private int debugSpawnCount = 5;

  private float lastNoiseSpawnTime = float.MinValue;

  /// <summary>
  /// Diccionario que guarda el delegado concreto asociado a cada TriggerNotificator.
  /// Necesario para poder desuscribirse correctamente: las lambdas anónimas
  /// crean una instancia nueva cada vez y += / -= nunca coincidirían.
  /// </summary>
  private Dictionary<TriggerNotificator, TriggerNotificator.TriggerEvent> triggerDelegates
      = new Dictionary<TriggerNotificator, TriggerNotificator.TriggerEvent>();

  // =====================================================
  // UNITY
  // =====================================================

  /// <summary>
  /// Start() en lugar de OnEnable() para garantizar que PathSurfaceBuilder
  /// ya ha ejecutado su Start() y generado los TriggerNotificators.
  /// El orden se refuerza con Script Execution Order en Project Settings.
  /// </summary>
  void Start()
  {
    SubscribeToAllTriggers();
    SubscribeToAllNoiseDetectors();

    if (spawnOnStartForDebug)
      SpawnDebugEnemies();
  }

  void OnDestroy()
  {
    UnsubscribeFromAllTriggers();
    UnsubscribeFromAllNoiseDetectors();
  }

  // =====================================================
  // SUSCRIPCIONES — TRIGGERS
  // =====================================================

  /// <summary>
  /// Suscribe un delegado nombrado a cada TriggerNotificator de la escena
  /// y lo guarda en el diccionario para poder desuscribirse después.
  /// </summary>
  private void SubscribeToAllTriggers()
  {
    TriggerNotificator[] allTriggers = FindObjectsByType<TriggerNotificator>(FindObjectsSortMode.None);

    if (allTriggers.Length == 0)
    {
      Debug.LogWarning("[EnemySpawner] No se encontró ningún TriggerNotificator en la escena.");
      return;
    }

    foreach (TriggerNotificator trigger in allTriggers)
    {
      // Captura la variable local para que el delegado apunte
      // al trigger correcto en cada iteración del foreach.
      TriggerNotificator capturedTrigger = trigger;
      TriggerNotificator.TriggerEvent triggerDelegate = () => OnTriggerZoneEntered(capturedTrigger);

      triggerDelegates[trigger] = triggerDelegate;
      trigger.OnPlayerEntered += triggerDelegate;
    }

    Debug.Log($"[EnemySpawner] Suscrito a {allTriggers.Length} TriggerNotificators.");
  }

  private void UnsubscribeFromAllTriggers()
  {
    foreach (KeyValuePair<TriggerNotificator, TriggerNotificator.TriggerEvent> entry in triggerDelegates)
    {
      if (entry.Key != null)
        entry.Key.OnPlayerEntered -= entry.Value;
    }

    triggerDelegates.Clear();
  }

  // =====================================================
  // SUSCRIPCIONES — RUIDO
  // =====================================================

  private void SubscribeToAllNoiseDetectors()
  {
    NoiseDetector[] allDetectors = FindObjectsByType<NoiseDetector>(FindObjectsSortMode.None);
    foreach (NoiseDetector detector in allDetectors)
      detector.OnHighNoiseDetected += OnHighNoiseDetected;
  }

  private void UnsubscribeFromAllNoiseDetectors()
  {
    NoiseDetector[] allDetectors = FindObjectsByType<NoiseDetector>(FindObjectsSortMode.None);
    foreach (NoiseDetector detector in allDetectors)
      detector.OnHighNoiseDetected -= OnHighNoiseDetected;
  }

  // =====================================================
  // CALLBACKS
  // =====================================================

  /// <summary>
  /// Llamado cuando el jugador entra en una TriggerZone.
  /// Instancia enemigos cerca del trigger que lo perseguirán.
  /// </summary>
  private void OnTriggerZoneEntered(TriggerNotificator sourceTrigger)
  {
    for (int i = 0; i < enemiesPerTrigger; i++)
    {
      Vector2 offset = Random.insideUnitCircle * triggerSpawnOffsetRadius;
      Vector3 spawnPosition = sourceTrigger.transform.position
                            + new Vector3(offset.x, spawnHeightOffset, offset.y);

      SpawnEnemy(spawnPosition, sourceTrigger);
    }
  }

  /// <summary>
  /// Llamado cuando se detecta ruido alto en una NoiseZone.
  /// Instancia enemigos alrededor del jugador respetando el cono de exclusión.
  /// </summary>
  private void OnHighNoiseDetected(Vector3 noisePosition, float intensity)
  {
    if (Time.time - lastNoiseSpawnTime < noiseSpawnCooldown)
      return;

    lastNoiseSpawnTime = Time.time;

    Vector3 vagonetaForward = vagonetaTransform != null
        ? Vector3.ProjectOnPlane(vagonetaTransform.forward, Vector3.up).normalized
        : Vector3.forward;

    int spawnedCount = 0;
    int safetyCounter = 0;
    int maxAttempts = enemiesPerNoise * 10;

    while (spawnedCount < enemiesPerNoise && safetyCounter < maxAttempts)
    {
      safetyCounter++;

      Vector2 randomCircle = Random.insideUnitCircle * noiseSpawnRadius;
      Vector3 candidatePos = noisePosition + new Vector3(randomCircle.x, spawnHeightOffset, randomCircle.y);

      float distanceToPlayer = Vector3.Distance(candidatePos, noisePosition);
      if (distanceToPlayer < noiseInnerExclusionRadius)
        continue;

      Vector3 directionToCandidate = Vector3.ProjectOnPlane(
          (candidatePos - noisePosition).normalized, Vector3.up
      );

      float angleWithVagoneta = Vector3.Angle(vagonetaForward, directionToCandidate);
      if (angleWithVagoneta <= coneExclusionAngle * 0.5f)
        continue;

      TriggerNotificator nearestTrigger = FindNearestTrigger(candidatePos);
      SpawnEnemy(candidatePos, nearestTrigger);
      spawnedCount++;
    }
  }

  // =====================================================
  // SPAWN
  // =====================================================

  /// <summary>
  /// Instancia un prefab de enemigo aleatorio en la posición indicada
  /// y deja que EnemyInitializer lo configure automáticamente.
  /// </summary>
  private void SpawnEnemy(Vector3 position, TriggerNotificator nearestTrigger)
  {
    if (enemyPrefabs == null || enemyPrefabs.Length == 0)
    {
      Debug.LogError("[EnemySpawner] No hay prefabs de enemigos asignados.");
      return;
    }

    GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
    GameObject spawnedEnemy = Instantiate(prefab, position, Quaternion.identity, GetOrCreateNPCContainer());

    EnemyInitializer initializer = spawnedEnemy.GetComponent<EnemyInitializer>();
    if (initializer != null)
      initializer.OverrideNearestTrigger(nearestTrigger);
  }

  /// <summary>
  /// Instancia enemigos distribuidos entre los triggers disponibles al inicio.
  /// Solo para depuración visual en play mode.
  /// </summary>
  private void SpawnDebugEnemies()
  {
    TriggerNotificator[] allTriggers = FindObjectsByType<TriggerNotificator>(FindObjectsSortMode.None);

    if (allTriggers.Length == 0)
    {
      Debug.LogWarning("[EnemySpawner] Sin triggers para debug spawn.");
      return;
    }

    for (int i = 0; i < debugSpawnCount; i++)
    {
      TriggerNotificator randomTrigger = allTriggers[Random.Range(0, allTriggers.Length)];
      Vector2 offset = Random.insideUnitCircle * triggerSpawnOffsetRadius;
      Vector3 spawnPosition = randomTrigger.transform.position
                                       + new Vector3(offset.x, spawnHeightOffset, offset.y);

      SpawnEnemy(spawnPosition, randomTrigger);
    }
  }

  // =====================================================
  // HELPERS
  // =====================================================

  private TriggerNotificator FindNearestTrigger(Vector3 position)
  {
    TriggerNotificator[] allTriggers = FindObjectsByType<TriggerNotificator>(FindObjectsSortMode.None);

    TriggerNotificator nearest = null;
    float nearestDistance = float.MaxValue;

    foreach (TriggerNotificator candidate in allTriggers)
    {
      float distance = Vector3.Distance(position, candidate.transform.position);
      if (distance < nearestDistance)
      {
        nearestDistance = distance;
        nearest = candidate;
      }
    }

    return nearest;
  }

  private Transform GetOrCreateNPCContainer()
  {
    Transform container = transform.Find("NPCs_Instanciados");

    if (container == null)
    {
      GameObject containerObject = new GameObject("NPCs_Instanciados");
      containerObject.transform.SetParent(transform);
      container = containerObject.transform;
    }

    return container;
  }
}