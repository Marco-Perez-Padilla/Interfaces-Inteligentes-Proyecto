using UnityEngine;
using TMPro;

/**
 * @file: FloatingText3D.cs
 * @brief: Muestra un texto 3D flotante que sigue a un objetivo en el mundo y
 * siempre mira hacia la cámara (billboard).
 *
 * Notas:
 * - Útil para mensajes diegéticos como avisos, pistas o feedback visual en 3D.
 * - El texto sigue al target con un offset configurable.
 * - La rotación se ajusta cada frame para mirar a la cámara.
 */
public class FloatingText3D : MonoBehaviour
{
    public Transform target;                            // Objeto al que seguirá el texto
    public Vector3 offset = new Vector3(0, 100, -50);    // Desplazamiento respecto al target
    public Camera cam;                                  // Cámara a la que mirará el texto

    private TextMeshPro textMesh;                        // Referencia al componente de texto

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();          // Obtener el componente TextMeshPro
    }

    void LateUpdate()
    {
        if (target == null || cam == null)
            return;

        // Posicionar el texto relativo al objetivo
        transform.position = target.position + offset;

        // Rotar el texto para que siempre mire a la cámara (billboard)
        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position
        );
    }

    // Cambia el mensaje mostrado por el texto flotante
    public void SetText(string message)
    {
        textMesh.text = message;
    }
}
