using UnityEngine;

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
    }
}