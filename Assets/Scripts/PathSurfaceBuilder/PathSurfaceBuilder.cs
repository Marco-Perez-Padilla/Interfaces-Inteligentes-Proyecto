using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Construye la superficie visual del path instanciando piezas de rail
/// a lo largo de cada corredor entre nodos. Cada corredor se agrupa bajo
/// un GameObject hijo propio para facilitar su gestión en escena.
///
/// Las piezas se repiten (modo Tile) en lugar de escalarse, de forma que
/// el prefab mantiene sus proporciones originales. La longitud útil del
/// corredor se recorta en ambos extremos (nodeMargin) para dejar espacio
/// a las piezas de nodo entre corredores.
///
/// Opcionalmente añade TriggerNotificator a un subconjunto aleatorio de
/// las piezas instanciadas, lo que permite que EnemyInitializer los
/// encuentre al arrancar la escena.
/// </summary>
[ExecuteAlways]
public class PathSurfaceBuilder : MonoBehaviour
{
  // =====================================================
  // REFERENCES
  // =====================================================

  [Header("References")]
  [SerializeField] private PathGenerator pathGenerator;

  // =====================================================
  // PREFABS
  // =====================================================

  [Header("Prefabs")]
  [SerializeField] private GameObject railPrefab;

  // =====================================================
  // RAIL AXIS
  // =====================================================

  [Header("Rail Axis")]
  [Tooltip("Eje LOCAL del prefab alineado con la dirección de avance del rail.")]
  [SerializeField] private RailAxis railForwardAxis = RailAxis.X;

  // =====================================================
  // NODE MARGIN
  // =====================================================

  [Header("Node Margin")]
  [Tooltip("Espacio reservado en cada extremo del corredor para las piezas de nodo. " +
           "Normalmente la mitad del spacing del PathGenerator.")]
  [SerializeField] private float nodeMargin = 0.5f;

  // =====================================================
  // TRIGGER NOTIFICATOR
  // =====================================================

  [Header("Trigger Notificator")]
  [Tooltip("Probabilidad de que una pieza reciba un TriggerNotificator. 0 = nunca, 1 = siempre.")]
  [Range(0f, 1f)]
  [SerializeField] private float triggerSpawnProbability = 0.3f;

  [Tooltip("Tamaño del BoxCollider trigger añadido al TriggerNotificator.")]
  [SerializeField] private Vector3 triggerColliderSize = new Vector3(1f, 2f, 1f);

  // =====================================================
  // INTERNAL
  // =====================================================

  /// <summary>Longitud del mesh del rail medida sobre su eje de avance.</summary>
  private float railMeshLength = 1f;

#if UNITY_EDITOR
  private bool pendingRebuild;
#endif

  // =====================================================
  // RAIL AXIS ENUM
  // =====================================================

  /// <summary>
  /// Eje local del prefab que apunta en la dirección de avance del rail.
  /// </summary>
  public enum RailAxis
  {
    /// <summary>El eje X local (Vector3.right) apunta hacia adelante.</summary>
    X,
    /// <summary>El eje Y local (Vector3.up) apunta hacia adelante.</summary>
    Y,
    /// <summary>El eje Z local (Vector3.forward) apunta hacia adelante.</summary>
    Z,
  }

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
#else
  void RequestRebuild() => Rebuild();
#endif

  // =====================================================
  // CORE
  // =====================================================

  /// <summary>
  /// Regenera toda la geometría de rails destruyendo los hijos actuales
  /// y reconstruyendo uno por camino del grafo.
  /// </summary>
  void Rebuild()
  {
    if (pathGenerator == null || pathGenerator.graph == null)
      return;

    CacheRailMeshLength();
    ClearChildren();

    BuildPathRails(pathGenerator.graph.mainPath, "MainPath");

    if (pathGenerator.graph.subPaths != null)
    {
      int subPathIndex = 0;
      foreach (List<PathNode> subPath in pathGenerator.graph.subPaths)
      {
        BuildPathRails(subPath, $"SubPath_{subPathIndex}");
        subPathIndex++;
      }
    }
  }

