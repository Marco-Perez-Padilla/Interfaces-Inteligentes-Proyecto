using UnityEngine;

/**
 * @file: PieceTriggerSetup.cs
 * @brief: Configura zonas trigger y de ruido para las piezas del mapa.
 *
 * Notas:
 * - Las zonas trigger notifican cuándo el jugador entra/sale de ellas.
 * - Las zonas de ruido generan eventos de detección de sonido.
 */

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

    /// <summary>
    /// Modos de configuración de zonas:
    /// - AlwaysTrigger: Todas las piezas tienen TriggerNotificator.
    /// - AlwaysNoise: Todas las piezas tienen NoiseDetector.
    /// - AlwaysBoth: Todas las piezas tienen ambos componentes.
    /// - Random: Cada pieza tiene una probabilidad de ser zona de ruido o trigger.
    /// </summary>
    private enum ZoneMode
    {
        AlwaysTrigger,
        AlwaysNoise,
        AlwaysBoth,
        Random
    }

    /// <summary>
    /// Se suscribe al evento de instanciación de piezas para configurar las zonas trigger y de ruido una vez que las piezas estén listas en la escena.
    /// Se desuscribe al deshabilitar para evitar fugas de memoria.
    /// </summary>
    void OnEnable() => LegacyPieceApplier.OnPiecesInstantiated += OnPiecesReady;
    void OnDisable() => LegacyPieceApplier.OnPiecesInstantiated -= OnPiecesReady;

    /// <summary>
    /// Configura las zonas trigger y de ruido para cada pieza del mapa según el modo seleccionado
    /// y las opciones configuradas. Añade los componentes necesarios a cada pieza y cuenta cuántas zonas se han configurado.
    /// </summary>
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

    ///  <summary>
    /// Añade un NoiseDetector a la pieza y, si npcPrefab está asignado, también añade un NPCNoiseSpawner configurado con las opciones especificadas para generar enemigos cuando se detecte ruido en esa zona.
    /// </summary>
    private void AddNoiseZone(Transform piece)
    {
        piece.gameObject.AddComponent<NoiseDetector>();

        if (npcPrefab == null)
        {
            Debug.LogWarning($"[PieceTriggerSetup] {piece.name} es zona de ruido pero npcPrefab no está asignado.");
            return;
        }

        NPCNoiseSpawner spawner = piece.gameObject.AddComponent<NPCNoiseSpawner>();
        spawner.Setup(npcPrefab, vagonetaTransform, npcCount, spawnRadius, spawnCooldown, coneAngle, innerExclusionRadius);
    }
}