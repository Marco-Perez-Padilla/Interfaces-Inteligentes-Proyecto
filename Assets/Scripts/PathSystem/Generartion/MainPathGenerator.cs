using System.Collections.Generic;
using UnityEngine;

/**
 * @file MainPathGenerator.cs
 * @brief Genera el camino principal sin ciclos.
 */
public class MainPathGenerator
{
    private readonly Grid2D grid;
    private readonly Transform cart;
    private readonly PathGraph graph;

    public MainPathGenerator(Grid2D grid, Transform cart, PathGraph graph)
    {
        this.grid = grid;
        this.cart = cart;
        this.graph = graph;
    }

    public List<PathNode> Generate(int seed, int maxLength)
    {
        Vector3 start = GetClosestPoint(cart.position);
        Random.InitState(seed);

        List<Vector3> raw = GenerateDFS(start, maxLength);

        List<PathNode> path = new();

        for (int i = 0; i < raw.Count; i++)
        {
            PathNode node = graph.GetOrCreateNode(raw[i], PathType.Main);
            path.Add(node);

            if (i > 0)
            {
                Connect(path[i - 1], node);
            }
        }

        return path;
    }

    // ======================================================
    // DFS 2D
    // ======================================================

    private List<Vector3> GenerateDFS(Vector3 start, int maxLength)
    {
        List<Vector3> result = new();
        HashSet<Vector3> visited = new();
        Stack<Vector3> stack = new();

        visited.Add(start);
        stack.Push(start);
        result.Add(start);

        while (stack.Count > 0 && result.Count < maxLength)
        {
            Vector3 current = stack.Peek();
            var neighbours = GetUnvisitedNeighbours(current, visited);

            if (neighbours.Count > 0)
            {
                Vector3 next = neighbours[Random.Range(0, neighbours.Count)];
                visited.Add(next);
                stack.Push(next);
                result.Add(next);
            }
            else
            {
                stack.Pop();
                result.RemoveAt(result.Count - 1);
            }
        }

        return result;
    }

    private List<Vector3> GetUnvisitedNeighbours(Vector3 p, HashSet<Vector3> visited)
    {
        List<Vector3> result = new();
        foreach (var n in grid.GetNeighbours(p))
            if (!visited.Contains(n))
                result.Add(n);
        return result;
    }

    private Vector3 GetClosestPoint(Vector3 pos)
    {
        Vector3 closest = grid.points[0];
        float min = Vector3.Distance(pos, closest);

        foreach (var p in grid.points)
        {
            float d = Vector3.Distance(pos, p);
            if (d < min)
            {
                min = d;
                closest = p;
            }
        }
        return closest;
    }

    private void Connect(PathNode a, PathNode b)
    {
        if (!a.connections.Contains(b))
        {
            a.connections.Add(b);
            b.connections.Add(a);
        }
    }
}
