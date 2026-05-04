using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Construye la superficie visual del path instanciando planos de nodo y de conexión
/// a partir del grafo generado por PathGenerator. Opcionalmente añade TriggerNotificator
/// a un subconjunto aleatorio de las piezas de suelo generadas, lo que permite que
/// EnemyInitializer los encuentre al arrancar la escena.
/// </summary>
[ExecuteAlways]
public class PathSurfaceBuilder : MonoBehaviour
{
  // =====================================================
  // REFERENCES
  // =====================================================

  [Header("References")]
  [SerializeField] private PathGenerator pathGenerator;

  [Header("Prefabs")]
  [SerializeField] private GameObject nodePlanePrefab;
  [SerializeField] private GameObject connectionPlanePrefab;

  // =====================================================
  // SIZES (LOGICAL)
  // =====================================================

  [Header("Sizes")]
  [Tooltip("Tamaño lógico del nodo (en unidades del mundo)")]
  [SerializeField] private float nodeSize = 0.4f;

  [Tooltip("Ancho lógico de la conexión")]
  [SerializeField] private float connectionWidth = 0.2f;

  // =====================================================
  // TRIGGER NOTIFICATOR
  // =====================================================

  [Header("Trigger Notificator")]
  [Tooltip("Probabilidad de que una pieza de suelo reciba un TriggerNotificator. 0 = nunca, 1 = siempre.")]
  [Range(0f, 1f)]
  [SerializeField] private float triggerSpawnProbability = 0.3f;

  [Tooltip("Tamaño del collider BoxCollider añadido al TriggerNotificator. " +
           "Ajustar para que cubra el ancho del corredor.")]
  [SerializeField] private Vector3 triggerColliderSize = new Vector3(1f, 2f, 1f);

  // =====================================================
  // INTERNAL
  // =====================================================

  private float nodeMeshSize = 1f;
  private float connectionMeshLength = 1f;

#if UNITY_EDITOR
    private bool pendingRebuild;
#endif

  // =====================================================
  // UNITY
  // =====================================================

  void OnEnable() => RequestRebuild();
  void OnValidate() => RequestRebuild();
  void Start() => Rebuild();

#if UNITY_EDITOR
    void RequestRebuild()
    {
        if (pendingRebuild) return;
        pendingRebuild = true;
        EditorApplication.delayCall += DelayedRebuild;
    }

    void DelayedRebuild()
    {
        pendingRebuild = false;
        if (this == null) return;
        Rebuild();
    }
#endif

  // =====================================================
  // CORE
  // =====================================================

  void Rebuild()
  {
    if (pathGenerator == null || pathGenerator.graph == null)
      return;

    CacheMeshSizes();
    ClearChildren();

    BuildPath(pathGenerator.graph.mainPath);

    if (pathGenerator.graph.subPaths != null)
    {
      foreach (List<PathNode> subPath in pathGenerator.graph.subPaths)
        BuildPath(subPath);
    }
  }

  void ClearChildren()
  {
    for (int i = transform.childCount - 1; i >= 0; i--)
    {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(transform.GetChild(i).gameObject);
            else
#endif
      Destroy(transform.GetChild(i).gameObject);
    }
  }

  // =====================================================
  // BUILD PATH
  // =====================================================

  void BuildPath(List<PathNode> path)
  {
    if (path == null || path.Count < 1)
      return;

    for (int i = 0; i < path.Count; i++)
    {
      SpawnNode(path[i].position);

      if (i < path.Count - 1)
        SpawnConnection(path[i].position, path[i + 1].position);
    }
  }

  // =====================================================
  // NODE
  // =====================================================

  void SpawnNode(Vector3 position)
  {
    GameObject floorPiece = Instantiate(
        nodePlanePrefab,
        position,
        Quaternion.identity,
        transform
    );

    float scale = nodeSize / nodeMeshSize;
    floorPiece.transform.localScale = new Vector3(scale, 1f, scale);

    TryAddTriggerNotificator(floorPiece);
  }

  // =====================================================
  // CONNECTION
  // =====================================================

  void SpawnConnection(Vector3 from, Vector3 to)
  {
    Vector3 delta = to - from;
    float realDistance = delta.magnitude;

    if (realDistance < 0.001f)
      return;

    Vector3 direction = delta.normalized;
    float logicalLength = Mathf.Max(0f, realDistance - nodeSize);

    if (logicalLength <= 0.001f)
      return;

    Vector3 center = from + direction * (realDistance * 0.5f);
    Quaternion rotation = Quaternion.LookRotation(direction);

    GameObject floorPiece = Instantiate(
        connectionPlanePrefab,
        center,
        rotation,
        transform
    );

    float scaleZ = logicalLength / connectionMeshLength;
    float scaleX = connectionWidth / nodeMeshSize;
    floorPiece.transform.localScale = new Vector3(scaleX, 1f, scaleZ);

    TryAddTriggerNotificator(floorPiece);
  }

  // =====================================================
  // TRIGGER NOTIFICATOR
  // =====================================================

