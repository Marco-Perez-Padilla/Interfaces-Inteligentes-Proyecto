using UnityEngine;

/**
 * Movimiento de la vagoneta con:
 * - Toma de decisiones (nodos con múltiples salidas)
 * - Impulso (V)
 * - Freno bloqueante (X)
 * - Sonido y cámara dependientes de velocidad
 */
public class CartMovement : MonoBehaviour
{
    [Header("References")]
    public PathGenerator pathGenerator;
    public CameraShake cameraShake;
    public AudioSource engineAudio;

    [Header("Movement")]
    public float speed = 2f;
    public float rotationSpeed = 8f;

    [Header("Impulse")]
    public float impulseStrength = 2.5f;
    public float brakeStrength = 6f;
    public float impulseDecay = 3f;

    [Header("Brake")]
    public float hardBrakeThreshold = -1.5f; // bloqueo total

    private float speedImpulse;
    private bool brakeLocked;

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

        if (engineAudio != null)
            engineAudio.loop = true;
    }

    void Update()
    {
        HandleSpeedInput();
        DecayImpulse();
        UpdateBrakeState();

        if (!isWaitingDecision && targetNode != null && !brakeLocked)
            Move();

        UpdateCameraShake();
        UpdateSound();
    }

    // ======================================================
    // INPUT
    // ======================================================

    private void HandleSpeedInput()
    {
        if (Input.GetKey(KeyCode.V))
            speedImpulse += impulseStrength * Time.deltaTime;

        if (Input.GetKey(KeyCode.X))
            speedImpulse -= brakeStrength * Time.deltaTime;
    }

    // ======================================================
    // MOVEMENT
    // ======================================================

    private void Move()
    {
        float currentSpeed = Mathf.Max(0f, speed + speedImpulse);

        if (currentSpeed <= 0.01f)
            return;

        Vector3 dir = targetNode.position - transform.position;
        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);

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
            currentSpeed * Time.deltaTime
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

        targetNode = exits.Count == 1 ? exits[0] : null;
    }

    // ======================================================
    // IMPULSE / BRAKE
    // ======================================================

    private void DecayImpulse()
    {
        speedImpulse = Mathf.MoveTowards(
            speedImpulse,
            0f,
            impulseDecay * Time.deltaTime
        );
    }

    private void UpdateBrakeState()
    {
        brakeLocked = speedImpulse <= hardBrakeThreshold;

        if (brakeLocked)
            speedImpulse = Mathf.Min(speedImpulse, hardBrakeThreshold);
    }

    // ======================================================
    // FEEDBACK
    // ======================================================

    private void UpdateCameraShake()
    {
        if (cameraShake == null)
            return;

        float factor = Mathf.Clamp01((speed + speedImpulse) / speed);
        cameraShake.dynamicFactor = factor;
    }

    private void UpdateSound()
    {
        if (engineAudio == null)
            return;

        float speedFactor = Mathf.Clamp01((speed + speedImpulse) / speed);

        engineAudio.pitch = Mathf.Lerp(0.6f, 1.4f, speedFactor);
        engineAudio.volume = Mathf.Lerp(0.3f, 1f, speedFactor);

        if (!engineAudio.isPlaying && speedFactor > 0.05f)
            engineAudio.Play();

        if (engineAudio.isPlaying && speedFactor <= 0.05f)
            engineAudio.Stop();
    }

    // ======================================================
    // UTIL
    // ======================================================

    private System.Collections.Generic.List<PathNode> GetValidExits()
    {
        var list = new System.Collections.Generic.List<PathNode>();

        foreach (var n in currentNode.connections)
            if (n != previousNode)
                list.Add(n);

        return list;
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
}
