using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class PathSurfaceBuilder : MonoBehaviour
{
  // =====================================================
  // REFERENCES
  // =====================================================

  [Header("References")]
  [SerializeField] PathGenerator pathGenerator;

  [Header("Prefabs")]
  [SerializeField] GameObject nodePlanePrefab;
  [SerializeField] GameObject connectionPlanePrefab;

  // =====================================================
  // SIZES (LOGICAL)
  // =====================================================

  [Header("Sizes")]
  [Tooltip("Tamaño lógico del nodo (en unidades del mundo)")]
  [SerializeField] float nodeSize = 0.4f;

  [Tooltip("Ancho lógico de la conexión")]
  [SerializeField] float connectionWidth = 0.2f;

  // =====================================================
  // INTERNAL
  // =====================================================

  float nodeMeshSize = 1f;
  float connectionMeshLength = 1f;

#if UNITY_EDITOR
    bool pendingRebuild;
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
      foreach (var sub in pathGenerator.graph.subPaths)
        BuildPath(sub);
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
    GameObject go = Instantiate(
        nodePlanePrefab,
        position,
        Quaternion.identity,
        transform
    );

    float scale = nodeSize / nodeMeshSize;

    go.transform.localScale = new Vector3(
        scale,
        1f,
        scale
    );
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

    // Longitud lógica: resta el tamaño del nodo
    float logicalLength = Mathf.Max(0f, realDistance - nodeSize);
    if (logicalLength <= 0.001f)
      return;

    // Centro REAL entre nodos
    Vector3 center = from + direction * (realDistance * 0.5f);

    Quaternion rotation = Quaternion.LookRotation(direction);

    GameObject go = Instantiate(
        connectionPlanePrefab,
        center,
        rotation,
        transform
    );

    // Escala correcta (independiente del prefab)
    float scaleZ = logicalLength / connectionMeshLength;
    float scaleX = connectionWidth / nodeMeshSize;

    go.transform.localScale = new Vector3(
        scaleX,
        1f,
        scaleZ
    );
  }


  // =====================================================
  // MESH SIZE CACHE
  // =====================================================

  void CacheMeshSizes()
  {
    if (nodePlanePrefab != null)
    {
      var mf = nodePlanePrefab.GetComponent<MeshFilter>();
      if (mf != null && mf.sharedMesh != null)
        nodeMeshSize = mf.sharedMesh.bounds.size.x;
    }

    if (connectionPlanePrefab != null)
    {
      var mf = connectionPlanePrefab.GetComponent<MeshFilter>();
      if (mf != null && mf.sharedMesh != null)
        connectionMeshLength = mf.sharedMesh.bounds.size.z;
    }
  }
}
