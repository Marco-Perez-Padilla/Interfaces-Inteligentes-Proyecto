using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
/**
 * @file: gyro.cs
 * @brief: Visualiza la orientación del dispositivo utilizando el giroscopio.
 */

public class gyro : MonoBehaviour
{
    public Text displayText;
    private float anguloAcumulado = 0f;

    /// <summary>
    /// Habilita el giroscopio del dispositivo al iniciar la escena. Esto permite que el script pueda leer los datos de rotación del dispositivo para calcular el ángulo de orientación y mostrarlo en pantalla.
    /// Si el dispositivo no tiene giroscopio, el script seguirá funcionando utilizando la orientación de la cámara principal para determinar el ángulo, asegurando que la funcionalidad básica esté disponible en todos los dispositivos.
    /// </summary>
    void Start()
    {
        if (UnityEngine.InputSystem.Gyroscope.current != null)
            InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
    }

    /// <summary>
    /// Lee los datos del giroscopio para calcular el ángulo de orientación del dispositivo. Si el giroscopio no está disponible, utiliza la orientación de la cámara principal para determinar el ángulo. El ángulo se acumula y se muestra en pantalla junto con una representación cardinal (N, E, S, O) basada en el ángulo actual.
    /// Esto permite que el jugador tenga una referencia visual de su orientación en el espacio, lo que puede ser útil para la navegación o para interactuar con ciertos elementos del juego que dependan de la dirección en la que el jugador está mirando.
    /// </summary>
    void Update()
    {
        var gyroDevice = UnityEngine.InputSystem.Gyroscope.current;

        if (gyroDevice != null && gyroDevice.enabled)
        {
            Vector3 angularVelocity = gyroDevice.angularVelocity.ReadValue();
            float delta = angularVelocity.y * Mathf.Rad2Deg * Time.deltaTime;
            anguloAcumulado += delta;
        }
        else
        {
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0;
            forward.Normalize();
            float angulo = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            anguloAcumulado = (angulo + 360) % 360;
        }

        anguloAcumulado = (anguloAcumulado % 360 + 360) % 360;

        if (displayText != null)
            displayText.text =
                "Ángulo: " + anguloAcumulado.ToString("F1") + "°  " + AnguloACardinal(anguloAcumulado);
    }

    /// <summary>
    /// Convierte un ángulo en grados a una representación cardinal (N, E, S, O). Esto se utiliza para mostrar una referencia visual de la dirección en la que el jugador está mirando, lo que puede ser útil para la navegación o para interactuar con ciertos elementos del juego que dependan de la dirección.
    /// El método toma el ángulo acumulado y determina en qué rango se encuentra para asignarle la dirección cardinal correspondiente, proporcionando una forma fácil de entender la orientación del jugador en el espacio.
    /// </summary>
    string AnguloACardinal(float angulo)
    {
        if (angulo < 45 || angulo >= 315) return "N";
        if (angulo < 135) return "E";
        if (angulo < 225) return "S";
        return "O";
    }
}