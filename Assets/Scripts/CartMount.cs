using UnityEngine;

/**
 * @class CartMount
 * @brief Gestiona la entrada y salida del jugador en la vagoneta.
 *
 * Este componente controla:
 * - El re-parentado de la cámara
 * - La activación del movimiento de la vagoneta
 * - La activación/desactivación de la vibración
 *
 * El jugador no desaparece: cambia de estado.
 */
public class CartMount : MonoBehaviour
{
    // =========================
    // REFERENCIAS
    // =========================

    /** @brief Punto donde se coloca la cámara dentro de la vagoneta */
    [Header("References")]
    public Transform seatPoint;

    /** @brief Componente de movimiento de la vagoneta */
    public CartMovement cartMovement;

    // =========================
    // ESTADO INTERNO
    // =========================

    /** @brief Transform del jugador */
    private Transform player;

    /** @brief Transform de la cámara principal */
    private Transform playerCamera;

    /** @brief Componente de vibración de cámara */
    private CameraShake cameraShake;

    /** @brief Indica si el jugador está montado */
    private bool isMounted = false;

    /**
     * @brief Inicializa referencias y estado inicial.
     */
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCamera = Camera.main.transform;

        cameraShake = playerCamera.GetComponent<CameraShake>();
        if (cameraShake != null)
            cameraShake.enableShake = false;

        cartMovement.enabled = false;
    }

    /**
     * @brief Detecta la entrada/salida de la vagoneta.
     */
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isMounted)
                Mount();
            else
                Unmount();
        }
    }

    /**
     * @brief Monta al jugador en la vagoneta.
     */
    void Mount()
    {
        DisablePlayerMovement();

        // Cámara pasa a la vagoneta
        playerCamera.SetParent(seatPoint);
        playerCamera.localPosition = Vector3.zero;
        playerCamera.localRotation = Quaternion.identity;

        cartMovement.enabled = true;

        if (cameraShake != null)
        {
            cameraShake.ResetOriginalPosition();
            cameraShake.enableShake = true;
        }

        isMounted = true;
    }

    /**
     * @brief Baja al jugador de la vagoneta.
     */
    void Unmount()
    {
        // Cámara vuelve al jugador
        playerCamera.SetParent(player);
        playerCamera.localPosition = new Vector3(0f, 1.6f, 0f);
        playerCamera.localRotation = Quaternion.identity;

        cartMovement.enabled = false;

        if (cameraShake != null)
        {
            cameraShake.ResetOriginalPosition();
            cameraShake.enableShake = false;
        }
        EnablePlayerMovement();
        isMounted = false;
    }

    /**
     * @brief Desactiva el movimiento del jugador.
     */
    void DisablePlayerMovement()
    {
        // Ejemplo:
        // player.GetComponent<PlayerMovement>().enabled = false;
    }

    /**
     * @brief Reactiva el movimiento del jugador.
     */
    void EnablePlayerMovement()
    {
        // player.GetComponent<PlayerMovement>().enabled = true;
    }
}
