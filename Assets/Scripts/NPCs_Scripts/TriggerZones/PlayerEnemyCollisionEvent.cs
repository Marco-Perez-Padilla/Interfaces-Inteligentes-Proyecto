using UnityEngine;

/**
 * @file: PlayerEnemyCollisionEvent.cs
 * @brief: Detecta colisiones entre el jugador y enemigos mediante OnTriggerEnter,
 * compatible con NPCs kinematic que usan SphereCollider trigger.
 * Requiere contacto sostenido durante holdTime segundos para disparar Game Over.
 */
public class PlayerEnemyCollisionEvent : MonoBehaviour
{
    [Header("Tags")]
    public string enemyTag = "Enemy";

    [Header("Hold Time")]
    [Tooltip("Segundos de contacto sostenido antes de Game Over")]
    public float holdTime = 25f;

    private float contactTimer = 0f;
    private bool inContact = false;

    void Update()
    {
        if (!inContact) return;

        contactTimer += Time.deltaTime;
        if (contactTimer >= holdTime)
        {
            inContact = false;
            contactTimer = 0f;
            GameManager.Instance.GameOver();
        }
    }

    // NPCs tienen SphereCollider isTrigger — usar OnTriggerEnter/Exit
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(enemyTag)) return;
        inContact = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(enemyTag)) return;
        inContact = false;
        contactTimer = 0f;
    }
}