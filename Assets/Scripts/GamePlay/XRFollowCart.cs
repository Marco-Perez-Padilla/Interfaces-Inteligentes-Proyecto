using UnityEngine;

/**
 * Hace que XR Origin siga al carrito como si fuera su hijo,
 * pero sin los problemas de jerarquía de transformaciones
 */
public class XRFollowCart : MonoBehaviour
{
    [Header("References")]
    public CartMovement cart;
    public Transform xrOrigin;  // El XR Origin completo
    
    [Header("Follow Settings")]
    public bool followPosition = true;
    public bool followRotation = true;
    public float positionLerpSpeed = 10f;
    public float rotationLerpSpeed = 5f;
    public float positionThreshold = 0.01f;
    
    [Header("Height Control")]
    public bool maintainHeight = true;
    public float defaultHeight = 1.6f;
    public bool useCartHeight = false;
    
    private Vector3 lastCartPosition;
    private Quaternion lastCartRotation;
    private bool isInitialized = false;
    private Vector3 initialOffset;
    private float initialHeight;
    
    void Start()
    {
        if (cart == null)
        {
            Debug.LogError("XRFollowCart: No hay CartMovement asignado!");
            return;
        }
        
        if (xrOrigin == null)
        {
            // Busca automáticamente el XR Origin
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
        // Busca el XR Origin común en varios sistemas VR
        GameObject xrObject = GameObject.Find("XR Origin");
        if (xrObject == null) xrObject = GameObject.Find("XR Interaction Setup");
        if (xrObject == null) xrObject = GameObject.Find("Player");
        
        return xrObject?.transform;
    }
    
    void Initialize()
    {
        // Guarda posición/rotación inicial del carrito
        lastCartPosition = cart.transform.position;
        lastCartRotation = cart.transform.rotation;
        
        // Calcula offset inicial
        if (xrOrigin != null)
        {
            initialOffset = xrOrigin.position - cart.transform.position;
            initialHeight = xrOrigin.position.y;
        }
        
        isInitialized = true;
        Debug.Log($"XRFollowCart inicializado. Offset: {initialOffset}");
    }
    
    void LateUpdate()
    {
        if (!isInitialized) return;
        
        UpdateFollowing();
    }
    
    void UpdateFollowing()
    {
        if (followPosition)
        {
            UpdatePosition();
        }
        
        if (followRotation)
        {
            UpdateRotation();
        }
        
        // Actualiza referencias
        lastCartPosition = cart.transform.position;
        lastCartRotation = cart.transform.rotation;
    }
    
    void UpdatePosition()
    {
        if (xrOrigin == null) return;
        
        // Calcula cuánto se movió el carrito
        Vector3 cartMovement = cart.transform.position - lastCartPosition;
        
        // Si el movimiento es significativo
        if (cartMovement.magnitude > positionThreshold)
        {
            // Aplica el movimiento al XR Origin
            xrOrigin.position += cartMovement;
            
            // Control de altura
            if (maintainHeight)
            {
                Vector3 pos = xrOrigin.position;
                
                if (useCartHeight)
                {
                    // Mantiene altura relativa al carrito
                    pos.y = cart.transform.position.y + defaultHeight;
                }
                else
                {
                    // Mantiene altura absoluta
                    pos.y = initialHeight;
                }
                
                xrOrigin.position = pos;
            }
        }
    }
    
    void UpdateRotation()
    {
        if (xrOrigin == null) return;
        
        // Solo rotación en eje Y (para no inclinar al jugador)
        float cartYRotation = cart.transform.eulerAngles.y;
        float currentYRotation = xrOrigin.eulerAngles.y;
        
        // Interpola suavemente
        float newYRotation = Mathf.LerpAngle(
            currentYRotation, 
            cartYRotation, 
            rotationLerpSpeed * Time.deltaTime
        );
        
        // Aplica solo rotación Y
        Vector3 newRotation = xrOrigin.eulerAngles;
        newRotation.y = newYRotation;
        xrOrigin.rotation = Quaternion.Euler(newRotation);
    }
    
    // Método para resetear si hay problemas
    public void ResetFollowing()
    {
        if (xrOrigin == null || cart == null) return;
        
        // Vuelve a calcular offset
        initialOffset = xrOrigin.position - cart.transform.position;
        lastCartPosition = cart.transform.position;
        lastCartRotation = cart.transform.rotation;
        
        Debug.Log("Following resetado");
    }
    
    // Para teleportar XR Origin si es necesario
    public void TeleportToCart()
    {
        if (xrOrigin == null || cart == null) return;
        
        xrOrigin.position = cart.transform.position + initialOffset;
        xrOrigin.rotation = cart.transform.rotation;
        
        Debug.Log("XR Origin teleportado al carrito");
    }
    
    // DEPURACIÓN: Verifica distancias
    void OnDrawGizmosSelected()
    {
        if (cart != null && xrOrigin != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(cart.transform.position, xrOrigin.position);
            Gizmos.DrawWireSphere(xrOrigin.position, 0.5f);
        }
    }
}