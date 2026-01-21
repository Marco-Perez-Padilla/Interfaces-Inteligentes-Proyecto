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
        // Asegura que el collider actúe como trigger
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si ya se notificó y solo se permite una notificación, se ignora
        //if (notified && notifyOnlyOnce)
        //    return;

        // Ignora cualquier objeto que no tenga la etiqueta objetivo
        if (!other.CompareTag(targetTag))
            return;

        // Notifica entrada en el trigger
        OnPlayerEntered?.Invoke();

        // Marca como notificado si se usa notifyOnlyOnce
        //notified = true;
    }

    private void OnTriggerExit(Collider other)
    {
        // Ignora cualquier objeto que no tenga la etiqueta objetivo
        if (!other.CompareTag(targetTag))
            return;

        // Notifica salida del trigger
        OnPlayerExited?.Invoke();
    }
}
