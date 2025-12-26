using UnityEngine;
using TMPro;

public class FloatingText3D : MonoBehaviour
{
    public Transform target;            
    public Vector3 offset = new Vector3(0, 100, -50); 
    private TextMeshPro textMesh;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;

        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    public void SetText(string message)
    {
        textMesh.text = message;
    }
}
