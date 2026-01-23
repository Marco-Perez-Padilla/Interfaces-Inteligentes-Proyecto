using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @file: CarretillaUI.cs
 * @brief: Controla la activación de una interfaz de usuario asociada a una carretilla
 * cuando el jugador entra en su zona trigger. Permite mostrar u ocultar la UI mediante
 * una acción de input configurable.
 *
 * Notas:
 * - La UI se muestra automáticamente al entrar en la zona.
 * - El jugador puede alternar la visibilidad mientras permanezca dentro.
 * - La acción de input se gestiona mediante el nuevo Input System.
 */
public class CarretillaUI : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;               // Panel de UI a mostrar/ocultar
    private bool playerInside = false;                          // Indica si el jugador está en la zona
    private bool uiVisible = false;                             // Estado actual de la UI

    [SerializeField] private InputActionReference hideMenuAction; // Acción de input para alternar la UI

    private void Start()
    {
        HideUI();                                               // Asegurar que la UI empieza oculta
    }
    
    private void OnEnable()
    {
        // Suscripción al input para alternar la UI
        hideMenuAction.action.performed += ToggleUI;
        hideMenuAction.action.Enable();
    }

    private void OnDisable()
    {
        // Desuscripción del input
        hideMenuAction.action.performed -= ToggleUI;
        hideMenuAction.action.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;                                    // El jugador entra en la zona
        ShowUI();                                               // Mostrar la UI automáticamente
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;                                   // El jugador sale de la zona
        HideUI();                                               // Ocultar la UI
    }

    private void ToggleUI(InputAction.CallbackContext context)
    {
        if (!playerInside)
            return;                                             // Evitar togglear fuera de la zona

        uiVisible = !uiVisible;                                 // Alternar estado
        uiPanel.SetActive(uiVisible);
    }

    private void ShowUI()
    {
        uiVisible = true;
        uiPanel.SetActive(true);
    }

    private void HideUI()
    {
        uiVisible = false;
        uiPanel.SetActive(false);
    }
}
