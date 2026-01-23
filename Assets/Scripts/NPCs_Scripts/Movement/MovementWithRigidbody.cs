using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @file: MovementWithRigidbody.cs
 * @brief: Controla el movimiento básico del jugador usando Rigidbody y entrada por teclado
 * (WASD) mediante el nuevo Input System.
 *
 * Notas:
 * - Este script se usa solo en Debug
 * - El movimiento se realiza en el plano XZ.
 * - La velocidad vertical (eje Y) se conserva para no interferir con la física (gravedad, saltos).
 * - La dirección se calcula en Update y se aplica en FixedUpdate para mantener coherencia física.
 */
public class MovementWithRigidbody : MonoBehaviour
{
    public float speed = 5f;        // Velocidad de movimiento del jugador

    private Rigidbody rigid;
    private Vector3 direction;      // Dirección de movimiento calculada a partir del input

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float moveX = 0f;
        float moveZ = 0f;

        // Lectura directa de teclado (WASD)
        if (Keyboard.current.aKey.isPressed) moveX -= 1f;
        if (Keyboard.current.dKey.isPressed) moveX += 1f;
        if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
        if (Keyboard.current.wKey.isPressed) moveZ += 1f;

        direction = new Vector3(moveX, 0f, moveZ).normalized;
    }

    private void FixedUpdate()
    {
        // Aplicar velocidad manteniendo la componente vertical del Rigidbody
        rigid.linearVelocity =
            direction * speed + new Vector3(0f, rigid.linearVelocity.y, 0f);
    }
}
