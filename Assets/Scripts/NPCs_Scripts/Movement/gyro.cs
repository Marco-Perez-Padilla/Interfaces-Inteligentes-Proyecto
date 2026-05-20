using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class gyro : MonoBehaviour
{
    public Text displayText;
    private float anguloAcumulado = 0f;

    void Start()
    {
        if (UnityEngine.InputSystem.Gyroscope.current != null)
            InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
    }

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

    string AnguloACardinal(float angulo)
    {
        if (angulo < 45 || angulo >= 315) return "N";
        if (angulo < 135) return "E";
        if (angulo < 225) return "S";
        return "O";
    }
}