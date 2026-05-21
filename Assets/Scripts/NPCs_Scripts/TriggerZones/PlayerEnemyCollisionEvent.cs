using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * @file: PlayerEnemyCollisionEvent.cs
 * @brief: Detecta contacto sostenido entre el jugador y enemigos.
 * Tras holdTime segundos de contacto continuo, reinicia la escena.
 */
public class PlayerEnemyCollisionEvent : MonoBehaviour
{
    [Header("Tags")]
    public string enemyTag = "Enemy";

    [Header("Hold Time")]
    [Tooltip("Segundos de contacto sostenido antes de reiniciar")]
    public float holdTime = 3f;

    private float contactTimer = 0f;
    private bool inContact = false;

    void Update()
    {
        if (!inContact) return;

        contactTimer += Time.deltaTime;
        if (contactTimer >= holdTime)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

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