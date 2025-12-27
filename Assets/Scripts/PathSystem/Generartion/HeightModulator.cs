using System.Collections.Generic;
using UnityEngine;

/**
 * @file HeightModulator.cs
 * @brief Aplica variación vertical sin romper la topología del grafo.
 *
 * Reglas:
 * - El main path tiene un tramo inicial plano
 * - Las subrutas salen planas desde Pi
 * - Nunca se sube/baja en el nodo de salida
 * - El nodo de fusión SIEMPRE hereda la altura del main
 */
public static class HeightModulator
{
  // ======================================================
  // MAIN PATH
  // ======================================================

  public static void ApplyToMain(
      List<PathNode> main,
      float stepHeight,
      float maxHeight,
      float climbChance,
      int flatStartLength)
  {
    if (main == null || main.Count < 2)
      return;

    int maxLevel = Mathf.RoundToInt(maxHeight / stepHeight);
    int currentLevel = 0;
    int direction = 0;

    for (int i = 0; i < main.Count; i++)
    {
      if (i < flatStartLength)
      {
        SetHeight(main[i], 0f);
        continue;
      }

      if (direction == 0 && Random.value < climbChance)
        direction = Random.value > 0.5f ? 1 : -1;

      if (direction == 1 && currentLevel >= maxLevel)
        direction = 0;

      if (direction == -1 && currentLevel <= 0)
        direction = 0;

      currentLevel = Mathf.Clamp(currentLevel + direction, 0, maxLevel);
      SetHeight(main[i], currentLevel * stepHeight);

      if (Random.value < 0.25f)
        direction = 0;
    }
  }

  // ======================================================
  // SUB PATH
  // ======================================================

  public static void ApplyToSubPath(
      List<PathNode> sub,
      int flatExitLength,
      float stepHeight,
      float maxHeight,
      float climbChance)
  {
    if (sub == null || sub.Count == 0)
      return;

    // Altura base = nodo Pi (conectado al main)
    PathNode pi = sub[0].connections.Find(n => n.pathType == PathType.Main);
    float baseHeight = pi != null ? pi.position.y : 0f;

    int maxLevel = Mathf.RoundToInt(maxHeight / stepHeight);
    int currentLevel = Mathf.RoundToInt(baseHeight / stepHeight);
    int direction = 0;

    for (int i = 0; i < sub.Count; i++)
    {
      PathNode node = sub[i];

      // Tramo de salida plano
      if (i < flatExitLength)
      {
        SetHeight(node, baseHeight);
        continue;
      }

      // Último nodo: interpolar hacia merge
      if (i == sub.Count - 1 &&
          node.connections.Exists(c => c.isMergeNode))
      {
        PathNode merge = node.connections.Find(c => c.isMergeNode);
        float y = Mathf.Lerp(
            currentLevel * stepHeight,
            merge.position.y,
            0.85f
        );

        SetHeight(node, y);
        continue;
      }

      if (direction == 0 && Random.value < climbChance)
        direction = Random.value > 0.5f ? 1 : -1;

      if (direction == 1 && currentLevel >= maxLevel)
        direction = 0;

      if (direction == -1 && currentLevel <= 0)
        direction = 0;

      currentLevel = Mathf.Clamp(currentLevel + direction, 0, maxLevel);
      SetHeight(node, currentLevel * stepHeight);

      if (Random.value < 0.25f)
        direction = 0;
    }
  }

  // ======================================================
  // UTILIDAD
  // ======================================================

  private static void SetHeight(PathNode node, float y)
  {
    Vector3 p = node.position;
    p.y = y;
    node.position = p;
  }
}
