using System.Collections.Generic;
using UnityEngine;

/**
 * @class PathGenerator
 * @brief Genera un camino procedural 3D sobre un grid ortogonal cúbico.
 *
 * Características:
 * - Grid uniforme en X, Y y Z (usa spacing en todos los ejes)
 * - Visualización 3D del grid mediante Gizmos
 * - Camino sin ciclos (DFS)
 * - Pendientes diagonales realistas (sin escalones)
 * - No permite subir y bajar de forma inmediata
 *
 * El camino se genera en dos fases:
 *  1) Topología X/Z (DFS)
 *  2) Modulación vertical por niveles del grid
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

    /** @brief Número de celdas en X */
    public int gridWidth = 10;

    /** @brief Número de celdas en Z */
    public int gridHeight = 10;

    /** @brief Tamaño de cada celda del grid (X, Y y Z) */
    public float spacing = 2f;

    // ======================================================
    // CONFIGURACIÓN DEL CAMINO
    // ======================================================

    /** @brief Longitud máxima del camino */
    [Header("Path Settings")]
    public int maxPathLength = 60;

    // ======================================================
    // CONFIGURACIÓN DE ALTURA
    // ======================================================

    /** @brief Altura máxima del camino */
    [Header("Height Settings")]
    public float maxHeight = 6f;

    /** @brief Probabilidad de iniciar una pendiente */
    [Range(0f, 1f)]
    public float climbChance = 0.25f;

    // ======================================================
    // VISUALIZACIÓN DEL GRID 3D
    // ======================================================

    /** @brief Número de niveles visibles del grid en Y */
    [Header("Grid 3D Visualization")]
    public int heightLevels = 4;

    // ======================================================
    // DATOS GENERADOS
    // ======================================================

    /** @brief Todos los puntos del grid base (X/Z) */
    public List<Vector3> gridPoints = new List<Vector3>();

    /** @brief Camino final generado (3D) */
    public List<Vector3> pathPoints = new List<Vector3>();

    /** @brief Referencia a la vagoneta */
    private Transform cart;

    // ======================================================
    // CICLO DE VIDA
    // ======================================================

    /**
     * @brief Se ejecuta al activar el componente.
     */
    void OnEnable()
    {
        Generate();
    }

    /**
     * @brief Se ejecuta al modificar valores en el Inspector.
     */
    void OnValidate()
    {
        ClampPathLength();
        Generate();
    }

    // ======================================================
    // GENERACIÓN PRINCIPAL
    // ======================================================

    /**
     * @brief Genera el grid, el camino y la altura.
     */
    void Generate()
    {
        if (!FindCart()) return;

        GenerateGrid();
        GeneratePathWithRetries();
        ApplyVerticalVariation();
    }

    /**
     * @brief Busca la vagoneta usando su tag.
     */
    bool FindCart()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Vagoneta");
        if (obj == null) return false;

        cart = obj.transform;
        return true;
    }

    /**
     * @brief Genera el grid ortogonal base en X/Z.
     */
    void GenerateGrid()
    {
        gridPoints.Clear();
        Vector3 origin = transform.position;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                gridPoints.Add(origin + new Vector3(x * spacing, 0f, z * spacing));
            }
        }
    }

    /**
     * @brief Intenta generar un camino válido varias veces.
     */
    void GeneratePathWithRetries()
    {
        int attempts = 0;
        const int MAX_ATTEMPTS = 20;

        while (attempts < MAX_ATTEMPTS)
        {
            Random.InitState(seed + attempts);
            GeneratePathDFS();

            if (pathPoints.Count >= maxPathLength)
                return;

            attempts++;
        }
    }

    /**
     * @brief Genera un camino sin ciclos usando DFS.
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
                stack.Pop();
                pathPoints.RemoveAt(pathPoints.Count - 1);
            }
        }
    }

    /**
     * @brief Aplica altura usando niveles discretos del grid.
     *
     * La altura se calcula como:
     *   nivel * spacing
     * garantizando un grid cúbico coherente.
     */
    void ApplyVerticalVariation()
    {
        if (pathPoints.Count < 2) return;

        int currentLevel = 0;
        int maxLevel = Mathf.RoundToInt(maxHeight / spacing);

        int state = 0;
        //  1 = subir
        //  0 = plano
        // -1 = bajar

        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            if (state == 0 && Random.value < climbChance)
                state = Random.value > 0.5f ? 1 : -1;

            if (state == 1 && currentLevel >= maxLevel)
                state = 0;

            if (state == -1 && currentLevel <= 0)
                state = 0;

            currentLevel += state;
            currentLevel = Mathf.Clamp(currentLevel, 0, maxLevel);

            Vector3 next = pathPoints[i + 1];
            next.y = currentLevel * spacing;
            pathPoints[i + 1] = next;

            if (state != 0 && Random.value < 0.25f)
                state = 0;
        }
    }

    // ======================================================
    // UTILIDADES
    // ======================================================

    /**
     * @brief Devuelve el punto del grid más cercano a una posición.
     */
    Vector3 GetClosestPoint(Vector3 pos)
    {
        Vector3 closest = gridPoints[0];
        float minDist = Vector3.Distance(pos, closest);

        foreach (Vector3 p in gridPoints)
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

    /**
     * @brief Obtiene vecinos ortogonales no visitados.
     */
    List<Vector3> GetOrthogonalNeighbours(Vector3 point, HashSet<Vector3> visited)
    {
        List<Vector3> neighbours = new List<Vector3>();

        foreach (Vector3 p in gridPoints)
        {
            if (visited.Contains(p)) continue;

            Vector3 d = p - point;

            bool moveX = Mathf.Abs(Mathf.Abs(d.x) - spacing) < 0.01f && Mathf.Abs(d.z) < 0.01f;
            bool moveZ = Mathf.Abs(Mathf.Abs(d.z) - spacing) < 0.01f && Mathf.Abs(d.x) < 0.01f;

            if (moveX || moveZ)
                neighbours.Add(p);
        }

        return neighbours;
    }

    /**
     * @brief Limita la longitud máxima del camino.
     */
    void ClampPathLength()
    {
        maxPathLength = Mathf.Clamp(maxPathLength, 1, gridWidth * gridHeight);
    }

    // ======================================================
    // GIZMOS
    // ======================================================

    /**
     * @brief Dibuja el grid 3D y el camino en la Scene View.
     */
    void OnDrawGizmos()
    {
        // Grid 3D
        Gizmos.color = new Color(0.6f, 0.6f, 0.6f, 0.4f);
        for (int h = 0; h <= heightLevels; h++)
        {
            float y = h * spacing;
            foreach (Vector3 p in gridPoints)
            {
                Gizmos.DrawSphere(new Vector3(p.x, y, p.z), 0.08f);
            }
        }

        // Camino
        if (pathPoints.Count < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Gizmos.DrawSphere(pathPoints[i], 0.25f);
            Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
        }
    }
}
