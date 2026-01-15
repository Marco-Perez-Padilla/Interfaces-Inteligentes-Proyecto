using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightVRController : MonoBehaviour
{
    [Header("Hands Parent (XR Hands)")]
    public Transform rightHandParent;
    public Transform leftHandParent;
    private Transform handVisual;

    [Header("Light Reference")]
    public Light flashlightLight;
    public bool isOn = true;

    [Header("Grip Offset")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Input Actions")]
    public InputActionProperty toggleFlashlightAction; // tipo Button
    public InputActionProperty switchHandAction;       // tipo Button

    void OnEnable()
    {
        toggleFlashlightAction.action.Enable();
        switchHandAction.action.Enable();

        toggleFlashlightAction.action.performed += OnToggleFlashlight;
        switchHandAction.action.performed += OnSwitchHand;
    }

    void OnDisable()
    {
        toggleFlashlightAction.action.performed -= OnToggleFlashlight;
        switchHandAction.action.performed -= OnSwitchHand;

        toggleFlashlightAction.action.Disable();
        switchHandAction.action.Disable();
    }

    void Start()
    {
        handVisual = GetHandVisual(rightHandParent);
        UpdateLight();
    }

    void LateUpdate()
    {
        if (handVisual != null)
        {
            transform.position = handVisual.position + handVisual.TransformVector(positionOffset);
            transform.rotation = handVisual.rotation * Quaternion.Euler(rotationOffset);
        }
    }

    private void OnToggleFlashlight(InputAction.CallbackContext ctx)
    {
        isOn = !isOn;
        UpdateLight();
    }

    private void OnSwitchHand(InputAction.CallbackContext ctx)
    {
        if (handVisual.parent == rightHandParent)
            SwitchHand(leftHandParent);
        else
            SwitchHand(rightHandParent);
    }

    private void UpdateLight()
    {
        if (flashlightLight != null)
            flashlightLight.enabled = isOn;
    }

    public void SwitchHand(Transform handParent)
    {
        handVisual = GetHandVisual(handParent);
    }

    private Transform GetHandVisual(Transform handParent)
    {
        if (handParent == null) return null;

        string[] possibleNames = new string[]
        {
            "RightHandQuestVisual",
            "LeftHandQuestVisual",
            "RightHandAndroidXRVisual",
            "LeftHandAndroidXRVisual"
        };

        foreach (string name in possibleNames)
        {
            Transform t = handParent.Find(name);
            if (t != null)
                return t;
        }

        if (handParent.childCount > 0)
            return handParent.GetChild(0);

        return handParent;
    }
}