  /// <summary>
  /// Con probabilidad triggerSpawnProbability, añade un TriggerNotificator
  /// y un BoxCollider trigger al GameObject de suelo recibido.
  /// El BoxCollider se configura con triggerColliderSize para cubrir
  /// el espacio del corredor en esa posición.
  /// Solo se ejecuta en play mode para no contaminar la escena en el editor.
  /// </summary>
  /// <param name="floorPiece">GameObject de suelo recién instanciado.</param>
  private void TryAddTriggerNotificator(GameObject floorPiece)
  {
    if (!Application.isPlaying)
      return;

    if (Random.value > triggerSpawnProbability)
      return;

    BoxCollider triggerCollider = floorPiece.AddComponent<BoxCollider>();
    triggerCollider.isTrigger = true;
    triggerCollider.size = triggerColliderSize;

    floorPiece.AddComponent<TriggerNotificator>();
  }

  // =====================================================
  // MESH SIZE CACHE
  // =====================================================

  void CacheMeshSizes()
  {
    if (nodePlanePrefab != null)
    {
      MeshFilter meshFilter = nodePlanePrefab.GetComponent<MeshFilter>();
      if (meshFilter != null && meshFilter.sharedMesh != null)
        nodeMeshSize = meshFilter.sharedMesh.bounds.size.x;
    }

    if (connectionPlanePrefab != null)
    {
      MeshFilter meshFilter = connectionPlanePrefab.GetComponent<MeshFilter>();
      if (meshFilter != null && meshFilter.sharedMesh != null)
        connectionMeshLength = meshFilter.sharedMesh.bounds.size.z;
    }
  }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (pathGenerator == null || pathGenerator.graph == null)
            return;

        DrawTriggerGizmosForPath(pathGenerator.graph.mainPath);

        if (pathGenerator.graph.subPaths != null)
        {
            foreach (List<PathNode> subPath in pathGenerator.graph.subPaths)
                DrawTriggerGizmosForPath(subPath);
        }
    }

    /// <summary>
    /// Dibuja gizmos para nodos y conexiones del path.
    /// Verde semitransparente = recibirá trigger en runtime (probabilidad determinista).
    /// Gris = no recibirá trigger.
    /// La semilla usa la posición del nodo para ser estable entre frames
    /// y reproducible entre editor y runtime con la misma semilla de generación.
    /// </summary>
    private void DrawTriggerGizmosForPath(List<PathNode> path)
    {
        if (path == null) return;

        for (int nodeIndex = 0; nodeIndex < path.Count; nodeIndex++)
        {
            Vector3 nodePosition = path[nodeIndex].position;
            bool    nodeHasTrigger = ComputeDeterministicTrigger(nodePosition, nodeIndex);

            DrawTriggerGizmo(nodePosition, Quaternion.identity, nodeHasTrigger);

            if (nodeIndex < path.Count - 1)
            {
                Vector3 nextPosition   = path[nodeIndex + 1].position;
                Vector3 connectionCenter = (nodePosition + nextPosition) * 0.5f;
                Quaternion connectionRotation = Quaternion.LookRotation((nextPosition - nodePosition).normalized);
                bool connectionHasTrigger = ComputeDeterministicTrigger(connectionCenter, nodeIndex + 1000);

                DrawTriggerGizmo(connectionCenter, connectionRotation, connectionHasTrigger);
            }
        }
    }

    /// <summary>
    /// Dibuja un cubo gizmo en la posición indicada.
    /// Si hasTrigger es true, lo dibuja verde con etiqueta TRIGGER.
    /// Si es false, lo dibuja gris tenue para que se vea el path completo.
    /// </summary>
    private void DrawTriggerGizmo(Vector3 position, Quaternion rotation, bool hasTrigger)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

        if (hasTrigger)
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.15f);
            Gizmos.DrawCube(Vector3.zero, triggerColliderSize);

            Gizmos.color = new Color(0f, 1f, 0.5f, 0.9f);
            Gizmos.DrawWireCube(Vector3.zero, triggerColliderSize);

            Handles.Label(
                position + Vector3.up * (triggerColliderSize.y * 0.5f + 0.1f),
                "TRIGGER",
                EditorStyles.miniLabel
            );
        }
        else
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.05f);
            Gizmos.DrawWireCube(Vector3.zero, triggerColliderSize);
        }

        Gizmos.matrix = originalMatrix;
    }

    /// <summary>
    /// Determina de forma determinista si una posición recibirá trigger,
    /// usando su posición mundial como semilla. Mismo resultado en editor
    /// y en runtime siempre que el grafo se genere con la misma semilla.
    /// </summary>
    private bool ComputeDeterministicTrigger(Vector3 position, int index)
    {
        float pseudoRandom = Mathf.Abs(Mathf.Sin(
            position.x * 73.1f +
            position.y * 157.3f +
            position.z * 241.7f +
            index      * 311.9f
        ));

        return pseudoRandom <= triggerSpawnProbability;
    }
#endif
}