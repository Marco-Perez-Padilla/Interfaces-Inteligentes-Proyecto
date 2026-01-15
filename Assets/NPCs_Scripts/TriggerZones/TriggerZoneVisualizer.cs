using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerZoneVisualizer : MonoBehaviour
{
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        Collider col = GetComponent<Collider>();
        if (col == null) return;

        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
        else if (col is CapsuleCollider capsule)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(transform.position, capsule.radius);
        }
    }
}
