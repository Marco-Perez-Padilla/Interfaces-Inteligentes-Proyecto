using System.Collections.Generic;
using UnityEngine;

public static class DecisionResolver
{
  private const float HEIGHT_EPSILON = 0.01f;
  private const float DIR_DOT = 0.9f;

  public static void Resolve(PathGraph graph)
  {
    Process(graph.mainPath);

    foreach (var sub in graph.subPaths)
      Process(sub);
  }

  private static void Process(List<PathNode> path)
  {
    if (path == null)
      return;

    foreach (var n in path)
      Evaluate(n);
  }

  private static void Evaluate(PathNode node)
  {
    node.isDecisionNode = false;
    node.decisionExits.Clear();

    if (node.connections.Count < 2)
      return;

    float y = node.position.y;
    List<PathNode> sameLevel = new();

    foreach (var c in node.connections)
    {
      if (Mathf.Abs(c.position.y - y) < HEIGHT_EPSILON)
        sameLevel.Add(c);
    }

    List<Vector3> dirs = new();

    foreach (var c in sameLevel)
    {
      Vector3 d = (c.position - node.position).normalized;
      d.y = 0f;

      bool unique = true;
      foreach (var u in dirs)
      {
        if (Vector3.Dot(u, d) > DIR_DOT)
        {
          unique = false;
          break;
        }
      }

      if (unique)
      {
        dirs.Add(d);
        node.decisionExits.Add(c);
      }
    }

    if (node.decisionExits.Count >= 2)
      node.isDecisionNode = true;
  }
}
