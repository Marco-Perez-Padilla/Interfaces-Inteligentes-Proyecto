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

    ///  <summary>
    ///  Controla el temporizador de contacto y reinicia la escena si el contacto se mantiene durante el tiempo especificado.
    ///  </summary>
    void Update()
    {
        if (!inContact) return;
        contactTimer += Time.deltaTime;
        if (contactTimer >= holdTime)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Detecta cuando el jugador entra en contacto con el enemigo y comienza a contar el tiempo de contacto.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[PECE] TriggerEnter: {other.name} | tag: {other.tag}");
        if (!other.CompareTag(playerTag)) return;
        Debug.Log("[PECE] Contacto con Player detectado, iniciando timer");
        inContact = true;
    }

    /// <summary>
    /// Detecta cuando el jugador sale del contacto con el enemigo y resetea el temporizador.
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[PECE] TriggerExit: {other.name} | tag: {other.tag}");
        if (!other.CompareTag(playerTag)) return;
        Debug.Log("[PECE] Player salió, reseteando timer");
        inContact = false;
        contactTimer = 0f;
    }
}