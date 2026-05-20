using UnityEngine;

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

    Transform FindXROrigin()
    {
        GameObject xrObject = GameObject.Find("XR Origin Hands");
        if (xrObject == null) xrObject = GameObject.Find("XR Origin");
        if (xrObject == null) xrObject = GameObject.Find("Player");
        return xrObject?.transform;
    }

    void Initialize()
    {
        // Calcula el offset inicial (posición relativa al carrito)
        initialOffset = xrOrigin.position - cart.transform.position;
        isInitialized = true;
        Debug.Log($"XRFollowCart inicializado. Offset: {initialOffset}");
    }

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