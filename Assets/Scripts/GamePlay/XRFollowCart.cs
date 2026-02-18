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

        Initialize();
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

        // Posición objetivo: si hay seatPoint, úsalo; si no, usa offset
        Vector3 targetPos;
        if (seatPoint != null)
            targetPos = seatPoint.position;
        else
            targetPos = cart.transform.position + initialOffset;

        // Interpolación suave de posición
        xrOrigin.position = Vector3.Lerp(xrOrigin.position, targetPos, positionLerpSpeed * Time.deltaTime);

        // Rotación: solo eje Y
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

        // Si hay seatPoint, teletransporta allí; si no, usa offset
        if (seatPoint != null)
        {
            xrOrigin.position = seatPoint.position;
            xrOrigin.rotation = cart.transform.rotation * Quaternion.Euler(rotationOffset);
        }
        else
        {
            xrOrigin.position = cart.transform.position + initialOffset;
            xrOrigin.rotation = cart.transform.rotation;
        }

        // Recalcula offset para mantener coherencia
        initialOffset = xrOrigin.position - cart.transform.position;
        Debug.Log("XR Origin teleportado al carrito");
    }
}