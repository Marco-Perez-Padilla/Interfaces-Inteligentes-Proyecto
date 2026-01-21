using UnityEngine;

/**
 * @file: NPCNoiseSpawner.cs
 * @brief: Cuando se le notifica un evento de 'OnHighNoiseDetected', spawnea parejas de NPCs según el prefab
 * designado en el editor. Para evitar una aparición de parejas en cada update, se implementa un cooldown de
 * 2 segundos. Además, se excluye de la zona de aparición un cono de exclusión de 60 grados con respecto al
 * vector forward de la vagoneta, y un círculo interno del jugador de 2 metros.
 *
 * Notas: Los NPCs aparecen con el script de 'VisualChasing.cs' asignado y con un tiempo de persecución de 20
 * segundos.
 */
public class NPCNoiseSpawner : MonoBehaviour
{
    [Header("NPC Settings")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int npcCount = 2;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float maxDistanceToPlayer = 50f;

    [Header("Noise Settings")]
    [SerializeField] private float minIntensity = 0.1f;

    [Header("Cooldown Settings")]
    [SerializeField] private float spawnCooldown = 2f;

    [Header("Cone Exclusion Settings")]
    [SerializeField, Range(0f, 180f)] private float coneAngle = 60f;
    [SerializeField] private Transform vagonetaTransform;
    [SerializeField] private float innerExclusionRadius = 2f;
    [SerializeField] private float chaseStartDelay = 0.25f;

    private float lastSpawnTime = 0f;

    // Suscripción al evento
    private void OnEnable()
    {
        NoiseDetector[] detectors = Object.FindObjectsByType<NoiseDetector>(FindObjectsSortMode.None);
        foreach (var detector in detectors)
        {
            detector.OnHighNoiseDetected += SpawnNPCs;
        }
    }

    // Desuscripción del evento
    private void OnDisable()
    {
        NoiseDetector[] detectors = Object.FindObjectsByType<NoiseDetector>(FindObjectsSortMode.None);
        foreach (var detector in detectors)
        {
            detector.OnHighNoiseDetected -= SpawnNPCs;
        }
    }

    // Método principal que genera los NPCs
    private void SpawnNPCs(Vector3 noisePosition, float intensity)
    {
        if (vagonetaTransform == null)
        {
            return;
        }
        
        // Si la intensidad captada por el micrófono es menor a la menor estipulada, no se generan los NPCs
        if (intensity < minIntensity)
        {
            return;
        }
        
        float dist = Vector3.Distance(transform.position, noisePosition);
        // Radio máximo desde el que puede ocurrir la generación con respecto al centro de la zona que activó el evento
        if (dist > maxDistanceToPlayer)
        {
            return;
        }
        
        // Cooldown de aparición
        if (Time.time - lastSpawnTime < spawnCooldown)
        {
            return;
        }

        // Actualizar el último tiempo de aparición para nuevo cooldown
        lastSpawnTime = Time.time;

        // Vector forward usado para el cono de exclusión
        Vector3 forward = vagonetaTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        // Número de generaciones ocurridas a la vez
        int spawned = 0;
        // Número máximo de intentos para la correcta generación de NPCs 
        int safetyCounter = 0;

        // Generar NPCs mientras no sea más de lo permitido (2 por defecto y vez) y en menos de 20 intentos (por defecto)
        while (spawned < npcCount && safetyCounter < npcCount * 10)
        {
            safetyCounter++;

            // Círculo aleatorio de generación de NPCs, radio de 5 unidades por defecto
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            // Generación basada en la zona donde se generó el ruido
            Vector3 spawnPos = noisePosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // Si la generación es dentro del círculo interno del jugador, se intenta de nuevo
            float distanceToPlayer = Vector3.Distance(spawnPos, noisePosition);
            if (distanceToPlayer < innerExclusionRadius)
            {
                continue;
            }

            Vector3 dirToSpawn = (spawnPos - noisePosition).normalized;
            dirToSpawn.y = 0f;

            float angle = Vector3.Angle(forward, dirToSpawn);

            // Se excluye también del cono de la vagoneta
            if (angle <= coneAngle * 0.5f)
            {
                continue;
            }
            
            // Instanciar el NPC con tag Enemy
            GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
            npc.tag = "Enemy";

            // Añadir Rigidbody al NPC si no lo tiene
            Rigidbody npcRb = npc.GetComponent<Rigidbody>();
            if (npcRb == null)
                npcRb = npc.AddComponent<Rigidbody>();

            // Añadir script VisualChasing al NPC, con objetivo "Player"
            VisualChasing chasingScript = npc.GetComponent<VisualChasing>();
            if (chasingScript == null)
                chasingScript = npc.AddComponent<VisualChasing>();

            chasingScript.player = GameObject.FindGameObjectWithTag("Player")?.transform;

            // Asignar el TriggerNotificator más cercano
            TriggerNotificator[] allTriggers = Object.FindObjectsByType<TriggerNotificator>(FindObjectsSortMode.None);
            TriggerNotificator closestTrigger = null;
            float closestDistance = float.MaxValue;
            
            foreach (var trigger in allTriggers)
            {
                float distance = Vector3.Distance(npc.transform.position, trigger.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTrigger = trigger;
                }
            }
            
            if (closestTrigger != null)
            {
                chasingScript.triggerZone = closestTrigger;
            }
            else
            {
                Debug.LogWarning("No se encontró ningún TriggerNotificator en la escena");
            }

            // Persecución de 20 segundos cuando se spawnea por ruido
            chasingScript.chaseDuration = 20f;
            
            // Iniciar persecución después de un delay de 0.25 segundos por defecto
            StartCoroutine(StartChaseAfterDelay(chasingScript, chaseStartDelay, spawned + 1));

            spawned++;
        }
    }

    private System.Collections.IEnumerator StartChaseAfterDelay(VisualChasing chasing, float delay, int npcNumber)
    {
        yield return new WaitForSeconds(delay);
        chasing.StartChase();
    }

// Pintar en un Gizmos el cono de exclusión y círculos -- Debug
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (vagonetaTransform == null) return;

        Vector3 origin = transform.position;
        Vector3 forward = vagonetaTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        // Cono de exclusión (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, forward * spawnRadius);

        float halfAngle = coneAngle * 0.5f;
        Vector3 left = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 right = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawRay(origin, left * spawnRadius);
        Gizmos.DrawRay(origin, right * spawnRadius);

        // Círculo interno de exclusión (rojo)
        Gizmos.color = Color.red;
        DrawCircle(origin, innerExclusionRadius, 32);

        // Radio de spawn (amarillo)
        Gizmos.color = Color.yellow;
        DrawCircle(origin, spawnRadius, 32);

        // Radio de detección máximo (azul)
        Gizmos.color = Color.blue;
        DrawCircle(origin, maxDistanceToPlayer, 32);
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius, 
                0, 
                Mathf.Sin(angle) * radius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
#endif
}