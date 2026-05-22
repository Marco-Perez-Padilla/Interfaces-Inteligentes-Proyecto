using UnityEngine;

/**
 * @file: XRFollowCart.cs
 * @brief: Hace que el jugador siga al carrito con un offset definido en un entorno VR.
 */

public class XRFollowCart : MonoBehaviour
{
    [Header("References")]
    public CartMovement cart;
    public Transform xrOrigin;          // El XR Origin completo
    public Transform seatPoint;          // Opcional: punto del asiento (si no, se usa offset)

    [Header("Follow Settings")]
    public float positionLerpSpeed = 10f;
    public float rotationLerpSpeed = 5f;

    private Vector3 cameraLocalPosition; // offset de la cámara respecto al XR Origin
    private Vector3 initialOffset;       // Offset global entre XR Origin y carrito al montar
    public Vector3 rotationOffset = Vector3.zero;
    private bool isInitialized = false;

    /// <summary>
    /// Inicializa el seguimiento al carrito. Si no se asigna un XR Origin, intenta encontrar uno en la escena. Calcula el offset inicial entre el XR Origin y el carrito para mantener la posición relativa durante el seguimiento.
    /// Si se asigna un seatPoint, el XR Origin se posicionará para que la cámara esté en ese punto, ajustando el offset de la cámara para mantener la posición relativa correcta. Si no se asigna un seatPoint, el XR Origin seguirá al carrito utilizando el offset inicial calculado al inicio.
    /// </summary>
    void Start()
    {
        if (cart == null)
        {
            Debug.LogError("XRFollowCart: No hay CartMovement asignado!");
            return;
        }

        if (xrOrigin == null)
        {
            xrOrigin = FindXROrigin();
            if (xrOrigin == null)
            {
                Debug.LogError("XRFollowCart: No se encontró XR Origin!");
                return;
            }
        }

        // Busca la cámara principal dentro del XR Origin
        Camera cam = xrOrigin.GetComponentInChildren<Camera>();
        if (cam != null)
            cameraLocalPosition = cam.transform.localPosition;
        else
            cameraLocalPosition = Vector3.up * 1.5f; // valor por defecto
        Initialize();
    }

    /// <summary>
    /// Calcula la posición objetivo para el XR Origin. Si se asigna un seatPoint, el objetivo es el seatPoint menos el offset local de la cámara para mantener la posición correcta. Si no se asigna un seatPoint, el objetivo es la posición del carrito más el offset inicial calculado al inicio.
    /// Esto permite que el XR Origin siga al carrito de manera coherente, manteniendo la posición relativa correcta tanto si se utiliza un seatPoint como si no.
    /// </summary>
    Vector3 GetTargetPosition()
    {
        if (seatPoint != null)
        {
            // El punto donde queremos que esté la cámara es seatPoint.position
            // Pero nosotros movemos el rig → rig debe ir a seatPoint - cameraLocalOffset
            return seatPoint.position - cameraLocalPosition;
        }
        else
        {
            // Sin seatPoint, usa el carrito + offset inicial (ajustado también)
            return cart.transform.position + initialOffset;
        }
    }

    /// <summary>
    /// Busca el XR Origin en la escena. Primero intenta encontrar un objeto llamado "XR Origin Hands", luego "XR Origin", y finalmente "Player" como fallback. Esto permite que el script sea flexible y funcione con diferentes configuraciones de XR Origin sin necesidad de asignarlo manualmente.
    /// Si no se encuentra ningún XR Origin, el método retorna null, lo que puede ser manejado por el método Start para evitar errores y notificar al desarrollador sobre la falta de un XR Origin en la escena.
    /// </summary>
    Transform FindXROrigin()
    {
        GameObject xrObject = GameObject.Find("XR Origin Hands");
        if (xrObject == null) xrObject = GameObject.Find("XR Origin");
        if (xrObject == null) xrObject = GameObject.Find("Player");
        return xrObject?.transform;
    }

    /// <summary>
    /// Inicializa el seguimiento al carrito calculando el offset inicial entre el XR Origin y el carrito. Esto asegura que el XR Origin mantenga la posición relativa correcta al seguir al carrito, incluso si el jugador se teletransporta o se mueve de manera abrupta. El método también establece la bandera de inicialización para permitir que el seguimiento comience en el método LateUpdate.
    /// </summary>
    void Initialize()
    {
        // Calcula el offset inicial (posición relativa al carrito)
        initialOffset = xrOrigin.position - cart.transform.position;
        isInitialized = true;
        Debug.Log($"XRFollowCart inicializado. Offset: {initialOffset}");
    }

    /// <summary>
    /// Actualiza la posición y rotación del XR Origin para seguir al carrito. La posición se interpola suavemente hacia la posición objetivo calculada por el método GetTargetPosition, mientras que la rotación se interpola para seguir la rotación del carrito en el eje Y. Esto permite que el jugador siga al carrito de manera fluida y natural, manteniendo la orientación correcta mientras se mueve.
    /// Si el seguimiento no ha sido inicializado, el método retorna sin realizar ninguna acción, lo que evita errores y permite que el XR Origin permanezca en su posición actual hasta que se inicialice correctamente.
    /// </summary>
    void LateUpdate()
    {
        if (!isInitialized) return;

        Vector3 targetPos = GetTargetPosition();
        xrOrigin.position = Vector3.Lerp(xrOrigin.position, targetPos, positionLerpSpeed * Time.deltaTime);
        
        // Rotación igual que antes
        float targetY = cart.transform.eulerAngles.y;
        float currentY = xrOrigin.eulerAngles.y;
        float newY = Mathf.LerpAngle(currentY, targetY, rotationLerpSpeed * Time.deltaTime);
        Vector3 rot = xrOrigin.eulerAngles;
        rot.y = newY;
        xrOrigin.rotation = Quaternion.Euler(rot);
    }

    /// <summary>
    /// Teletransporta el XR Origin a la posición del carrito de forma instantánea, sin interpolación. Esto puede ser útil para situaciones donde el jugador necesita ser reposicionado rápidamente, como al montar el carrito o después de un teletransporte. El método también recalcula el offset inicial después de teletransportar para asegurar que el seguimiento continúe funcionando correctamente desde la nueva posición.
    /// Si el XR Origin o el carrito no están asignados, el método retorna sin realizar ninguna acción, lo que evita errores y permite que el juego continúe funcionando incluso si no se han configurado correctamente las referencias.
    /// </summary>
    public void TeleportToCart()
    {
        if (xrOrigin == null || cart == null) return;

        Vector3 targetPos = GetTargetPosition();
        xrOrigin.position = targetPos;
        
        if (seatPoint != null)
            xrOrigin.rotation = cart.transform.rotation * Quaternion.Euler(rotationOffset);
        else
            xrOrigin.rotation = cart.transform.rotation;

        // Recalcula offset para mantener coherencia
        initialOffset = xrOrigin.position - cart.transform.position;
        Debug.Log($"XR Origin teleportado. Posición del rig: {targetPos}");
    }
}