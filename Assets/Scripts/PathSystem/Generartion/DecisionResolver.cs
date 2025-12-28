using System.Collections.Generic;
using UnityEngine;

/**
 * @file DecisionResolver.cs
 * @brief Analiza el grafo final y detecta decisiones jugables reales.
 *
 * Reglas:
 * - Una decisión existe solo si hay >= 2 salidas
 * - Todas las salidas deben estar al mismo nivel Y
 * - Cruces a distinta altura NO generan decisión
 */
public static class DecisionResolver
{
  private const float HEIGHT_EPSILON = 0.01f;

  /**
   * @brief Procesa todos los nodos del grafo.
   */
  public static void Resolve(PathGraph graph)
  {
    if (graph == null)
      return;

    ProcessPath(graph.mainPath);

    foreach (var sub in graph.subPaths)
      ProcessPath(sub);
  }

  // --------------------------------------------------

  private static void ProcessPath(List<PathNode> path)
  {
    if (path == null)
      return;

    foreach (var node in path)
      EvaluateNode(node);
  }

  // --------------------------------------------------

  private static void EvaluateNode(PathNode node)
  {
    node.isDecisionNode = false;
    node.decisionExits.Clear();

    if (node.connections.Count < 2)
      return;

    float baseY = node.position.y;

    List<PathNode> sameLevelExits = new();

    foreach (var other in node.connections)
    {
      if (Mathf.Abs(other.position.y - baseY) < HEIGHT_EPSILON)
        sameLevelExits.Add(other);
    }

    if (sameLevelExits.Count >= 2)
    {
      node.isDecisionNode = true;
      node.decisionExits.AddRange(sameLevelExits);
    }
  }
}
