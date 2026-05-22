using UnityEngine;

/// <summary>
/// Se suscribe a LegacyPieceApplier.OnPiecesInstantiated y añade
/// automáticamente un BoxCollider + TriggerNotificator a cada pieza
/// del mapa para que EnemySpawner pueda detectar al jugador.
///
/// Coloca este script en un GameObject vacío en la escena junto al
/// resto de managers (EnemySpawner, LegacyPieceApplier, etc).
/// </summary>
public class PieceTriggerSetup : MonoBehaviour
{
    [Header("Collider Settings")]
    [SerializeField] private Vector3 triggerSize = new Vector3(4f, 3f, 4f);
    [SerializeField] private Vector3 triggerCenter = new Vector3(0f, 1.5f, 0f);

    [Header("References")]
    [SerializeField] private GameObject piecesContainer;

    [Header("Zone Type")]
    [SerializeField] private ZoneMode zoneMode = ZoneMode.Random;
    [Range(0f, 1f)]
    [SerializeField] private float noiseZoneChance = 0.3f;

    [Header("NPCNoiseSpawner Settings")]
    [Tooltip("Prefab de enemigo para zonas de ruido.")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private Transform vagonetaTransform;
    [SerializeField] private int npcCount = 2;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float spawnCooldown = 2f;
    [SerializeField] private float coneAngle = 60f;
    [SerializeField] private float innerExclusionRadius = 2f;

    private enum ZoneMode
    {
        AlwaysTrigger,
        AlwaysNoise,
        AlwaysBoth,
        Random
    }

    void OnEnable() => LegacyPieceApplier.OnPiecesInstantiated += OnPiecesReady;
    void OnDisable() => LegacyPieceApplier.OnPiecesInstantiated -= OnPiecesReady;

    void OnPiecesReady()
    {
        if (piecesContainer == null)
        {
            Debug.LogError("[PieceTriggerSetup] No hay piecesContainer asignado.");
            return;
        }

        int count = 0;

        foreach (Transform piece in piecesContainer.transform)
        {
            if (piece.name.StartsWith("Corridor_")) continue;

            BoxCollider box = piece.GetComponent<BoxCollider>() ?? piece.gameObject.AddComponent<BoxCollider>();
            box.size = triggerSize;
            box.center = triggerCenter;
            box.isTrigger = true;

            bool hasTrigger = piece.GetComponent<TriggerNotificator>() != null;
            bool hasNoise = piece.GetComponent<NoiseDetector>() != null;

            switch (zoneMode)
            {
                case ZoneMode.AlwaysTrigger:
                    if (!hasTrigger) piece.gameObject.AddComponent<TriggerNotificator>();
                    break;

                case ZoneMode.AlwaysNoise:
                    if (!hasNoise) AddNoiseZone(piece);
                    break;

                case ZoneMode.AlwaysBoth:
                    if (!hasTrigger) piece.gameObject.AddComponent<TriggerNotificator>();
                    if (!hasNoise) AddNoiseZone(piece);
                    break;

                case ZoneMode.Random:
                    if (Random.value < noiseZoneChance)
                    {
                        if (!hasNoise) AddNoiseZone(piece);
                    }
                    else
                    {
                        if (!hasTrigger) piece.gameObject.AddComponent<TriggerNotificator>();
                    }
                    break;
            }

            count++;
        }

        Debug.Log($"[PieceTriggerSetup] Zonas configuradas: {count} piezas.");
    }

    private void AddNoiseZone(Transform piece)
    {
        piece.gameObject.AddComponent<NoiseDetector>();

        // Solo añadir NPCNoiseSpawner si hay prefab configurado
        if (npcPrefab == null)
        {
            Debug.LogWarning($"[PieceTriggerSetup] {piece.name} es zona de ruido pero npcPrefab no está asignado.");
            return;
        }

        NPCNoiseSpawner spawner = piece.gameObject.AddComponent<NPCNoiseSpawner>();
        spawner.Setup(npcPrefab, vagonetaTransform, npcCount, spawnRadius, spawnCooldown, coneAngle, innerExclusionRadius);
    }
}