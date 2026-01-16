using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class PathPieceApplier : MonoBehaviour
{
  // ======================================================
  // REFERENCES
  // ======================================================

  [Header("References")]
  [SerializeField] PathGenerator pathGenerator;
  [SerializeField] PathPieceRegistry registry;

  // ======================================================
  // VISUAL MODE
  // ======================================================

  [Header("Visualization")]
  [SerializeField] PathVisualMode visualMode = PathVisualMode.All;

  // ======================================================
  // BLOCK SETTINGS
  // ======================================================

  [Header("Block Size")]
  [SerializeField] float blockScale = 1f;
  [SerializeField] float blockLength = 9f;

  [Header("Vertical Offsets")]
  [SerializeField] float blockSurfaceOffset = 0.5f;
  [SerializeField] float corridorSurfaceOffset = 0.5f;

  HashSet<Vector3> appliedNodes = new();

#if UNITY_EDITOR
    bool pendingRebuild;
#endif

  // ======================================================
  // UNITY
  // ======================================================

  void Start() => Rebuild();

#if UNITY_EDITOR
    void OnEnable() => RequestRebuild();
    void OnValidate() => RequestRebuild();

    void RequestRebuild()
    {
        if (pendingRebuild) return;
        pendingRebuild = true;
        EditorApplication.delayCall += DelayedRebuild;
    }

    void DelayedRebuild()
    {
        if (this == null) return;
        pendingRebuild = false;
        Rebuild();
    }
#endif

  // ======================================================
  // CORE
  // ======================================================

  void Rebuild()
  {
    if (pathGenerator == null || registry == null) return;
    if (pathGenerator.graph == null) return;

    ClearChildren();
    appliedNodes.Clear();

    switch (visualMode)
    {
      case PathVisualMode.MainOnly:
        ApplyPath(pathGenerator.graph.mainPath, true);
        break;

      case PathVisualMode.SubOnly:
        ApplySubPaths();
        break;

      case PathVisualMode.All:
        ApplyPath(pathGenerator.graph.mainPath, true);
        ApplySubPaths();
        break;
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

  // ======================================================
  // APPLY PATHS
  // ======================================================

  void ApplySubPaths()
  {
    var subs = pathGenerator.graph.subPaths;
    if (subs == null) return;

    foreach (var path in subs)
      ApplyPath(path, false);
  }

  void ApplyPath(List<PathNode> path, bool allowForks)
  {
    if (path == null || path.Count < 2) return;

    for (int i = 1; i < path.Count - 1; i++)
    {
      PathNode prev = path[i - 1];
      PathNode curr = path[i];
      PathNode next = path[i + 1];

      PathPieceType blockType =
          allowForks && curr.connections.Count > 2
              ? ResolveFork(curr, prev)
              : PathDirectionUtils.GetTurnPiece(
                  prev.position,
                  curr.position,
                  next.position
              );

      SpawnBlock(prev, curr, blockType);
      SpawnCorridor(curr, next);
    }
  }

  // ======================================================
  // BLOCK
  // ======================================================

  void SpawnBlock(PathNode prev, PathNode curr, PathPieceType type)
  {
    if (appliedNodes.Contains(curr.position))
      return;

    appliedNodes.Add(curr.position);

    Vector3 forward = curr.position - prev.position;
    forward.y = 0;

    GameObject prefab = registry.Get(type);
    if (prefab == null) return;

    Vector3 spawnPos = curr.position;
    spawnPos.y -= blockSurfaceOffset;

    GameObject go = Instantiate(
        prefab,
        spawnPos,
        Quaternion.LookRotation(forward),
        transform
    );

    go.transform.localScale = Vector3.one * blockScale;
  }

  // ======================================================
  // CORRIDOR
  // ======================================================

  void SpawnCorridor(PathNode from, PathNode to)
  {
    Vector3 delta = to.position - from.position;

    // SOLO distancia horizontal
    Vector3 horizontal = new Vector3(delta.x, 0, delta.z);
    float horizontalDistance = horizontal.magnitude;
    if (horizontalDistance < 0.01f) return;

    horizontal.Normalize();

    float corridorLength = Mathf.Max(0, horizontalDistance - blockLength);
    if (corridorLength <= 0.01f) return;

    // ALTURA MEDIA ENTRE NODOS
    float midY = (from.position.y + to.position.y) * 0.5f;

    Vector3 center =
        from.position +
        horizontal * (blockLength * 0.5f + corridorLength * 0.5f);

    center.y = midY - corridorSurfaceOffset;

    PathPieceType corridorType = ResolveCorridorType(from, to);
    GameObject prefab = registry.Get(corridorType);
    if (prefab == null) return;

    GameObject go = Instantiate(
        prefab,
        center,
        Quaternion.LookRotation(horizontal),
        transform
    );

    Vector3 scale = go.transform.localScale;
    scale.z = corridorLength;
    go.transform.localScale = scale;
  }



  // ======================================================
  // RESOLVERS
  // ======================================================

  PathPieceType ResolveCorridorType(PathNode from, PathNode to)
  {
    float dy = to.position.y - from.position.y;
    if (dy > 0.1f) return PathPieceType.CorridorUp;
    if (dy < -0.1f) return PathPieceType.CorridorDown;
    return PathPieceType.CorridorStraight;
  }

  PathPieceType ResolveFork(PathNode node, PathNode prev)
  {
    int left = 0, right = 0, straight = 0;
    Vector3 baseDir = node.position - prev.position;

    foreach (var n in node.connections)
    {
      if (n == prev) continue;

      float angle = PathDirectionUtils.SignedAngleXZ(
          baseDir,
          n.position - node.position
      );

      if (Mathf.Abs(angle) < 5f) straight++;
      else if (angle > 0) right++;
      else left++;
    }

    if (left == 1 && right == 1 && straight == 1)
      return PathPieceType.ForkTriple;
    if (left == 1 && right == 1)
      return PathPieceType.ForkLeftRight;
    if (left == 1 && straight == 1)
      return PathPieceType.ForkLeftStraight;

    return PathPieceType.ForkRightStraight;
  }
}

public enum PathVisualMode
{
  All,
  MainOnly,
  SubOnly
}
