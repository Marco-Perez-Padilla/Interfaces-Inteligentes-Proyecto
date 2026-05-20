using UnityEngine;
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
    public InputActionReference mountAction;

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
    }
}