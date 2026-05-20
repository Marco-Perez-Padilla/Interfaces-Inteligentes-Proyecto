using UnityEngine;
using TMPro;

public class FloatingText3D : MonoBehaviour
{
    public Transform target;            
    public Vector3 offset = new Vector3(0, 0.3f, 0); 
    private TextMeshPro textMesh;
    public Camera cam; 

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;

        if (Camera.main != null)
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    public void SetText(string message)
    {
        textMesh.text = message;
    }
}
