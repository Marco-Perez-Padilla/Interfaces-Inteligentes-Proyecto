using System.Collections.Generic;
using UnityEngine;

/**
 * @file HeightModulator.cs
 * @brief Aplica altura a caminos sin alterar la topología.
 *
 * Reglas clave:
 * - La altura NO crea bifurcaciones
 * - La altura pertenece al tramo, no al nodo
 * - Las subrutas heredan la altura del Pi
 * - La fusión fuerza coincidencia exacta en Y
 */
public static class HeightModulator
{
  // ======================================================
  // MAIN PATH
  // ======================================================

  /**
   * @brief Aplica altura al camino principal.
   *
   * @param path Camino principal
   * @param spacing Tamaño del grid
   * @param maxHeight Altura máxima absoluta
   * @param climbChance Probabilidad de cambiar estado vertical
   * @param flatStartLength Nodos iniciales planos
   */
  public static void ApplyToMain(
      List<PathNode> path,
      float spacing,
      float maxHeight,
      float climbChance,
      int flatStartLength)
  {
    if (path == null || path.Count < 2)
      return;

    int maxLevel = Mathf.RoundToInt(maxHeight / spacing);

    int currentLevel = 0;
    int verticalState = 0;
    // -1 = bajando, 0 = plano, 1 = subiendo

    for (int i = 0; i < path.Count; i++)
    {
      // Inicio plano garantizado
      if (i < flatStartLength)
      {
        SetHeight(path[i], 0f);
        continue;
      }

      // Cambio de estado SOLO si estamos planos
      if (verticalState == 0 && Random.value < climbChance)
        verticalState = Random.value > 0.5f ? 1 : -1;

      // Límites
      if (verticalState == 1 && currentLevel >= maxLevel)
        verticalState = 0;

      if (verticalState == -1 && currentLevel <= 0)
        verticalState = 0;

      currentLevel = Mathf.Clamp(
          currentLevel + verticalState,
          0,
          maxLevel
      );

      SetHeight(path[i], currentLevel * spacing);

      // Salida natural a plano
      if (verticalState != 0 && Random.value < 0.25f)
        verticalState = 0;
    }
  }

  // ======================================================
  // SUB PATH
  // ======================================================

  /**
   * @brief Aplica altura a una subruta.
   *
   * Reglas:
   * - Hereda la altura del Pi
   * - Tramo inicial plano
   * - Fusión fuerza altura exacta en Pj
   */
  public static void ApplyToSubPath(
      List<PathNode> subPath,
      int flatExitLength,
      float spacing,
      float maxHeight,
      float climbChance)
  {
    if (subPath == null || subPath.Count < 2)
      return;

    int maxLevel = Mathf.RoundToInt(maxHeight / spacing);

    // Heredar altura del Pi
    float startY = subPath[0].position.y;
    int currentLevel = Mathf.RoundToInt(startY / spacing);

    int verticalState = 0;

    for (int i = 0; i < subPath.Count; i++)
    {
      // Tramo plano inicial
      if (i < flatExitLength)
      {
        SetHeight(subPath[i], currentLevel * spacing);
        continue;
      }

      if (verticalState == 0 && Random.value < climbChance)
        verticalState = Random.value > 0.5f ? 1 : -1;

      if (verticalState == 1 && currentLevel >= maxLevel)
        verticalState = 0;

      if (verticalState == -1 && currentLevel <= 0)
        verticalState = 0;

      currentLevel = Mathf.Clamp(
          currentLevel + verticalState,
          0,
          maxLevel
      );

      SetHeight(subPath[i], currentLevel * spacing);

      if (Random.value < 0.3f)
        verticalState = 0;
    }

    // Forzar fusión exacta (último nodo)
    PathNode last = subPath[^1];
    foreach (var c in last.connections)
    {
      if (c.pathType == PathType.Main)
      {
        SetHeight(last, c.position.y);
        break;
      }
    }
  }

  // ======================================================
  // UTIL
  // ======================================================

  private static void SetHeight(PathNode node, float y)
  {
    Vector3 p = node.position;
    p.y = y;
    node.position = p;
  }
}
