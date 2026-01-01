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

        for (int i = 0; i < npcCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = noisePosition + new Vector3(randomCircle.x, 0, randomCircle.y);

            Instantiate(npcPrefab, spawnPos, Quaternion.identity);
        }
    }
}
