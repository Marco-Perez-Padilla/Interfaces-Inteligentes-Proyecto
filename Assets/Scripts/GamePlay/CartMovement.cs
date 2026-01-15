using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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

    [Header("Input Actions")]
    public InputActionReference impulseAction;
    public InputActionReference brakeAction;
    public InputActionReference moveAction;

    [Header("VR Controls")]
    public Transform rightHand;
    public Transform leftHand;
    public bool useVRHandControls = false;
    public float handPushThreshold = 0.2f;
    public float handPullThreshold = 0.15f;
    public float handCooldown = 0.3f;

    private float speedImpulse;
    private bool brakeLocked;

    private PathNode previousNode;
    private PathNode currentNode;
    private PathNode targetNode;

    public bool isWaitingDecision;

    // VR tracking variables
    private Vector3 lastRightHandPos;
    private Vector3 lastLeftHandPos;
    private float lastHandActionTime;

    void Start()
    {
        var main = pathGenerator.graph.mainPath;
        currentNode = main[0];
        targetNode = main[1];
        transform.position = currentNode.position;

        // Enable input actions
        if (impulseAction != null) impulseAction.action.Enable();
        if (brakeAction != null) brakeAction.action.Enable();
        if (moveAction != null) moveAction.action.Enable();

        // Initialize VR hand positions
        if (rightHand != null) lastRightHandPos = rightHand.position;
        if (leftHand != null) lastLeftHandPos = leftHand.position;

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
    // INPUT - Sistema unificado (Teclado + VR)
    // ======================================================

    private void HandleSpeedInput()
    {
        float impulseInput = 0f;
        float brakeInput = 0f;

        // 1. Input Actions (botones VR/teclado)
        if (impulseAction != null)
        {
            impulseInput = impulseAction.action.ReadValue<float>();
        }

        if (brakeAction != null)
        {
            brakeInput = brakeAction.action.ReadValue<float>();
        }

        // 2. Controles VR por movimiento de manos (si está habilitado)
        if (useVRHandControls && Time.time - lastHandActionTime > handCooldown)
        {
            float handImpulse = GetHandImpulseInput();
            float handBrake = GetHandBrakeInput();

            impulseInput = Mathf.Max(impulseInput, handImpulse);
            brakeInput = Mathf.Max(brakeInput, handBrake);
        }

        // 3. Controles de teclado tradicionales (backward compatibility)
        if (Input.GetKey(KeyCode.V))
        {
            impulseInput = 1f;
        }

        if (Input.GetKey(KeyCode.X))
        {
            brakeInput = 1f;
        }

        // Aplicar impulso y freno
        speedImpulse += impulseInput * impulseStrength * Time.deltaTime;
        speedImpulse -= brakeInput * brakeStrength * Time.deltaTime;

        // 4. Sistema de dirección con Input Action (para decisiones)
        if (moveAction != null && isWaitingDecision)
        {
            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
            HandleDirectionInput(moveInput);
        }

        // Actualizar posiciones de manos para VR
        if (rightHand != null) lastRightHandPos = rightHand.position;
        if (leftHand != null) lastLeftHandPos = leftHand.position;
    }

    private float GetHandImpulseInput()
    {
        if (rightHand == null || leftHand == null) return 0f;

        // Detectar empuje hacia adelante con ambas manos
        Vector3 rightHandDelta = transform.InverseTransformDirection(rightHand.position - lastRightHandPos);
        Vector3 leftHandDelta = transform.InverseTransformDirection(leftHand.position - lastLeftHandPos);

        // Empuje con la mano derecha (adelante) o ambas manos
        bool rightHandPush = rightHandDelta.z > handPushThreshold;
        bool leftHandPush = leftHandDelta.z > handPushThreshold;

        if (rightHandPush || leftHandPush)
        {
            lastHandActionTime = Time.time;
            return 1f;
        }

        return 0f;
    }

    private float GetHandBrakeInput()
    {
        if (rightHand == null || leftHand == null) return 0f;

        // Detectar tirón hacia atrás con ambas manos
        Vector3 rightHandDelta = transform.InverseTransformDirection(rightHand.position - lastRightHandPos);
        Vector3 leftHandDelta = transform.InverseTransformDirection(leftHand.position - lastLeftHandPos);

        // Tirón hacia atrás con las manos
        bool rightHandPull = rightHandDelta.z < -handPullThreshold;
        bool leftHandPull = leftHandDelta.z < -handPullThreshold;

        // También considerar movimiento lateral rápido como freno de mano
        bool rightHandSwipe = Mathf.Abs(rightHandDelta.x) > handPullThreshold * 1.5f;
        bool leftHandSwipe = Mathf.Abs(leftHandDelta.x) > handPullThreshold * 1.5f;

        if ((rightHandPull && leftHandPull) || rightHandSwipe || leftHandSwipe)
        {
            lastHandActionTime = Time.time;
            return 1f;
        }

        return 0f;
    }

    private void HandleDirectionInput(Vector2 input)
    {
        if (input.magnitude < 0.1f) return;

        // Solo procesar cuando estamos en un nodo de decisión
        if (!isWaitingDecision || currentNode == null) return;

        var exits = GetValidExits();
        if (exits.Count <= 1) return;

        // Determinar dirección basada en input
        PathNode selectedNode = null;
        
        if (exits.Count == 2)
        {
            // Para 2 salidas: izquierda/derecha
            Vector3 exit1Dir = (exits[0].position - currentNode.position).normalized;
            Vector3 exit2Dir = (exits[1].position - currentNode.position).normalized;
            
            float angle1 = Vector3.SignedAngle(transform.forward, exit1Dir, Vector3.up);
            float angle2 = Vector3.SignedAngle(transform.forward, exit2Dir, Vector3.up);
            
            if (input.x < -0.5f) // Izquierda
            {
                selectedNode = Mathf.Abs(angle1) > Mathf.Abs(angle2) ? exits[0] : exits[1];
            }
            else if (input.x > 0.5f) // Derecha
            {
                selectedNode = Mathf.Abs(angle1) < Mathf.Abs(angle2) ? exits[0] : exits[1];
            }
            else if (input.y > 0.5f) // Adelante
            {
                selectedNode = Mathf.Abs(angle1) < Mathf.Abs(angle2) ? exits[0] : exits[1];
            }
        }

        if (selectedNode != null)
        {
            targetNode = selectedNode;
            isWaitingDecision = false;
        }
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

    private List<PathNode> GetValidExits()
    {
        var list = new List<PathNode>();

        foreach (var n in currentNode.connections)
            if (n != previousNode)
                list.Add(n);

        return list;
    }

    public PathNode Current => currentNode;
    public PathNode Previous => previousNode;

    void OnDisable()
    {
        // Disable input actions when disabled
        if (impulseAction != null && impulseAction.action.enabled)
            impulseAction.action.Disable();
        if (brakeAction != null && brakeAction.action.enabled)
            brakeAction.action.Disable();
        if (moveAction != null && moveAction.action.enabled)
            moveAction.action.Disable();
    }

    public void Choose(PathNode next)
    {
        targetNode = next;
        isWaitingDecision = false;
    }
}