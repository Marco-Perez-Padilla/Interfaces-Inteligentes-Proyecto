using System.Collections.Generic;
using UnityEngine;

/**
 * @file SubPathGenerator.cs
 * @brief Genera subrutas coherentes y SIEMPRE conectadas.
 *
 * Reglas garantizadas:
 * - Sale EXACTAMENTE de Pi
 * - Reentra EXACTAMENTE en Pj
 * - Nunca genera rutas flotantes
 */
public class SubPathGenerator
{
    private readonly Grid2D grid;
    private readonly PathGraph graph;

    public SubPathGenerator(Grid2D grid, PathGraph graph)
    {
        this.grid = grid;
        this.graph = graph;
    }

    public List<PathNode> Generate(
        PathNode pi,
        PathNode pj,
        int minLength)
    {
        List<Vector3> raw = GenerateDFS(
            pi.position,
            pj.position,
            minLength
        );

        if (raw == null || raw.Count < 2)
            return null;


        if (raw[0] != pi.position)
            return null;

        if (raw[^1] != pj.position)
            return null;

        List<PathNode> sub = new();

        foreach (var p in raw)
        {
            PathNode node = graph.GetOrCreateNode(p, PathType.Sub);
            sub.Add(node);
        }

        // Conectar secuencia interna
        for (int i = 0; i < sub.Count - 1; i++)
        {
            Connect(sub[i], sub[i + 1]);
        }

        return sub;
    }

    // ======================================================
    // DFS LOCAL 2D (VALIDADO)
    // ======================================================

    private List<Vector3> GenerateDFS(
        Vector3 start,
        Vector3 goal,
        int minLength)
    {
        Stack<Vector3> stack = new();
        Dictionary<Vector3, Vector3> parent = new();
        HashSet<Vector3> visited = new();

        stack.Push(start);
        visited.Add(start);

        while (stack.Count > 0)
        {
            Vector3 current = stack.Pop();

            if (current == goal)
            {
                List<Vector3> path =
                    Reconstruct(parent, start, goal);

                if (path != null && path.Count >= minLength)
                    return path;
            }

            var neighbours = grid.GetNeighbours(current);
            Shuffle(neighbours);

            foreach (var n in neighbours)
            {
                if (visited.Contains(n))
                    continue;

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

    private void Connect(PathNode a, PathNode b)
    {
        if (!a.connections.Contains(b))
        {
            a.connections.Add(b);
            b.connections.Add(a);
        }
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
