using UnityEngine;
<<<<<<< HEAD

/// <summary>
/// Gestiona la entrada y salida del jugador en la vagoneta.
/// Al montar, coloca la cámara sobre el SeatPoint con un offset en Y
/// para que quede por encima del borde de la vagoneta y se vea desde dentro.
/// </summary>
public class CartMount : MonoBehaviour
{
    // =====================================================
    // REFERENCES
    // =====================================================

    [Header("References")]
    public Transform seatPoint;
    public CartMovement cartMovement;

    // =====================================================
    // CAMERA MOUNT SETTINGS
    // =====================================================

    [Header("Camera Mount Settings")]
    [Tooltip("Desplazamiento vertical de la cámara respecto al SeatPoint al montar. " +
             "Aumentar si la cámara queda demasiado baja dentro de la vagoneta.")]
    public float cameraHeightOffset = 0.4f;

    // =====================================================
    // INTERNAL
    // =====================================================

    private Transform player;
    private Transform cam;
    private CameraShake shake;
    private bool mounted;

    // =====================================================
    // UNITY
    // =====================================================

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main.transform;
        shake = cam.GetComponent<CameraShake>();

        if (shake != null)
            shake.enableShake = false;

        cartMovement.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!mounted) Mount();
            else Unmount();
        }
    }

    // =====================================================
    // MOUNT / UNMOUNT
    // =====================================================

    /// <summary>
    /// Coloca la cámara dentro de la vagoneta sobre el SeatPoint,
    /// aplicando un offset en Y para que quede por encima del borde.
    /// Activa el shake y el movimiento del carrito.
    /// </summary>
    void Mount()
    {
        cam.SetParent(seatPoint);
        cam.localPosition = new Vector3(0f, cameraHeightOffset, 0f);
        cam.localRotation = Quaternion.identity;

        if (shake != null)
        {
            // ResetOriginalPosition debe llamarse DESPUÉS de fijar localPosition
            // para que el shake parta de la posición correcta con el offset.
            shake.ResetOriginalPosition();
            shake.enableShake = true;
        }

        cartMovement.enabled = true;
        mounted = true;
    }

    /// <summary>
    /// Devuelve la cámara al jugador, desactiva el shake y detiene
    /// el movimiento del carrito.
    /// </summary>
    void Unmount()
    {
        cam.SetParent(player);
        cam.localPosition = new Vector3(0f, 1.6f, 0f);
        cam.localRotation = Quaternion.identity;

        if (shake != null)
        {
            shake.ResetOriginalPosition();
            shake.enableShake = false;
        }

        cartMovement.enabled = false;
        mounted = false;
=======
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class CartMount : MonoBehaviour
{
    [Header("References")]
    public Transform seatPoint;
    public CartMovement cartMovement;
    public XRFollowCart xrFollower;
    public GameObject locomotionSystem; // Opcional

    [Header("Input Action")]
    public InputActionReference mountAction; // Arrastrar la acción aquí

    private bool isMounted = false;

    void OnEnable()
    {
        if (mountAction != null)
        {
            mountAction.action.Enable();
            mountAction.action.performed += OnMountPerformed;
        }
    }

    void OnDisable()
    {
        if (mountAction != null)
        {
            mountAction.action.performed -= OnMountPerformed;
            mountAction.action.Disable();
        }
    }

    void Start()
    {
        if (xrFollower != null) xrFollower.enabled = false;
        if (cartMovement != null) cartMovement.enabled = false;
        SetLocomotionActive(true);
    }

    private void OnMountPerformed(InputAction.CallbackContext context)
    {
        if (!isMounted)
            Mount();
        else
            Unmount();
    }

    void Mount()
    {
        if (xrFollower != null)
        {
            xrFollower.enabled = true;
            xrFollower.TeleportToCart();
        }

        if (cartMovement != null)
            cartMovement.enabled = true;

        SetLocomotionActive(false);
        isMounted = true;
    }

    void Unmount()
    {
        if (xrFollower != null)
            xrFollower.enabled = false;

        if (cartMovement != null)
            cartMovement.enabled = false;

        SetLocomotionActive(true);
        isMounted = false;
    }

    void SetLocomotionActive(bool active)
    {
        Debug.Log($"SetLocomotionActive: {active}");

        var continuousMoves = FindObjectsOfType<ContinuousMoveProviderBase>(true);
        foreach (var provider in continuousMoves)
        {
            Debug.Log($"Provider {provider.name} enabled set to {active}");
            provider.enabled = active;
        }

        var teleports = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>(true);
        foreach (var provider in teleports)
            provider.enabled = active;

        var snapTurns = FindObjectsOfType<SnapTurnProviderBase>(true);
        foreach (var provider in snapTurns)
            provider.enabled = active;
>>>>>>> flashlight
    }
}