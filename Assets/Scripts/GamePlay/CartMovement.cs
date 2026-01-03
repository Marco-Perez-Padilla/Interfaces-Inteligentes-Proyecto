using UnityEngine;

/**
 * @file CartMovement.cs
 * @brief Maneja el movimiento de la vagoneta a lo largo del main path.
 *
 * Este script:
 * - Mueve la vagoneta
 * - NO toma decisiones
 * - NO toca CartDecisionController
 */
public class CartMovement : MonoBehaviour
{
    [Header("References")]
    public PathGenerator pathGenerator;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float rotationSpeed = 6f;

    private PathNode currentNode;
    private PathNode targetNode;
    public bool allowAdvance = true;

    void Start()
    {
        if (!Application.isPlaying)
            return;

        if (pathGenerator.graph.mainPath.Count < 2)
        {
            enabled = false;
            return;
        }

        currentNode = pathGenerator.graph.mainPath[0];
        targetNode = pathGenerator.graph.mainPath[1];

        transform.position = currentNode.position;
    }

    void Update()
    {
        if (!allowAdvance)
            return;

        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 dir = (targetNode.position - transform.position).normalized;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            rotationSpeed * Time.deltaTime
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetNode.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetNode.position) < 0.05f)
            AdvanceToNextNode();
    }

    private void AdvanceToNextNode()
    {
        int index = pathGenerator.graph.mainPath.IndexOf(targetNode);
        if (index >= 0 && index + 1 < pathGenerator.graph.mainPath.Count)
        {
            currentNode = targetNode;
            targetNode = pathGenerator.graph.mainPath[index + 1];
        }
    }
    public PathNode GetCurrentNode() => currentNode;
}
