using UnityEngine;

/**
 * @file: TriggerNotificator.cs
 * @brief: Componente genérico que notifica eventos cuando un objeto con una etiqueta
 * específica entra o sale de un collider configurado como trigger.
 *
 * Notas:
 * - Pensado principalmente para detectar al jugador usando la etiqueta "Player".
 * - Expone eventos para desacoplar la lógica de detección de la lógica de reacción.
 * - Incluye soporte comentado para notificar solo una vez si se desea activar.
 * - El collider del GameObject se fuerza automáticamente a modo trigger.
 */
public class TriggerNotificator : MonoBehaviour
{
    public delegate void TriggerEvent();
    public event TriggerEvent OnPlayerEntered;   // Evento al entrar en el trigger
    public event TriggerEvent OnPlayerExited;    // Evento al salir del trigger

    public string targetTag = "Player";          // Etiqueta del objeto a detectar
    //public bool notifyOnlyOnce = true;          // Notificar solo la primera vez (opcional)

    //private bool notified = false;              // Control interno para notifyOnlyOnce

    void Awake()
    {
        // Busca específicamente el BoxCollider añadido por PathSurfaceBuilder.
        // El MeshCollider del prefab es cóncavo y no puede ser trigger.
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
            return;
        }

        Debug.LogWarning(
          $"[TriggerNotificator] No se encontró BoxCollider en {gameObject.name}. " +
          "Asegúrate de añadir un BoxCollider antes de añadir este componente.",
          this
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasTargetTagInHierarchy(other.transform))
            return;
        OnPlayerEntered?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!HasTargetTagInHierarchy(other.transform))
            return;
        OnPlayerExited?.Invoke();
    }

    private bool HasTargetTagInHierarchy(Transform t)
    {
        while (t != null)
        {
            if (t.CompareTag(targetTag))
                return true;
            t = t.parent;
        }
        return false;
    }
}