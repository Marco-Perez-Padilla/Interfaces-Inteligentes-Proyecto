using UnityEngine;
/**
 * @file: FollowTarget.cs
 * @brief: Hace que un objeto siga a un objetivo con un offset definido.
 */

public class FollowTarget : MonoBehaviour
{
    public Transform target; 
    public Vector3 offset = new Vector3(-0.15f, 0.12f, 0.5f); 

    /// <summary>
    /// Actualiza la posición y rotación del objeto para seguir al objetivo con el offset definido. Esto se realiza en LateUpdate para asegurar que el seguimiento se realice después de que el objetivo haya actualizado su posición y rotación en el mismo frame, evitando posibles problemas de sincronización y asegurando un seguimiento suave y preciso.
    /// Si el objetivo no está asignado, el método simplemente retorna sin realizar ninguna acción, lo que permite que el objeto permanezca en su posición actual sin causar errores o comportamientos inesperados.
    /// </summary>
    void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + target.rotation * offset;
        transform.rotation = target.rotation;
    }
}