using System.Collections.Generic;
using UnityEngine;

/**
 * @class PathGenerator
 * @brief Generador procedural de caminos sobre un grid ortogonal.
 *
 * Genera un camino aleatorio SIN CICLOS utilizando DFS con backtracking.
 * El camino:
 *  - Es visible en el editor (sin Play)
 *  - Empieza en la vagoneta
 *  - No tiene diagonales
 *  - No repite nodos
 *  - Evita callejones sin salida prematuros
 *
 * Diseñado para ser ampliado a:
 *  - bifurcaciones
 *  - dificultad progresiva
 *  - subidas y bajadas
 */
[ExecuteAlways]
public class PathGenerator : MonoBehaviour
{
    // ======================================================
    // CONFIGURACIÓN DEL GRID
    // ======================================================

    /** @brief Semilla para generación reproducible */
    [Header("Grid Settings")]
    public int seed = 0;

    /** @brief Número de puntos en el eje X */
    public int gridWidth = 10;

    /** @brief Número de puntos en el eje Z */
    public int gridHeight = 10;

    /** @brief Distancia entre puntos */
    public float spacing = 2f;

    // ======================================================
    // CONFIGURACIÓN DEL CAMINO
    // ======================================================

    /** @brief Longitud máxima del camino */
    [Header("Path Settings")]
    public int maxPathLength = 60;

    // ======================================================
    // DATOS GENERADOS
    // ======================================================

    /** @brief Puntos del grid (espacio de búsqueda) */
    [Header("Generated Data")]
    public List<Vector3> gridPoints = new List<Vector3>();

    /** @brief Camino final generado */
    public List<Vector3> pathPoints = new List<Vector3>();

    /** @brief Referencia a la vagoneta */
    private Transform cart;

    // ======================================================
    // CICLO DE VIDA (EDITOR + RUNTIME)
    // ======================================================

    void OnEnable()
    {
        Generate();
    }

    void OnValidate()
    {   
        ClampPathLength();
        Generate();
    }

    // ======================================================
    // GENERACIÓN PRINCIPAL
    // ======================================================

    /**
     * @brief Genera grid y camino completo.
     */
    void Generate()
    {
        if (!FindCart()) return;

        GenerateGrid();

        int attempts = 0;
        const int MAX_ATTEMPTS = 20;

        while (attempts < MAX_ATTEMPTS)
        {
            Random.InitState(seed + attempts);
            GeneratePathDFS();

            if (pathPoints.Count >= maxPathLength)
                break;

            attempts++;
        }
    }


    /**
     * @brief Busca la vagoneta por tag.
     */
    bool FindCart()
    {
        GameObject cartObj = GameObject.FindGameObjectWithTag("Vagoneta");
        if (cartObj == null) return false;

        cart = cartObj.transform;
        return true;
    }

    /**
     * @brief Genera un grid ortogonal de puntos.
     */
    void GenerateGrid()
    {
        gridPoints.Clear();
        Vector3 origin = transform.position;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 p = origin + new Vector3(x * spacing, 0f, z * spacing);
                gridPoints.Add(p);
            }
        }
    }

    // ======================================================
    // CAMINO DFS + BACKTRACKING (SIN DIAGONALES)
    // ======================================================

    /**
     * @brief Genera el camino usando DFS con backtracking.
     *
     * - Sin ciclos
     * - Sin diagonales
     * - Retrocede correctamente (sin saltos visuales)
     */
    void GeneratePathDFS()
    {
        pathPoints.Clear();
        if (gridPoints.Count == 0) return;

        Vector3 start = GetClosestPoint(cart.position);

        HashSet<Vector3> visited = new HashSet<Vector3>();
        Stack<Vector3> stack = new Stack<Vector3>();

        visited.Add(start);
        stack.Push(start);
        pathPoints.Add(start);

        while (stack.Count > 0 && pathPoints.Count < maxPathLength)
        {
            Vector3 current = stack.Peek();
            List<Vector3> neighbours = GetOrthogonalNeighbours(current, visited);

            if (neighbours.Count > 0)
            {
                Vector3 next = neighbours[Random.Range(0, neighbours.Count)];
                visited.Add(next);
                stack.Push(next);
                pathPoints.Add(next);
            }
            else
            {
                // Backtracking: retroceder también visualmente
                stack.Pop();
                if (pathPoints.Count > 0)
                    pathPoints.RemoveAt(pathPoints.Count - 1);
            }
        }
    }

    // ======================================================
    // UTILIDADES
    // ======================================================

    /**
     * @brief Devuelve el punto del grid más cercano a una posición.
     */
    Vector3 GetClosestPoint(Vector3 position)
    {
        Vector3 closest = gridPoints[0];
        float minDist = Vector3.Distance(position, closest);

        foreach (Vector3 p in gridPoints)
        {
            float d = Vector3.Distance(position, p);
            if (d < minDist)
            {
                minDist = d;
                closest = p;
            }
        }

        return closest;
    }

    /**
     * @brief Obtiene vecinos ORTOGONALES no visitados (sin diagonales).
     */
    List<Vector3> GetOrthogonalNeighbours(Vector3 point, HashSet<Vector3> visited)
    {
        List<Vector3> neighbours = new List<Vector3>();

        foreach (Vector3 p in gridPoints)
        {
            if (visited.Contains(p)) continue;

            Vector3 delta = p - point;

            bool horizontal =
                Mathf.Abs(delta.x - spacing) < 0.01f && Mathf.Abs(delta.z) < 0.01f ||
                Mathf.Abs(delta.x + spacing) < 0.01f && Mathf.Abs(delta.z) < 0.01f;

            bool vertical =
                Mathf.Abs(delta.z - spacing) < 0.01f && Mathf.Abs(delta.x) < 0.01f ||
                Mathf.Abs(delta.z + spacing) < 0.01f && Mathf.Abs(delta.x) < 0.01f;

            if (horizontal || vertical)
                neighbours.Add(p);
        }

        return neighbours;
    }

    // ======================================================
    // GIZMOS
    // ======================================================

    /**
     * @brief Dibuja el grid y el camino en la Scene View.
     */
    void OnDrawGizmos()
    {
        // Grid
        Gizmos.color = Color.gray;
        foreach (Vector3 p in gridPoints)
            Gizmos.DrawSphere(p, 0.12f);

        // Camino
        if (pathPoints == null || pathPoints.Count < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Gizmos.DrawSphere(pathPoints[i], 0.25f);
            Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
        }
    }

    /**
     * @brief Asegura que la longitud del camino es válida.
     */
    void ClampPathLength()
    {
        int maxPossible = gridWidth * gridHeight;

        if (maxPathLength > maxPossible)
            maxPathLength = maxPossible;

        if (maxPathLength < 1)
            maxPathLength = 1;
    }

}
