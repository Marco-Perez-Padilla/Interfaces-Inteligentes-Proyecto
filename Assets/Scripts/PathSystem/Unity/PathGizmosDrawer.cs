using UnityEngine;
using System.Collections.Generic;

/**
 * @file PathGizmosDrawer.cs
 * @brief Dibuja el sistema de caminos mediante Gizmos para depuración.
 *
 * Características:
 * - Camino principal claramente diferenciado
 * - Cada subruta tiene un color único y estable
 * - Modos de visualización (Main / Sub / All)
 * - No altera la lógica del juego
 */
public class PathGizmosDrawer : MonoBehaviour
{
  [Header("References")]
  public PathGenerator generator;

  [Header("Debug")]
  public PathDebugMode debugMode = PathDebugMode.All;

  [Header("Visual Settings")]
  public float mainNodeRadius = 0.3f;
  public float subNodeRadius = 0.2f;

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
    if (generator.graph.mainPath == null)
      return;

    Gizmos.color = Color.yellow;

    DrawPath(generator.graph.mainPath, mainNodeRadius);
  }

  // ======================================================
  // SUB PATHS
  // ======================================================

  private void DrawSubPaths()
  {
    List<List<PathNode>> subs = generator.graph.subPaths;
    if (subs == null)
      return;

    for (int i = 0; i < subs.Count; i++)
    {
      List<PathNode> sub = subs[i];
      if (sub == null || sub.Count == 0)
        continue;

      Gizmos.color = GetColorForSubPath(sub[0]);
      DrawPath(sub, subNodeRadius);
    }
  }

  // ======================================================
  // DRAW UTIL
  // ======================================================

  private void DrawPath(List<PathNode> path, float radius)
  {
    if (path.Count == 0)
      return;

    for (int i = 0; i < path.Count; i++)
    {
      PathNode node = path[i];
      Gizmos.DrawSphere(node.position, radius);

      if (i < path.Count - 1)
      {
        Gizmos.DrawLine(node.position, path[i + 1].position);
      }
    }
  }

  // ======================================================
  // COLOR GENERATION
  // ======================================================

  /**
   * @brief Genera un color único y estable para una subruta.
   *
   * Usa el pathId y el número áureo para evitar colisiones de color.
   */
  private Color GetColorForSubPath(PathNode node)
  {
    // Golden ratio para buena distribución
    float hue = Mathf.Abs(node.pathId * 0.6180339887f) % 1f;

    // Saturación y brillo fijos para legibilidad
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
