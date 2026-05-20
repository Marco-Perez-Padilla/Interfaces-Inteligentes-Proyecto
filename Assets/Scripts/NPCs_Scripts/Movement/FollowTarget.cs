using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target; 
    public Vector3 offset = new Vector3(-0.15f, 0.12f, 0.5f); 

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + target.rotation * offset;
        transform.rotation = target.rotation;
    }
}