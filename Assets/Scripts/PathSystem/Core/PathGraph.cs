using System.Collections.Generic;

/**
 * @file PathGraph.cs
 * @brief Contenedor del grafo completo de caminos.
 */
public class PathGraph
{
  /** @brief Camino principal. */
  public List<PathNode> mainPath = new();

  /** @brief Subrutas independientes. */
  public List<List<PathNode>> subPaths = new();
}