  /// <summary>
  /// Destruye todos los GameObjects hijos de este transform.
  /// Usa DestroyImmediate en el editor para no dejar objetos huérfanos.
  /// </summary>
  void ClearChildren()
  {
    for (int childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
    {
#if UNITY_EDITOR
      if (!Application.isPlaying)
        DestroyImmediate(transform.GetChild(childIndex).gameObject);
      else
#endif
      Destroy(transform.GetChild(childIndex).gameObject);
    }
  }

  // =====================================================
  // BUILD PATH
  // =====================================================

  /// <summary>
  /// Itera los nodos del camino e instancia un corredor entre cada par
  /// de nodos consecutivos, agrupado bajo un GameObject hijo con el
  /// nombre proporcionado.
  /// </summary>
  /// <param name="path">Lista de nodos que forman el camino.</param>
  /// <param name="pathName">Nombre base para el GameObject padre del camino.</param>
  void BuildPathRails(List<PathNode> path, string pathName)
  {
    if (path == null || path.Count < 2)
      return;

    GameObject pathParent = new GameObject(pathName);
    pathParent.transform.SetParent(transform, worldPositionStays: false);

    for (int nodeIndex = 0; nodeIndex < path.Count - 1; nodeIndex++)
    {
      Vector3 fromPosition = path[nodeIndex].position;
      Vector3 toPosition = path[nodeIndex + 1].position;

      SpawnCorridor(fromPosition, toPosition, nodeIndex, pathParent.transform);
    }
  }

  // =====================================================
  // CORRIDOR
  // =====================================================

  /// <summary>
  /// Instancia N copias del prefab de rail a lo largo del segmento
  /// [fromPosition, toPosition], recortando nodeMargin en cada extremo.
  /// Todas las piezas quedan agrupadas bajo un GameObject hijo propio.
  /// </summary>
  /// <param name="fromPosition">Posición mundial del nodo de origen.</param>
  /// <param name="toPosition">Posición mundial del nodo de destino.</param>
  /// <param name="corridorIndex">Índice del corredor dentro del camino, usado para nombres.</param>
  /// <param name="pathParent">Transform padre bajo el que se crea el grupo.</param>
  void SpawnCorridor(
    Vector3 fromPosition,
    Vector3 toPosition,
    int corridorIndex,
    Transform pathParent)
  {
    Vector3 delta = toPosition - fromPosition;
    float totalDistance = delta.magnitude;

    if (totalDistance < 0.001f)
      return;

    // Longitud útil del corredor descontando el margen de nodo en cada extremo.
    float usableLength = totalDistance - (nodeMargin * 2f);

    if (usableLength <= 0.001f)
      return;

    if (railMeshLength <= 0.001f)
      return;

    Vector3 direction = delta.normalized;
    Quaternion railRotation = ComputeRailRotation(direction);
    Vector3 corridorOrigin = fromPosition + direction * nodeMargin;

    // Número de piezas que caben en la longitud útil.
    int pieceCount = Mathf.Max(1, Mathf.RoundToInt(usableLength / railMeshLength));

    // Tamaño real de cada pieza ajustado para que encajen exactamente.
    float adjustedPieceLength = usableLength / pieceCount;

    // Agrupador de este corredor.
    GameObject corridorParent = new GameObject($"Corridor_{corridorIndex}");
    corridorParent.transform.SetParent(pathParent, worldPositionStays: false);

    for (int pieceIndex = 0; pieceIndex < pieceCount; pieceIndex++)
    {
      // Centro de cada pieza a lo largo del corredor.
      float distanceAlongCorridor = (pieceIndex + 0.5f) * adjustedPieceLength;
      Vector3 piecePosition = corridorOrigin + direction * distanceAlongCorridor;

      GameObject railPiece = Instantiate(
          railPrefab,
          piecePosition,
          railRotation,
          corridorParent.transform
      );

      // Escala solo el eje de avance para ajustar la longitud de la pieza,
      // sin deformar el ancho ni el alto del rail.
      ScaleRailPieceLength(railPiece, adjustedPieceLength);

      TryAddTriggerNotificator(railPiece);
    }
  }

  // =====================================================
  // ROTATION
  // =====================================================

  /// <summary>
  /// Calcula la rotación que alinea el eje de avance del prefab
  /// con la dirección del corredor.
  /// </summary>
  /// <param name="corridorDirection">Dirección normalizada del corredor.</param>
  /// <returns>Rotación mundial para el prefab instanciado.</returns>
  Quaternion ComputeRailRotation(Vector3 corridorDirection)
{
  switch (railForwardAxis)
  {
    case RailAxis.X:
      // LookRotation apunta Z hacia corridorDirection.
      // Multiplicamos por -90° en Y para que sea X quien apunte hacia adelante,
      // manteniendo el eje Up como referencia y evitando roll indeseado.
      return Quaternion.LookRotation(corridorDirection, Vector3.up)
           * Quaternion.Euler(0f, -90f, 0f);

    case RailAxis.Y:
      return Quaternion.FromToRotation(Vector3.up, corridorDirection);

    case RailAxis.Z:
    default:
      return Quaternion.LookRotation(corridorDirection, Vector3.up);
  }
}

  // =====================================================
  // SCALE
  // =====================================================

  /// <summary>
  /// Ajusta la escala de la pieza sobre su eje de avance para que su
  /// longitud encaje exactamente en el hueco asignado. El ancho y el
  /// alto se dejan intactos para no deformar el mesh.
  /// </summary>
  /// <param name="railPiece">GameObject instanciado del rail.</param>
  /// <param name="targetLength">Longitud deseada en unidades del mundo.</param>
  void ScaleRailPieceLength(GameObject railPiece, float targetLength)
  {
    float lengthScale = targetLength / railMeshLength;
    Vector3 currentScale = railPiece.transform.localScale;

    switch (railForwardAxis)
    {
      case RailAxis.X:
        railPiece.transform.localScale =
            new Vector3(lengthScale, currentScale.y, currentScale.z);
        break;

      case RailAxis.Y:
        railPiece.transform.localScale =
            new Vector3(currentScale.x, lengthScale, currentScale.z);
        break;

      case RailAxis.Z:
      default:
        railPiece.transform.localScale =
            new Vector3(currentScale.x, currentScale.y, lengthScale);
        break;
    }
  }

  // =====================================================
  // TRIGGER NOTIFICATOR
  // =====================================================

  /// <summary>
  /// Con probabilidad triggerSpawnProbability, añade un TriggerNotificator
  /// y un BoxCollider trigger al GameObject de rail recibido.
  /// Solo se ejecuta en play mode para no contaminar la escena en el editor.
  /// </summary>
  /// <param name="railPiece">GameObject de rail recién instanciado.</param>
  private void TryAddTriggerNotificator(GameObject railPiece)
  {
    if (!Application.isPlaying)
      return;

    if (Random.value > triggerSpawnProbability)
      return;

    BoxCollider triggerCollider = railPiece.AddComponent<BoxCollider>();
    triggerCollider.isTrigger = true;
    triggerCollider.size = triggerColliderSize;

    railPiece.AddComponent<TriggerNotificator>();
  }

  // =====================================================
  // MESH SIZE CACHE
  // =====================================================

  /// <summary>
  /// Lee la longitud del mesh del prefab de rail sobre su eje de avance
  /// y la cachea para usarla en los cálculos de tiling.
  /// </summary>
  void CacheRailMeshLength()
  {
    if (railPrefab == null)
      return;

    MeshFilter meshFilter = railPrefab.GetComponent<MeshFilter>();

    if (meshFilter == null || meshFilter.sharedMesh == null)
      return;

    Bounds meshBounds = meshFilter.sharedMesh.bounds;

    switch (railForwardAxis)
    {
      case RailAxis.X:
        railMeshLength = meshBounds.size.x;
        break;

      case RailAxis.Y:
        railMeshLength = meshBounds.size.y;
        break;

      case RailAxis.Z:
      default:
        railMeshLength = meshBounds.size.z;
        break;
    }

    if (railMeshLength <= 0.001f)
      railMeshLength = 1f;
  }

  // =====================================================
  // GIZMOS
  // =====================================================

#if UNITY_EDITOR
  void OnDrawGizmos()
  {
    if (pathGenerator == null || pathGenerator.graph == null)
      return;

    DrawPathGizmos(pathGenerator.graph.mainPath);

    if (pathGenerator.graph.subPaths != null)
    {
      foreach (List<PathNode> subPath in pathGenerator.graph.subPaths)
        DrawPathGizmos(subPath);
    }
  }

  /// <summary>
  /// Dibuja gizmos de trigger para todos los corredores de un camino.
  /// </summary>
  /// <param name="path">Camino a dibujar.</param>
  private void DrawPathGizmos(List<PathNode> path)
  {
    if (path == null || path.Count < 2)
      return;

    for (int nodeIndex = 0; nodeIndex < path.Count - 1; nodeIndex++)
    {
      Vector3 fromPosition = path[nodeIndex].position;
      Vector3 toPosition   = path[nodeIndex + 1].position;

      DrawCorridorTriggerGizmos(fromPosition, toPosition, nodeIndex);
    }
  }

  /// <summary>
  /// Dibuja cubos gizmo en las posiciones de cada pieza de rail del
  /// corredor, indicando cuáles recibirán trigger en runtime.
  /// </summary>
  /// <param name="fromPosition">Posición mundial del nodo de origen.</param>
  /// <param name="toPosition">Posición mundial del nodo de destino.</param>
  /// <param name="corridorIndex">Índice del corredor, usado como semilla determinista.</param>
  private void DrawCorridorTriggerGizmos(
    Vector3 fromPosition,
    Vector3 toPosition,
    int     corridorIndex)
  {
    Vector3 delta         = toPosition - fromPosition;
    float   totalDistance = delta.magnitude;

    if (totalDistance < 0.001f || railMeshLength <= 0.001f)
      return;

    float   usableLength  = totalDistance - (nodeMargin * 2f);

    if (usableLength <= 0.001f)
      return;

    Vector3    direction      = delta.normalized;
    Quaternion railRotation   = ComputeRailRotation(direction);
    Vector3    corridorOrigin = fromPosition + direction * nodeMargin;

    int   pieceCount           = Mathf.Max(1, Mathf.RoundToInt(usableLength / railMeshLength));
    float adjustedPieceLength  = usableLength / pieceCount;

    for (int pieceIndex = 0; pieceIndex < pieceCount; pieceIndex++)
    {
      float   distanceAlongCorridor = (pieceIndex + 0.5f) * adjustedPieceLength;
      Vector3 piecePosition         = corridorOrigin + direction * distanceAlongCorridor;

      bool hasTrigger = ComputeDeterministicTrigger(
          piecePosition,
          corridorIndex * 1000 + pieceIndex
      );

      DrawTriggerGizmo(piecePosition, railRotation, hasTrigger);
    }
  }

  /// <summary>
  /// Dibuja un cubo gizmo en la posición indicada.
  /// Verde = recibirá trigger. Gris = no recibirá trigger.
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
  /// usando posición e índice como semilla para reproducibilidad entre
  /// editor y runtime.
  /// </summary>
  private bool ComputeDeterministicTrigger(Vector3 position, int index)
  {
    float pseudoRandom = Mathf.Abs(Mathf.Sin(
        position.x * 73.1f  +
        position.y * 157.3f +
        position.z * 241.7f +
        index      * 311.9f
    ));

    return pseudoRandom <= triggerSpawnProbability;
  }
#endif
}