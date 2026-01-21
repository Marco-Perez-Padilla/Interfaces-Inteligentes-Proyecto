using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @file: MoveCamera.cs
 * @brief: Controla la rotación de la cámara en primera persona usando el ratón mediante
 * el nuevo Input System. La rotación vertical se aplica a la cámara y la horizontal al cuerpo
 * del jugador.
 *
 * Notas:
 * - Este script se usa solo en Debug
 * - El eje Y del ratón controla la rotación vertical (pitch) con límites para evitar giros extremos.
 * - El eje X del ratón rota el cuerpo del jugador (yaw).
 * - El cursor se bloquea y oculta al iniciar para una experiencia inmersiva.
 */
public class MoveCamera : MonoBehaviour
{
    public Transform playerBody;          // Cuerpo del jugador que rota horizontalmente
    public float mouseSensitivity = 100f; // Sensibilidad del ratón

    private float xRotation = 0f;         // Rotación vertical acumulada de la cámara

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        float mouseX = delta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = delta.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
