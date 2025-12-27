using System.Collections.Generic;
using UnityEngine;

/**
 * @file SubPathGenerator.cs
 * @brief Genera subrutas topológicas cerradas (FASE 1).
 *
 * Reglas Fase 1:
 * - Sale una sola vez del main path (Pi)
 * - Tiene un Pj elegido previamente
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
    List<PathNode> possiblePj = new();

    for (int i = piIndex + minLength; i < mainPath.Count - 1; i++)
    {
      if (!mainPath[i].isDP)
        possiblePj.Add(mainPath[i]);
    }

    if (possiblePj.Count == 0)
      return null;

    PathNode pj = possiblePj[Random.Range(0, possiblePj.Count)];

    // -----------------------------
    // DFS dirigido Pi -> Pj
    // -----------------------------
    HashSet<Vector3> visited = new();
    HashSet<Vector3> forbidden = new();

    foreach (var n in mainPath)
    {
      if (n != pi && n != pj)
        forbidden.Add(n.position);
    }

    List<Vector3> rawPath = GenerateDirectedDFS(
        pi.position,
        pj.position,
        visited,
        forbidden,
        minLength
    );

    if (rawPath == null || rawPath.Count < minLength)
      return null;

    // -----------------------------
    // Construcción PathNode
    // -----------------------------
    List<PathNode> sub = new();

    foreach (var p in rawPath)
    {
      sub.Add(new PathNode
      {
        position = p,
        pathType = PathType.Sub,
        pathId = subId
      });
    }

    for (int i = 0; i < sub.Count - 1; i++)
    {
      sub[i].connections.Add(sub[i + 1]);
      sub[i + 1].connections.Add(sub[i]);
    }

    pi.connections.Add(sub[0]);
    sub[0].connections.Add(pi);

    sub[^1].connections.Add(pj);
    pj.connections.Add(sub[^1]);

    return sub;
  }

  // ======================================================
  // DFS DIRIGIDO
  // ======================================================

  private List<Vector3> GenerateDirectedDFS(
      Vector3 start,
      Vector3 goal,
      HashSet<Vector3> visited,
      HashSet<Vector3> forbidden,
      int minLength)
  {
    Stack<Vector3> stack = new();
    Dictionary<Vector3, Vector3> parent = new();

    stack.Push(start);
    visited.Add(start);

    while (stack.Count > 0)
    {
      Vector3 current = stack.Pop();

      if (current == goal && visited.Count >= minLength)
        return ReconstructPath(parent, start, goal);

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

  private List<Vector3> ReconstructPath(
      Dictionary<Vector3, Vector3> parent,
      Vector3 start,
      Vector3 end)
  {
    List<Vector3> path = new();
    Vector3 current = end;
    path.Add(current);

    while (current != start)
    {
      if (!parent.ContainsKey(current))
        return null;

      current = parent[current];
      path.Add(current);
    }

    path.Reverse();
    return path;
  }

  private void Shuffle(List<Vector3> list)
  {
    for (int i = 0; i < list.Count; i++)
    {
      int j = Random.Range(i, list.Count);
      (list[i], list[j]) = (list[j], list[i]);
    }
  }
}
