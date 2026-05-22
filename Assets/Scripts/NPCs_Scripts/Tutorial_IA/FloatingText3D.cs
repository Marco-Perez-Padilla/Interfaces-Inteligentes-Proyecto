using UnityEngine;
using TMPro;
/**
 * @file: FloatingText3D.cs
 * @brief: Muestra un texto flotante sobre un objeto en la escena.
 */

public class FloatingText3D : MonoBehaviour
{
    public Transform target;            
    public Vector3 offset = new Vector3(0, 0.3f, 0); 
    private TextMeshPro textMesh;
    public Camera cam; 

    /// <summary>
    /// Inicializa la referencia al componente TextMeshPro.
    /// </summary>
    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    /// <summary>
    /// Actualiza la posición del texto para que siga al objetivo y siempre mire hacia la cámara.
    /// </summary>
    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;

        if (Camera.main != null)
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    /// <summary>
    /// Establece el texto que se mostrará en el TextMeshPro.
    /// </summary>
    public void SetText(string message)
    {
        textMesh.text = message;
    }
}
