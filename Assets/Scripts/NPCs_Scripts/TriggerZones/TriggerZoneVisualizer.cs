using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Dibuja en el editor una representación visual del collider trigger
/// asociado al TriggerNotificator del mismo GameObject.
/// No afecta al comportamiento en runtime. Solo visible en Scene View.
/// </summary>
[RequireComponent(typeof(TriggerNotificator))]
public class TriggerZoneVisualizer : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Visualización")]
    [SerializeField] private Color zoneColor        = new Color(0f, 1f, 0.5f, 0.25f);
    [SerializeField] private Color zoneBorderColor  = new Color(0f, 1f, 0.5f, 0.9f);
    [SerializeField] private bool  showLabel        = true;

    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        DrawColliderGizmo(col);

        if (showLabel)
            Handles.Label(transform.position + Vector3.up * 0.5f, "TRIGGER", EditorStyles.boldLabel);
    }

    /// <summary>
    /// Dibuja el gizmo adaptado al tipo de collider presente en el GameObject.
    /// Soporta BoxCollider y SphereCollider.
    /// </summary>
    private void DrawColliderGizmo(Collider col)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider boxCollider)
        {
            Gizmos.color = zoneColor;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);

            Gizmos.color = zoneBorderColor;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
        else if (col is SphereCollider sphereCollider)
        {
            Gizmos.color = zoneColor;
            Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);

            Gizmos.color = zoneBorderColor;
            Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
        }

        Gizmos.matrix = originalMatrix;
    }
#endif
}