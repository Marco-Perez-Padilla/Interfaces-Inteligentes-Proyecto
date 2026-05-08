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
/// Se reconstruye automáticamente cada vez que PathGenerator regenera
/// el grafo mediante el evento PathGenerator.OnGraphRegenerated.
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

  void OnEnable()
  {
    PathGenerator.OnGraphRegenerated += OnGraphRegenerated;
    RequestRebuild();
  }

  void OnDisable()
  {
    PathGenerator.OnGraphRegenerated -= OnGraphRegenerated;
  }

  void OnValidate() => RequestRebuild();
  void Start() => Rebuild();

  // =====================================================
  // GRAPH REGENERATED CALLBACK
  // =====================================================

  /// <summary>
  /// Callback invocado cuando el PathGenerator termina de regenerar el grafo.
  /// Solo reacciona si el evento viene del PathGenerator asignado a este builder.
  /// </summary>
  /// <param name="sender">PathGenerator que disparó el evento.</param>
  void OnGraphRegenerated(PathGenerator sender)
  {
    if (sender != pathGenerator)
      return;

    RequestRebuild();
  }

  // =====================================================
  // REBUILD
  // =====================================================

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
  /// de nodos consecutivos, agrupado bajo un GameObject hijo.
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
  /// <param name="corridorIndex">Índice del corredor dentro del camino.</param>
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

    float usableLength = totalDistance - (nodeMargin * 2f);

    if (usableLength <= 0.001f || railMeshLength <= 0.001f)
      return;

    Vector3 direction = delta.normalized;
    Quaternion railRotation = ComputeRailRotation(direction);
    Vector3 corridorOrigin = fromPosition + direction * nodeMargin;

    int pieceCount = Mathf.Max(1, Mathf.RoundToInt(usableLength / railMeshLength));
    float adjustedPieceLength = usableLength / pieceCount;

    GameObject corridorParent = new GameObject($"Corridor_{corridorIndex}");
    corridorParent.transform.SetParent(pathParent, worldPositionStays: false);

    for (int pieceIndex = 0; pieceIndex < pieceCount; pieceIndex++)
    {
      float distanceAlongCorridor = (pieceIndex + 0.5f) * adjustedPieceLength;
      Vector3 piecePosition = corridorOrigin + direction * distanceAlongCorridor;

      GameObject railPiece = Instantiate(
          railPrefab,
          piecePosition,
          railRotation,
          corridorParent.transform
      );

      ScaleRailPieceLength(railPiece, adjustedPieceLength);
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
  /// Dibuja líneas gizmo a lo largo de cada corredor del camino para
  /// visualizar la distribución de piezas en el editor.
  /// </summary>
  /// <param name="path">Camino a dibujar.</param>
  void DrawPathGizmos(List<PathNode> path)
  {
    if (path == null || path.Count < 2)
      return;

    Gizmos.color = new Color(0.8f, 0.6f, 0.1f, 0.6f);

    for (int nodeIndex = 0; nodeIndex < path.Count - 1; nodeIndex++)
    {
      Vector3 fromPosition = path[nodeIndex].position;
      Vector3 toPosition   = path[nodeIndex + 1].position;

      Gizmos.DrawLine(fromPosition, toPosition);
    }
  }
#endif
}