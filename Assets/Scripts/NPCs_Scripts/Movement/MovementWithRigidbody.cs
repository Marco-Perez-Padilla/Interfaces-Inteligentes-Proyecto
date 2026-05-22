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

    /// <summary>
    /// Obtiene la referencia al Rigidbody del objeto al iniciar la escena. Esto es necesario para aplicar las fuerzas de movimiento en el método FixedUpdate, asegurando que el movimiento del jugador se integre correctamente con el sistema de física de Unity.
    /// Si el Rigidbody no está presente, el script añadirá uno automáticamente para evitar errores y garantizar que el movimiento funcione correctamente.
    /// </summary>
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Lee la entrada del teclado para determinar la dirección de movimiento del jugador. La dirección se normaliza para asegurar que el movimiento tenga una velocidad constante en todas las direcciones, evitando que el jugador se mueva más rápido en diagonal. Esta dirección se almacena para ser aplicada posteriormente en FixedUpdate, donde se maneja la física del movimiento.
    /// Si el dispositivo de entrada del teclado no está disponible, el método simplemente retorna sin realizar ninguna acción, lo que permite que el jugador permanezca en su posición actual sin causar errores o comportamientos inesperados.
    /// </summary>
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

    /// <summary>
    /// Aplica la velocidad al Rigidbody del jugador en la dirección calculada, manteniendo la componente vertical del Rigidbody para no interferir con la física (gravedad, saltos). Esto se realiza en FixedUpdate para asegurar que el movimiento se integre correctamente con el sistema de física de Unity, proporcionando un movimiento suave y consistente.
    /// Si la dirección de movimiento es cero (no se presionan teclas), el método simplemente aplica la velocidad actual sin cambios, lo que permite que el jugador permanezca en su posición actual sin causar errores o comportamientos inesperados.
    /// </summary>
    private void FixedUpdate()
    {
        // Aplicar velocidad manteniendo la componente vertical del Rigidbody
        rigid.linearVelocity =
            direction * speed + new Vector3(0f, rigid.linearVelocity.y, 0f);
    }
}
