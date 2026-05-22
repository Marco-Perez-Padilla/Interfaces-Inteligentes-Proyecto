using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Start()
    {
        if (uiPanel == null)
        {
            Debug.LogError("CarretillaUI: Asigna el UI Panel en el Inspector.");
            return;
        }
        HideUI();
    }

    private void OnEnable()
    {
        if (hideMenuAction != null)
        {
            hideMenuAction.action.performed += ToggleUI;
            hideMenuAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (hideMenuAction != null)
        {
            hideMenuAction.action.performed -= ToggleUI;
            hideMenuAction.action.Disable();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        
        // Solo mostrar automáticamente si el jugador NO lo cerró manualmente
        if (!uiVisible && !manuallyClosed)
            ShowUI();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        HideUI();
    }

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

    private void ShowUI()
    {
        uiVisible = true;
        uiPanel.SetActive(true);
        UpdatePanelPosition();
    }

    private void HideUI()
    {
        uiVisible = false;
        uiPanel.SetActive(false);
    }

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