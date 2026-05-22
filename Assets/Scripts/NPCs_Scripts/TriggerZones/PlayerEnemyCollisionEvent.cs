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
    public string playerTag = "Seat";

    [Header("Hold Time")]
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
        Debug.Log($"[PECE] TriggerEnter: {other.name} | tag: {other.tag}");
        if (!other.CompareTag(playerTag)) return;
        Debug.Log("[PECE] Contacto con Player detectado, iniciando timer");
        inContact = true;
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[PECE] TriggerExit: {other.name} | tag: {other.tag}");
        if (!other.CompareTag(playerTag)) return;
        Debug.Log("[PECE] Player salió, reseteando timer");
        inContact = false;
        contactTimer = 0f;
    }
}