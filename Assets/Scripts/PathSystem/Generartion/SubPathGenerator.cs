using System.Collections.Generic;
using UnityEngine;

/**
 * @file SubPathGenerator.cs
 * @brief Genera subrutas topológicas 2D cerradas (Fase 1).
 *
 * Reglas:
 * - Sale una sola vez del main path (Pi)
 * - Tiene un Pj posterior en el main
 * - No pisa el main salvo Pi y Pj
 * - No se ramifica
 * - Si no llega a Pj, se descarta
 */
public class SubPathGenerator
{
  private readonly Grid2D grid;

  public SubPathGenerator(Grid2D grid)
  {
    this.grid = grid;
  }

  /**
   * @brief Intenta generar una subruta desde Pi.
   */
  public List<PathNode> Generate(
      PathNode pi,
      List<PathNode> mainPath,
      int subId,
      int minLength)
  {
    int piIndex = mainPath.IndexOf(pi);
    if (piIndex < 0)
      return null;

    // -----------------------------
    // Selección de Pj
    // -----------------------------
    List<PathNode> candidates = new();
    for (int i = piIndex + minLength; i < mainPath.Count; i++)
      candidates.Add(mainPath[i]);

    Shuffle(candidates);

    foreach (var pj in candidates)
    {
      var sub = TryGenerate(pi, pj, mainPath, subId, minLength);
      if (sub != null)
        return sub;
    }

    return null;
  }

  // ======================================================
  // DFS dirigido Pi -> Pj
  // ======================================================

  private List<PathNode> TryGenerate(
      PathNode pi,
      PathNode pj,
      List<PathNode> mainPath,
      int subId,
      int minLength)
  {
    HashSet<Vector3> forbidden = new();
    foreach (var n in mainPath)
    {
      if (n != pi && n != pj)
        forbidden.Add(n.position);
    }

    List<Vector3> raw = GenerateDFS(
        pi.position,
        pj.position,
        forbidden,
        minLength
    );

    if (raw == null)
      return null;

    List<PathNode> sub = new();
    foreach (var p in raw)
    {
      sub.Add(new PathNode
      {
        position = p,
        pathType = PathType.Sub,
        pathId = subId
      });
    }

    // Conexiones internas
    for (int i = 0; i < sub.Count - 1; i++)
    {
      sub[i].connections.Add(sub[i + 1]);
      sub[i + 1].connections.Add(sub[i]);
    }

    // Conexiones con main
    pi.connections.Add(sub[0]);
    sub[0].connections.Add(pi);

    sub[^1].connections.Add(pj);
    pj.connections.Add(sub[^1]);

    return sub;
  }

  private List<Vector3> GenerateDFS(
      Vector3 start,
      Vector3 goal,
      HashSet<Vector3> forbidden,
      int minLength)
  {
    Stack<Vector3> stack = new();
    Dictionary<Vector3, Vector3> parent = new();
    HashSet<Vector3> visited = new();

    stack.Push(start);
    visited.Add(start);

    while (stack.Count > 0)
    {
      var current = stack.Pop();

      if (current == goal && visited.Count >= minLength)
        return Reconstruct(parent, start, goal);

      var neighbours = grid.GetNeighbours(current);
      Shuffle(neighbours);

      foreach (var n in neighbours)
      {
        if (visited.Contains(n)) continue;
        if (forbidden.Contains(n)) continue;

        visited.Add(n);
        parent[n] = current;
        stack.Push(n);
      }
    }

    return null;
  }

  private List<Vector3> Reconstruct(
      Dictionary<Vector3, Vector3> parent,
      Vector3 start,
      Vector3 end)
  {
    List<Vector3> path = new();
    Vector3 current = end;
    path.Add(current);

    while (current != start)
    {
      if (!parent.TryGetValue(current, out current))
        return null;
      path.Add(current);
    }

    path.Reverse();
    return path;
  }

  private void Shuffle<T>(List<T> list)
  {
    for (int i = 0; i < list.Count; i++)
    {
      int j = Random.Range(i, list.Count);
      (list[i], list[j]) = (list[j], list[i]);
    }
  }
}
