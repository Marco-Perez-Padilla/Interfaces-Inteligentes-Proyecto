using UnityEngine;

/**
 * @file: PlayerEnemyCollisionEvent.cs
 * @brief: Detecta colisiones entre el jugador y enemigos. Al colisionar con un objeto
 * con la etiqueta "Enemy", dispara el evento de Game Over en el GameManager.
 *
 * Notas: Se basa en la etiqueta de los NPCs para determinar si son enemigos. No requiere
 * suscripciones ni referencias adicionales, solo que GameManager.Instance exista en escena.
 */
public class PlayerEnemyCollisionEvent : MonoBehaviour
{
    [Header("Tags")]
    public string enemyTag = "Enemy";   // Tag que identifica a los enemigos

    // Detecta colisiones físicas con otros colliders
    void OnCollisionEnter(Collision collision)
    {
        // Si el objeto colisionado no tiene la etiqueta "Enemy", no hacemos nada
        if (!collision.gameObject.CompareTag(enemyTag))
            return;

        // Llamada al GameManager para mostrar la pantalla de Game Over
        GameManager.Instance.GameOver();
    }
}
