using UnityEngine;
using System.Collections.Generic;

/**
 * @file SlopeLimiter.cs
 * @brief Limita la pendiente máxima entre nodos conectados.
 *
 * Garantiza que ningún tramo tenga pendiente > 45°.
 *
 * Regla:
 * |ΔY| <= spacing
 */
public static class SlopeLimiter
{
  /**
   * @brief Aplica el límite de pendiente a todo el grafo.
   *
   * @param graph Grafo de caminos
   * @param spacing Tamaño del grid (ΔXZ)
   * @param iterations Pasadas de relajación (2–4 recomendado)
   */
  public static void Apply(PathGraph graph, float spacing, int iterations = 3)
  {
    if (graph == null || graph.nodes == null)
      return;

    for (int it = 0; it < iterations; it++)
    {
      foreach (var node in graph.nodes.Values)
      {
        foreach (var other in node.connections)
        {
          LimitPair(node, other, spacing);
        }
      }
    }
  }

  // ======================================================
  // CORE
  // ======================================================

  private static void LimitPair(PathNode a, PathNode b, float spacing)
  {
    float dy = b.position.y - a.position.y;

    if (Mathf.Abs(dy) <= spacing)
      return;

    float clampedDy = Mathf.Sign(dy) * spacing;

    // Ajustamos ambos nodos hacia el punto medio
    float mid = (a.position.y + b.position.y) * 0.5f;

    SetY(a, mid - clampedDy * 0.5f);
    SetY(b, mid + clampedDy * 0.5f);
  }

  private static void SetY(PathNode n, float y)
  {
    Vector3 p = n.position;
    p.y = y;
    n.position = p;
  }
}