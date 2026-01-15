using UnityEngine;

/**
 * @class CartMount
 * @brief Gestiona la entrada/salida del jugador en la vagoneta.
 */
public class CartMount : MonoBehaviour
{
    public Transform seatPoint;
    public CartMovement cartMovement;

    private Transform player;
    private Transform cam;
    private CameraShake shake;
    private bool mounted;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main.transform;
        shake = cam.GetComponent<CameraShake>();

        if (shake != null)
            shake.enableShake = false;

        cartMovement.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!mounted) Mount();
            else Unmount();
        }
    }

    void Mount()
    {
        cam.SetParent(seatPoint);
        cam.localPosition = Vector3.zero;
        cam.localRotation = Quaternion.identity;

        if (shake != null)
        {
            shake.ResetOriginalPosition();
            shake.enableShake = true;
        }

        cartMovement.enabled = true;
        mounted = true;
    }

    void Unmount()
    {
        cam.SetParent(player);
        cam.localPosition = new Vector3(0f, 1.6f, 0f);
        cam.localRotation = Quaternion.identity;

        if (shake != null)
        {
            shake.ResetOriginalPosition();
            shake.enableShake = false;
        }

        cartMovement.enabled = false;
        mounted = false;
    }
}
