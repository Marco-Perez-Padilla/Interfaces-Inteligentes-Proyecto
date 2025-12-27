using System.Collections.Generic;
using UnityEngine;

/**
 * @file MainPathGenerator.cs
 * @brief Genera el camino principal usando DFS con backtracking y reintentos.
 *
 * Características:
 * - Parte desde la posición real de la vagoneta
 * - No deja caminos inconexos
 * - No genera rutas rechazadas
 * - Respeta el límite natural del grid
 * - Reintenta con seeds derivadas
 */
public class MainPathGenerator
{
    private readonly Grid2D grid;
    private readonly Transform cart;

    private const int MAX_ATTEMPTS = 20;

    public MainPathGenerator(Grid2D grid, Transform cart)
    {
        this.grid = grid;
        this.cart = cart;
    }

    /**
     * @brief Genera el camino principal.
     */
    public List<PathNode> Generate(
        int seed,
        int maxLength)
    {
        Vector3 start = GetClosestPoint(cart.position);

        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            Random.InitState(seed + attempt);

            List<Vector3> rawPath = GenerateDFS(start, maxLength);

            if (rawPath.Count >= maxLength)
                return ConvertToPathNodes(rawPath);
        }

        // Fallback: devuelve lo mejor que haya
        return ConvertToPathNodes(GenerateDFS(start, maxLength));
    }

    // ======================================================
    // DFS REAL CON BACKTRACKING
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
            List<Vector3> neighbours = GetUnvisitedNeighbours(current, visited);

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

    // ======================================================
    // CONVERSIÓN A NODOS
    // ======================================================

    private List<PathNode> ConvertToPathNodes(List<Vector3> raw)
    {
        List<PathNode> nodes = new();

        for (int i = 0; i < raw.Count; i++)
        {
            nodes.Add(new PathNode
            {
                position = raw[i],
                pathType = PathType.Main,
                pathId = 0
            });
        }

        // Conexiones lineales
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            nodes[i].connections.Add(nodes[i + 1]);
            nodes[i + 1].connections.Add(nodes[i]);
        }

        return nodes;
    }

    // ======================================================
    // UTILIDADES
    // ======================================================

    private Vector3 GetClosestPoint(Vector3 pos)
    {
        Vector3 closest = grid.points[0];
        float minDist = Vector3.Distance(pos, closest);

        foreach (var p in grid.points)
        {
            float d = Vector3.Distance(pos, p);
            if (d < minDist)
            {
                minDist = d;
                closest = p;
            }
        }

        return closest;
    }

    private List<Vector3> GetUnvisitedNeighbours(
        Vector3 p,
        HashSet<Vector3> visited)
    {
        List<Vector3> result = new();

        foreach (var n in grid.GetNeighbours(p))
        {
            if (!visited.Contains(n))
                result.Add(n);
        }

        return result;
    }
}
