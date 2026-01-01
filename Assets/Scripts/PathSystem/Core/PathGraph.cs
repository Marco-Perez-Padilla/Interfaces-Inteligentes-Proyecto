using System.Collections.Generic;
using UnityEngine;

/**
 * @file PathGraph.cs
 * @brief Contenedor global del grafo navegable.
 *
 * Mantiene un registro único de nodos por posición.
 */
public class PathGraph
{
  /** Registro global de nodos (clave = posición exacta) */
  public Dictionary<Vector3, PathNode> nodes = new();

  /** Camino principal */
  public List<PathNode> mainPath = new();

  /** Subrutas generadas */
  public List<List<PathNode>> subPaths = new();

  /**
   * @brief Devuelve un nodo existente o crea uno nuevo.
   */
  public PathNode GetOrCreateNode(Vector3 pos, PathType type)
  {
    if (nodes.TryGetValue(pos, out var node))
      return node;

    node = new PathNode
    {
      position = pos,
      pathType = type
    };

    nodes[pos] = node;
    return node;
  }
}
