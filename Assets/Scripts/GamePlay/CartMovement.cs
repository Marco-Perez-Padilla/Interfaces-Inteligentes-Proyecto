using UnityEngine;

/**
 * @file CartMovement.cs
 * @brief Movimiento de la vagoneta sobre el grafo.
 *
 * - Avanza automáticamente
 * - SOLO se detiene en bifurcaciones reales
 * - Mantiene nodo anterior para lógica de decisiones
 */
public class CartMovement : MonoBehaviour
{
    [Header("References")]
    public PathGenerator pathGenerator;

    [Header("Movement")]
    public float speed = 2f;
    public float rotationSpeed = 8f;

    private PathNode previousNode;
    private PathNode currentNode;
    private PathNode targetNode;

    public bool isWaitingDecision;

    void Start()
    {
        var main = pathGenerator.graph.mainPath;
        currentNode = main[0];
        targetNode = main[1];
        transform.position = currentNode.position;
    }

    void Update()
    {
        if (isWaitingDecision || targetNode == null)
            return;

        Move();
    }

    private void Move()
    {
        Vector3 dir = (targetNode.position - transform.position);
        Vector3 flatDir = new Vector3(dir.x, 0, dir.z);

        if (flatDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(flatDir),
                rotationSpeed * Time.deltaTime
            );
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetNode.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetNode.position) < 0.05f)
            Arrive();
    }

    private void Arrive()
    {
        previousNode = currentNode;
        currentNode = targetNode;

        var exits = GetValidExits();

        if (exits.Count > 1)
        {
            isWaitingDecision = true;
            targetNode = null;
            return;
        }

        // Continuar automáticamente
        targetNode = exits.Count == 1 ? exits[0] : null;
    }

    // ======================================================
    // API
    // ======================================================

    public void Choose(PathNode next)
    {
        targetNode = next;
        isWaitingDecision = false;
    }

    public PathNode Current => currentNode;
    public PathNode Previous => previousNode;

    // ======================================================
    // UTIL
    // ======================================================

    private System.Collections.Generic.List<PathNode> GetValidExits()
    {
        var list = new System.Collections.Generic.List<PathNode>();

        foreach (var n in currentNode.connections)
        {
            if (n != previousNode)
                list.Add(n);
        }

        return list;
    }
}
