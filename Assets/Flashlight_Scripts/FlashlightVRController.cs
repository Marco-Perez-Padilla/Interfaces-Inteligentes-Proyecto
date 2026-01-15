using UnityEngine;

public class FlashlightVRController : MonoBehaviour
{
    [Header("Hands Parent (XR Hands)")]
    public Transform rightHandParent;   // RightHand padre (siempre existe)
    public Transform leftHandParent;    // LeftHand padre
    private Transform handVisual;       // mano actual a seguir

    [Header("Light Reference")]
    public Light flashlightLight;       // Spot Light de la linterna
    public bool isOn = true;            // estado inicial

    [Header("Grip Offset (opcional)")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Editor / Simulator Testing")]
    public bool useSimulatorController = true;  // para testing en editor
    public Transform rightControllerSimulator;  // arrastrar Right Controller del XR Device Simulator
    public Transform leftControllerSimulator;   // arrastrar Left Controller del XR Device Simulator

    void Start()
    {
        // Inicialmente en mano derecha
        handVisual = GetHandVisual(rightHandParent);

        // Encender / apagar linterna
        UpdateLight();
    }

    void Update()
    {
        // --- Encender / apagar linterna (editor / testing) ---
        if (Input.GetKeyDown(KeyCode.K))
            ToggleFlashlight();

        // --- Cambio de mano (editor / testing) ---
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (handVisual.parent == rightHandParent)
                SwitchHand(leftHandParent);
            else
                SwitchHand(rightHandParent);
        }

        // Aquí se pueden añadir triggers o gestos para VR real
    }

    void LateUpdate()
    {
        if (handVisual != null)
        {
            Transform target = handVisual;

#if UNITY_EDITOR
            if (useSimulatorController)
            {
                // En editor / simulator usa los controllers visibles
                if (handVisual.parent == rightHandParent && rightControllerSimulator != null)
                    target = rightControllerSimulator;
                else if (handVisual.parent == leftHandParent && leftControllerSimulator != null)
                    target = leftControllerSimulator;
            }
#endif

            // Seguir target con offset
            transform.position = target.position + target.TransformVector(positionOffset);
            transform.rotation = target.rotation * Quaternion.Euler(rotationOffset);
        }
    }

    // Encender / apagar linterna
    public void ToggleFlashlight()
    {
        isOn = !isOn;
        UpdateLight();
    }

    private void UpdateLight()
    {
        if (flashlightLight != null)
            flashlightLight.enabled = isOn;
    }

    // Cambiar de mano
    public void SwitchHand(Transform handParent)
    {
        handVisual = GetHandVisual(handParent);
    }

    // Detecta hijo visual correcto según plataforma
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

        // Si no encuentra, usa el primer hijo
        if (handParent.childCount > 0)
            return handParent.GetChild(0);

        return handParent; // fallback seguro
    }
}
