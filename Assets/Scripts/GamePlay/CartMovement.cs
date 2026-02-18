using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/**
 * Movimiento de la vagoneta con:
 * - Toma de decisiones (nodos con múltiples salidas)
 * - Impulso por pulsación (no continuo)
 * - Freno continuo
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

    [Header("Impulse (por pulsación)")]
    public float impulsePerPress = 5f;      // Cantidad fija que se añade por cada pulsación
    public float impulseDecay = 3f;         // Decaimiento natural (por segundo)

    [Header("Brake (continuo)")]
    public float brakeStrength = 6f;        // Fuerza de frenado por segundo
    public float hardBrakeThreshold = -1.5f; // Bloqueo total
    private bool wasBraking = false;

    [Header("Input Actions")]
    public InputActionReference impulseAction;  // Para detectar pulsaciones
    public InputActionReference brakeAction;    // Para freno continuo
    public InputActionReference moveAction;     // Para dirección

    [Header("VR Controls (manos)")]
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

    // Eventos para feedback VR
    public System.Action onImpulse; 
    public System.Action onBrake;  

    void OnEnable()
    {
        // Habilitar acciones y suscribirse a eventos
        if (impulseAction != null)
        {
            impulseAction.action.Enable();
            impulseAction.action.performed += OnImpulsePerformed;
        }
        if (brakeAction != null) brakeAction.action.Enable();
        if (moveAction != null) moveAction.action.Enable();
    }

    void OnDisable()
    {
        // Desuscribirse y deshabilitar
        if (impulseAction != null)
        {
            impulseAction.action.performed -= OnImpulsePerformed;
            impulseAction.action.Disable();
        }
        if (brakeAction != null && brakeAction.action.enabled)
            brakeAction.action.Disable();
        if (moveAction != null && moveAction.action.enabled)
            moveAction.action.Disable();
    }

    void Start()
    {
        var main = pathGenerator.graph.mainPath;
        currentNode = main[0];
        targetNode = main[1];
        transform.position = currentNode.position;

        // Inicializar posiciones de manos para VR
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

        // Log para ver el valor del joystick (solo debug)
        if (moveAction != null)
        {
            Vector2 moveVal = moveAction.action.ReadValue<Vector2>();
            if (moveVal.magnitude > 0.1f)
            {
                Debug.Log($"Move Action RAW Value: {moveVal}");
            }
        }

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
        float brakeInput = 0f;

        // 1. Freno continuo desde Input Action
        if (brakeAction != null)
        {
            brakeInput = brakeAction.action.ReadValue<float>();
            bool isBraking = brakeInput > 0.1f;
            if (isBraking && !wasBraking)
            {
                if (onBrake != null) onBrake.Invoke();
            }
            wasBraking = isBraking;
        }

        // 2. Controles VR por movimiento de manos (si está habilitado)
        if (useVRHandControls && Time.time - lastHandActionTime > handCooldown)
        {
            float handImpulse = GetHandImpulseInput(); // 0 o 1
            float handBrake = GetHandBrakeInput();

            // Si hay impulso de manos, añadimos un pulso (como si fuera un botón)
            if (handImpulse > 0.1f)
            {
                speedImpulse += impulsePerPress;
                Debug.Log($"Impulse por manos: +{impulsePerPress}");
            }

            brakeInput = Mathf.Max(brakeInput, handBrake);
        }

        // 3. Controles de teclado tradicionales
        if (Input.GetKeyDown(KeyCode.V))
        {
            speedImpulse += impulsePerPress;
            Debug.Log($"Impulse por tecla V: +{impulsePerPress}");
        }
        if (Input.GetKey(KeyCode.X))
        {
            brakeInput = 1f;
        }

        // Aplicar freno (continuo)
        speedImpulse -= brakeInput * brakeStrength * Time.deltaTime;

        // 4. Sistema de dirección con Input Action (para decisiones)
        // if (moveAction != null && isWaitingDecision)
        // {
        //     Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        //     HandleDirectionInput(moveInput);
        // }

        // Actualizar posiciones de manos para VR
        if (rightHand != null) lastRightHandPos = rightHand.position;
        if (leftHand != null) lastLeftHandPos = leftHand.position;
    }

    // Evento llamado cuando se pulsa el botón de impulso (VR o teclado)
    private void OnImpulsePerformed(InputAction.CallbackContext context)
    {
        speedImpulse += impulsePerPress;
        Debug.Log($"Impulse pulsado! speedImpulse = {speedImpulse}");
        if (onImpulse != null) onImpulse.Invoke();
    }

    private float GetHandImpulseInput()
    {
        if (rightHand == null || leftHand == null) return 0f;

        Vector3 rightHandDelta = transform.InverseTransformDirection(rightHand.position - lastRightHandPos);
        Vector3 leftHandDelta = transform.InverseTransformDirection(leftHand.position - lastLeftHandPos);

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

        Vector3 rightHandDelta = transform.InverseTransformDirection(rightHand.position - lastRightHandPos);
        Vector3 leftHandDelta = transform.InverseTransformDirection(leftHand.position - lastLeftHandPos);

        bool rightHandPull = rightHandDelta.z < -handPullThreshold;
        bool leftHandPull = leftHandDelta.z < -handPullThreshold;
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

        if (!isWaitingDecision || currentNode == null) return;

        var exits = GetValidExits();
        if (exits.Count <= 1) return;

        PathNode selectedNode = null;

        if (exits.Count == 2)
        {
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
            Debug.Log("--- MODO DECISIÓN ACTIVADO ---");
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

        Debug.Log($"Nodo en posición {currentNode.position} tiene {list.Count} salidas válidas.");

        return list;
    }

    public PathNode Current => currentNode;
    public PathNode Previous => previousNode;

    public void Choose(PathNode next)
    {
        targetNode = next;
        isWaitingDecision = false;
    }
}