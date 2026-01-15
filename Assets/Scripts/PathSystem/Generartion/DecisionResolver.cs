using System.Collections.Generic;

/**
 * @file DecisionResolver.cs
 * @brief Determina qué nodos son decisiones jugables reales.
 *
 * REGLAS:
 * - Debe tener >1 salida válida (excluyendo retorno)
 * - NO puede ser:
 *   - primordial
 *   - DP
 *   - bloqueado por cooldown
 */
public static class DecisionResolver
{
  public static void Resolve(PathGraph graph)
  {
    if (graph == null || graph.nodes == null)
      return;

    foreach (var node in graph.nodes.Values)
    {
      node.isDecisionNode = IsValidDecisionNode(node);
    }
  }

  // ======================================================
  // CORE
  // ======================================================

  private static bool IsValidDecisionNode(PathNode node)
  {
    // Restricciones duras
    if (node.isPrimordial)
      return false;

    if (node.isDP)
      return false;

    if (!node.canStartSubPath && !node.canReceiveSubPath)
      return false;

    // Geometría: ¿hay bifurcación real?
    int validExits = node.connections.Count;

    return validExits > 2;
    // >2 porque una es de entrada
  }
}
