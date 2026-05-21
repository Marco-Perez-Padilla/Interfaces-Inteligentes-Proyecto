using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawner central de enemigos. Se suscribe a todos los TriggerNotificator
/// y NoiseDetector de la escena para instanciar enemigos bajo dos condiciones:
///
///   1. El jugador entra en una TriggerZone → aparece un enemigo que lo persigue.
///   2. El jugador hace mucho ruido en una NoiseZone → aparecen varios enemigos.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs de enemigos")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Player Reference")]
    [Tooltip("Arrastra aquí el XR Origin o el objeto que se mueve en escena.")]
    [SerializeField] private Transform playerTransform;

    [Header("Spawn por TriggerZone")]
    [SerializeField] private int enemiesPerTrigger = 1;
    [SerializeField] private float triggerSpawnOffsetRadius = 2f;

    [Header("Spawn por Ruido")]
    [SerializeField] private int enemiesPerNoise = 2;
    [SerializeField] private float noiseSpawnRadius = 5f;
    [SerializeField] private float noiseInnerExclusionRadius = 2f;
    [SerializeField] private float noiseSpawnCooldown = 2f;
    [SerializeField] private Transform vagonetaTransform;
    [Range(0f, 180f)]
    [SerializeField] private float coneExclusionAngle = 60f;

    [Header("Offset en Y")]
    [SerializeField] private float spawnHeightOffset = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool spawnOnStartForDebug = false;
    [SerializeField] private int debugSpawnCount = 5;

    private float lastNoiseSpawnTime = float.MinValue;

    private Dictionary<TriggerNotificator, TriggerNotificator.TriggerEvent> triggerDelegates
        = new Dictionary<TriggerNotificator, TriggerNotificator.TriggerEvent>();

    // =====================================================
    // UNITY
    // =====================================================

    void OnEnable()
    {
        LegacyPieceApplier.OnPiecesInstantiated += OnPiecesReady;
    }

    void OnDisable()
    {
        LegacyPieceApplier.OnPiecesInstantiated -= OnPiecesReady;
    }

    void OnPiecesReady()
    {
        // Si no se asignó el player manualmente, buscar por tag
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                playerTransform = playerObject.transform;
            else
                Debug.LogWarning("[EnemySpawner] No se encontró objeto con tag Player. Asígnalo en el Inspector.");
        }

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
            TriggerNotificator capturedTrigger = trigger;
            TriggerNotificator.TriggerEvent triggerDelegate = () => OnTriggerZoneEntered(capturedTrigger);
            triggerDelegates[trigger] = triggerDelegate;
            trigger.OnPlayerEntered += triggerDelegate;
        }

        Debug.Log($"[EnemySpawner] Suscrito a {allTriggers.Length} TriggerNotificators.");
    }

    private void UnsubscribeFromAllTriggers()
    {
        foreach (var entry in triggerDelegates)
            if (entry.Key != null)
                entry.Key.OnPlayerEntered -= entry.Value;
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

    private void OnTriggerZoneEntered(TriggerNotificator sourceTrigger)
    {
        for (int i = 0; i < enemiesPerTrigger; i++)
        {
            Vector3 spawnPosition = FindSafeSpawnPosition(sourceTrigger.transform.position);
            SpawnEnemy(spawnPosition, sourceTrigger);
        }
    }

    private Vector3 FindSafeSpawnPosition(Vector3 origin)
    {
        float minDistanceFromPlayer = 3f; // configúralo en el inspector si quieres
        int maxAttempts = 20;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 offset = Random.insideUnitCircle * triggerSpawnOffsetRadius;
            Vector3 candidate = origin + new Vector3(offset.x, spawnHeightOffset, offset.y);

            if (playerTransform == null || 
                Vector3.Distance(candidate, playerTransform.position) >= minDistanceFromPlayer)
                return candidate;
        }

        // Fallback: detrás del jugador
        Vector3 behindPlayer = playerTransform != null
            ? playerTransform.position - playerTransform.forward * minDistanceFromPlayer
            : origin;
        return behindPlayer + Vector3.up * spawnHeightOffset;
    }

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

            if (Vector3.Distance(candidatePos, noisePosition) < noiseInnerExclusionRadius)
                continue;

            Vector3 dirToCandidate = Vector3.ProjectOnPlane((candidatePos - noisePosition).normalized, Vector3.up);
            if (Vector3.Angle(vagonetaForward, dirToCandidate) <= coneExclusionAngle * 0.5f)
                continue;

            TriggerNotificator nearestTrigger = FindNearestTrigger(candidatePos);
            SpawnEnemy(candidatePos, nearestTrigger);
            spawnedCount++;
        }
    }

    // =====================================================
    // SPAWN
    // =====================================================

    private void SpawnEnemy(Vector3 position, TriggerNotificator nearestTrigger)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("[EnemySpawner] No hay prefabs de enemigos asignados.");
            return;
        }

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject spawnedEnemy = Instantiate(prefab, position, Quaternion.identity, GetOrCreateNPCContainer());

        // Añadir EnemyInitializer si el prefab no lo tiene
        EnemyInitializer initializer = spawnedEnemy.GetComponent<EnemyInitializer>();
        if (initializer == null)
            initializer = spawnedEnemy.AddComponent<EnemyInitializer>();

        initializer.OverrideNearestTrigger(nearestTrigger);

        // Inyectar player directamente si lo tenemos
        if (playerTransform != null)
            initializer.OverridePlayerTransform(playerTransform);

        Debug.Log($"[EnemySpawner] Spawned: {spawnedEnemy.name}");
    }

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