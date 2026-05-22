using UnityEngine;
using UnityEngine.InputSystem;
/**
 * @file: CarretillaUI.cs
 * @brief: Muestra un panel de interfaz de usuario que sigue al jugador mientras está dentro de una zona trigger.
 *
 * Notas:
 * - El panel se muestra automáticamente cuando el jugador entra en la zona.
 * - El panel se oculta cuando el jugador sale de la zona.
 * - El panel se puede ocultar manualmente usando una acción de entrada.
 */
public class CarretillaUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject uiPanel;

    [Header("Offset (local to the cart)")]
    [SerializeField] private Vector3 localOffset = new Vector3(0, 1.5f, 0.5f);

    [Header("Billboard Settings")]
    [SerializeField] private bool flipPanel = true;   // Voltear 180 grados para que el texto sea legible

    [Header("Input")]
    [SerializeField] private InputActionReference hideMenuAction;

    private bool playerInside = false;
    private bool uiVisible = false;
    private bool manuallyClosed = false; 

    /// <summary>
    /// Inicializa el estado del UI Panel.
    /// </summary>
    private void Start()
    {
        if (uiPanel == null)
        {
            Debug.LogError("CarretillaUI: Asigna el UI Panel en el Inspector.");
            return;
        }
        HideUI();
    }

    /// <summary>
    /// Suscribe a la acción de entrada para mostrar/ocultar el panel.
    /// </summary>
    private void OnEnable()
    {
        if (hideMenuAction != null)
        {
            hideMenuAction.action.performed += ToggleUI;
            hideMenuAction.action.Enable();
        }
    }

    /// <summary> 
    /// Desuscribe de la acción de entrada para evitar fugas de memoria.
    /// </summary>
    private void OnDisable()
    {
        if (hideMenuAction != null)
        {
            hideMenuAction.action.performed -= ToggleUI;
            hideMenuAction.action.Disable();
        }
    }

    /// <summary>
    /// Detecta cuando el jugador entra en la zona trigger y muestra el panel si no fue cerrado manualmente.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        
        // Solo mostrar automáticamente si el jugador NO lo cerró manualmente
        if (!uiVisible && !manuallyClosed)
            ShowUI();
    }

    ///  <summary>
    /// Detecta cuando el jugador sale de la zona trigger y oculta el panel.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        HideUI();
    }

    ///  <summary>
    /// Alterna la visibilidad del panel cuando se activa la acción de entrada, pero solo si el jugador está dentro de la zona trigger.
    /// </summary>
    private void ToggleUI(InputAction.CallbackContext context)
    {
        if (!playerInside) return;
        uiVisible = !uiVisible;
        uiPanel.SetActive(uiVisible);
        
        if (!uiVisible)
            manuallyClosed = true;
        else
            manuallyClosed = false;
        
        if (uiVisible)
            UpdatePanelPosition();
    }

    ///  <summary>
    /// Muestra el panel y actualiza su posición.
    /// </summary>
    private void ShowUI()
    {
        uiVisible = true;
        uiPanel.SetActive(true);
        UpdatePanelPosition();
    }

    ///  <summary>
    /// Oculta el panel y marca que fue cerrado manualmente.
    /// </summary>
    private void HideUI()
    {
        uiVisible = false;
        uiPanel.SetActive(false);
    }

    ///  <summary>
    /// Actualiza la posición del panel para que siempre esté delante del carro y mire hacia la cámara.
    /// </summary>
    private void UpdatePanelPosition()
    {
        if (uiPanel == null) return;

        // Posición: siempre delante del carro según su orientación + offset
        Vector3 worldOffset = transform.TransformDirection(localOffset);
        uiPanel.transform.position = transform.position + worldOffset;

        // Rotación: que el panel mire hacia la cámara
        if (Camera.main != null)
        {
            uiPanel.transform.LookAt(Camera.main.transform);
            
            // Si el texto se ve al revés, volteamos 180 grados alrededor de Y
            if (flipPanel)
                uiPanel.transform.Rotate(0, 180, 0);
        }
    }
}