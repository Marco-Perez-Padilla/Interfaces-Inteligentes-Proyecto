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
    [Tooltip("Tamaño del BoxCollider trigger añadido a cada pieza.")]
    [SerializeField] private Vector3 triggerSize = new Vector3(4f, 3f, 4f);

    [Tooltip("Centro del BoxCollider respecto a la pieza.")]
    [SerializeField] private Vector3 triggerCenter = new Vector3(0f, 1.5f, 0f);

    [Header("References")]
    [Tooltip("El GameObject padre que contiene todas las piezas instanciadas.")]
    [SerializeField] private GameObject piecesContainer;

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
        if (piecesContainer == null)
        {
            Debug.LogError("[PieceTriggerSetup] No hay piecesContainer asignado.");
            return;
        }

        int count = 0;

        foreach (Transform piece in piecesContainer.transform)
        {
            // Saltar si ya tiene TriggerNotificator
            if (piece.GetComponent<TriggerNotificator>() != null)
                continue;

            // Añadir BoxCollider
            BoxCollider box = piece.gameObject.AddComponent<BoxCollider>();
            box.size = triggerSize;
            box.center = triggerCenter;
            box.isTrigger = true;

            // Añadir TriggerNotificator (ya busca el BoxCollider en Awake)
            piece.gameObject.AddComponent<TriggerNotificator>();

            count++;
        }

        Debug.Log($"[PieceTriggerSetup] Triggers añadidos a {count} piezas.");
    }
}