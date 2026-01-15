using UnityEngine;
using System.Collections.Generic;

/**
 * @file PathGizmosDrawer.cs
 * @brief Dibuja los trayectos del sistema de caminos mediante Gizmos.
 *
 * IMPORTANTE:
 * - Dibuja RECORRIDOS, no geometría topológica.
 * - Los trayectos pueden compartir nodos y tramos.
 * - Se aplica un offset visual para distinguir subrutas.
 */
public class PathGizmosDrawer : MonoBehaviour
{
  // ======================================================
  // REFERENCES
  // ======================================================

  [Header("References")]
  public PathGenerator generator;

  // ======================================================
  // DEBUG
  // ======================================================

  [Header("Debug")]
  public PathDebugMode debugMode = PathDebugMode.All;

  // ======================================================
  // VISUAL SETTINGS
  // ======================================================

  [Header("Visual Settings")]
  public float mainNodeRadius = 0.3f;
  public float subNodeRadius = 0.2f;

  [Header("Trajectory Offset")]
  [Tooltip("Separación visual lateral entre trayectos de subrutas")]
  public float subPathOffset = 0.15f;

  // ======================================================
  // UNITY
  // ======================================================

  void OnDrawGizmos()
  {
    if (generator == null || generator.graph == null)
      return;

    switch (debugMode)
    {
      case PathDebugMode.MainOnly:
        DrawMainPath();
        break;

      case PathDebugMode.SubOnly:
        DrawSubPaths();
        break;

      case PathDebugMode.All:
        DrawMainPath();
        DrawSubPaths();
        break;
    }
  }

  // ======================================================
  // MAIN PATH
  // ======================================================

  private void DrawMainPath()
  {
    var main = generator.graph.mainPath;
    if (main == null || main.Count == 0)
      return;

    Gizmos.color = Color.yellow;

    DrawTrajectory(
      main,
      mainNodeRadius,
      0f // sin offset
    );
  }

  // ======================================================
  // SUB PATHS
  // ======================================================

  private void DrawSubPaths()
  {
    var subs = generator.graph.subPaths;
    if (subs == null)
      return;

    for (int i = 0; i < subs.Count; i++)
    {
      var sub = subs[i];
      if (sub == null || sub.Count == 0)
        continue;

      Gizmos.color = GetColorForSubPath(i);

      // Offset estable por subruta
      float offset = subPathOffset * (i + 1);

      DrawTrajectory(
        sub,
        subNodeRadius,
        offset
      );
    }
  }

  // ======================================================
  // TRAJECTORY DRAW
  // ======================================================

  /**
   * @brief Dibuja un trayecto completo con offset lateral.
   *
   * @param path Trayecto a dibujar
   * @param radius Radio del nodo
   * @param offsetAmount Separación lateral visual
   */
  private void DrawTrajectory(
    List<PathNode> path,
    float radius,
    float offsetAmount)
  {
    if (path == null || path.Count == 0)
      return;

    for (int i = 0; i < path.Count; i++)
    {
      PathNode node = path[i];
      Vector3 pos = node.position;

      // Nodo
      Gizmos.DrawSphere(pos, radius);

      if (i < path.Count - 1)
      {
        PathNode next = path[i + 1];

        Vector3 offset = Vector3.zero;
        if (offsetAmount != 0f)
          offset = GetLateralOffset(node, next, offsetAmount);

        Gizmos.DrawLine(
          node.position + offset,
          next.position + offset
        );
      }
    }
  }

  // ======================================================
  // OFFSET UTIL
  // ======================================================

  /**
   * @brief Calcula un offset lateral estable entre dos nodos.
   */
  private Vector3 GetLateralOffset(PathNode a, PathNode b, float amount)
  {
    Vector3 dir = (b.position - a.position).normalized;
    Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
    return right * amount;
  }

  // ======================================================
  // COLOR GENERATION
  // ======================================================

  /**
   * @brief Genera un color único y estable para una subruta.
   *
   * El color depende del índice de la subruta, no de los nodos.
   */
  private Color GetColorForSubPath(int subIndex)
  {
    // Golden ratio para buena distribución cromática
    float hue = Mathf.Abs(subIndex * 0.6180339887f) % 1f;
    return Color.HSVToRGB(hue, 0.85f, 0.9f);
  }
}

/**
 * @brief Modos de visualización del PathGizmosDrawer.
 */
public enum PathDebugMode
{
  All,
  MainOnly,
  SubOnly
}
