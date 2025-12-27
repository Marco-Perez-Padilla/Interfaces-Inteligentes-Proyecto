using UnityEngine;

/**
 * @file CartMovement.cs
 * @brief Controla el movimiento de la vagoneta a través del camino principal.
 *
 * Este componente solo debe inicializarse en Play Mode.
 * Si el PathGenerator aún no ha generado un camino válido,
 * el movimiento se desactiva y se muestra un warning.
 */
public class CartMovement : MonoBehaviour
{
    [Header("References")]
    public PathGenerator pathGenerator;
    public CameraShake cameraShake;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float rotationSpeed = 6f;

    private PathNode currentNode;
    private PathNode targetNode;
    private Vector3 lastDirection;

    void Start()
    {
        // IMPORTANTE: nunca inicializar en Editor
        if (!Application.isPlaying)
            return;

        if (pathGenerator == null ||
            pathGenerator.graph == null ||
            pathGenerator.graph.mainPath == null ||
            pathGenerator.graph.mainPath.Count < 2)
        {
            Debug.LogWarning(
                "CartMovement: PathGenerator no válido o camino principal insuficiente."
            );
            enabled = false;
            return;
        }

        // Arranque en el primer nodo del main path
        currentNode = pathGenerator.graph.mainPath[0];
        targetNode = pathGenerator.graph.mainPath[1];

        transform.position = currentNode.position;
        lastDirection = transform.forward;
    }

    void Update()
    {
        if (!Application.isPlaying)
            return;

        if (currentNode == null || targetNode == null)
            return;

        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 targetPos = targetNode.position;
        Vector3 direction = (targetPos - transform.position).normalized;

        // Rotación progresiva
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // Movimiento
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            AdvanceToNextNode();
        }

        lastDirection = direction;
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
}
