using UnityEngine;

/**
 * @file: TriggerZoneVisualizer.cs
 * @brief: Dibuja Gizmos en el editor para visualizar zonas trigger asociadas a colliders.
 *
 * Notas:
 * - Solo se utiliza para depuración visual en el editor (no afecta al juego en runtime).
 * - Soporta BoxCollider, SphereCollider y CapsuleCollider.
 * - El color y la transparencia del Gizmo pueden configurarse desde el Inspector.
 * - Requiere obligatoriamente un Collider en el GameObject.
 */
public class TriggerZoneVisualizer : MonoBehaviour
{
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);   // Color del Gizmo (incluye alpha)

    private void OnDrawGizmos()
    {
        // Asignar color del Gizmo
        Gizmos.color = gizmoColor;

        // Obtener el collider asociado
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        // Visualización para BoxCollider
        if (col is BoxCollider box)
        {
            // Usar la matriz local para respetar posición, rotación y escala
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
        // Visualización para SphereCollider
        else if (col is SphereCollider sphere)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
        // Visualización básica para CapsuleCollider
        else if (col is CapsuleCollider capsule)
        {
            // Representación simplificada usando una esfera
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(transform.position, capsule.radius);
        }
    }
}
