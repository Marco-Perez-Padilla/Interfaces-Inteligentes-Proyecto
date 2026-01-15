using UnityEngine;
using UnityEngine.InputSystem;

public class CarretillaUI : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;
    private bool playerInside = false;
    private bool uiVisible = false;

    // Referencia al asset de Input Actions
    [SerializeField] private InputActionReference hideMenuAction;

    private void Start()
    {
        HideUI();
    }
    
    private void OnEnable()
    {
        hideMenuAction.action.performed += ToggleUI;
        hideMenuAction.action.Enable();
    }

    private void OnDisable()
    {
        hideMenuAction.action.performed -= ToggleUI;
        hideMenuAction.action.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            ShowUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            HideUI();
        }
    }

    private void ToggleUI(InputAction.CallbackContext context)
    {
        if (!playerInside) return;

        uiVisible = !uiVisible;
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
