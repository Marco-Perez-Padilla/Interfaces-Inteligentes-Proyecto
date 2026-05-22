using UnityEngine;

/**
 * @file: NPCAudio.cs
 * @brief: Reproduce un sonido asociado a un NPC cuando el jugador entra en una zona trigger.
 *
 * Notas:
 * - El sonido se reproduce solo una vez mientras el AudioSource esté activo.
 * - El AudioSource debe estar en el mismo GameObject que este script.
 * - El TriggerNotificator define cuándo el jugador entra en la zona de activación.
 */
public class NPCAudio : MonoBehaviour
{
    [Header("References")]
    public TriggerNotificator triggerZone;

    [Header("Audio")]
    public AudioClip audioClip;

    private AudioSource audioSource;

    /// <summary>
    /// Asegura que el AudioSource esté configurado correctamente.
    /// </summary>
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;
        audioSource.spatialBlend = 0f;
    }

    /// <summary>
    /// Se suscribe al evento del TriggerNotificator para reproducir el sonido cuando el jugador entra en la zona.
    /// </summary>
    void OnEnable()
    {
        if (triggerZone != null)
            triggerZone.OnPlayerEntered += PlaySound;
    }

    /// <summary> 
    /// Se desuscribe del evento para evitar llamadas a PlaySound después de que el objeto esté deshabilitado o destruido. 
    /// </summary>
    void OnDisable()
    {
        if (triggerZone != null)
            triggerZone.OnPlayerEntered -= PlaySound;
    }

    /// <summary>
    /// Se asegura de desuscribirse del evento para evitar errores si el objeto es destruido mientras el evento aún está activo.
    /// </summary>
    void OnDestroy()
    {
        if (triggerZone != null)
            triggerZone.OnPlayerEntered -= PlaySound;
    }

    /// <summary>
    /// Llamar justo después de AddComponent para asignar referencias
    /// antes de que OnEnable se suscriba al evento.
    /// </summary>
    public void Setup(TriggerNotificator trigger, AudioClip clip)
    {
        triggerZone = trigger;
        audioClip = clip;

        if (triggerZone != null)
            triggerZone.OnPlayerEntered += PlaySound;
    }

    /// <summary>
    /// Reproduce el sonido si no se está reproduciendo actualmente.
    /// </summary>
    void PlaySound()
    {
        Debug.Log($"[NPCAudio] PlaySound en {gameObject.name}, clip: {audioClip}");
        if (audioSource.isPlaying) return;
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}