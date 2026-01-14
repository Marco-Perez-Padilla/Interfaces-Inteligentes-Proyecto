using UnityEngine;

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
    public Vector3 forwardDirection = Vector3.forward;
    [Range(0f, 180f)]
    [SerializeField] public float coneAngle = 60f;

    private float lastSpawnTime = 0f;

    private void OnEnable()
    {
        NoiseDetector[] detectors = Object.FindObjectsByType<NoiseDetector>(FindObjectsSortMode.None);
        foreach (var detector in detectors)
        {
            detector.OnHighNoiseDetected += SpawnNPCs;
        }
    }

    private void OnDisable()
    {
        NoiseDetector[] detectors = Object.FindObjectsByType<NoiseDetector>(FindObjectsSortMode.None);
        foreach (var detector in detectors)
        {
            detector.OnHighNoiseDetected -= SpawnNPCs;
        }
    }

    private void SpawnNPCs(Vector3 noisePosition, float intensity)
    {
        if (intensity < minIntensity) return;
        if (Vector3.Distance(transform.position, noisePosition) > maxDistanceToPlayer) return;
        if (Time.time - lastSpawnTime < spawnCooldown) return;

        lastSpawnTime = Time.time;

        Vector3 forward = forwardDirection.normalized;

        int spawned = 0;
        int safetyCounter = 0;

        // Evita bucles infinitos si el cono es muy grande
        while (spawned < npcCount && safetyCounter < npcCount * 10)
        {
            safetyCounter++;

            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = noisePosition + new Vector3(randomCircle.x, 0, randomCircle.y);

            Vector3 dirToSpawn = (spawnPos - noisePosition).normalized;

            float angle = Vector3.Angle(forward, dirToSpawn);

            // Si está dentro del cono, se descarta
            if (angle <= coneAngle * 0.5f)
                continue;

            Instantiate(npcPrefab, spawnPos, Quaternion.identity);
            spawned++;
        }
    }

#if UNITY_EDITOR
    // Gizmos para visualizar el cono
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = transform.position;
        Vector3 forward = forwardDirection.normalized;

        Gizmos.DrawRay(origin, forward * spawnRadius);

        float halfAngle = coneAngle * 0.5f;
        Vector3 left = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 right = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawRay(origin, left * spawnRadius);
        Gizmos.DrawRay(origin, right * spawnRadius);
    }
#endif
}
