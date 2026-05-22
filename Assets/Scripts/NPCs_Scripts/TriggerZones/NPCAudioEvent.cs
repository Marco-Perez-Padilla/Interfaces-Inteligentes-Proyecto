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
using UnityEngine;

public class NPCAudio : MonoBehaviour
{
    [Header("References")]
    public TriggerNotificator triggerZone;

    [Header("Audio")]
    public AudioClip audioClip;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;
        audioSource.spatialBlend = 0f;
    }

    // OnEnable se llama cada vez que el GO se activa,
    // pero aquí nos interesa que se llame DESPUÉS de Setup()
    void OnEnable()
    {
        if (triggerZone != null)
            triggerZone.OnPlayerEntered += PlaySound;
    }

    void OnDisable()
    {
        if (triggerZone != null)
            triggerZone.OnPlayerEntered -= PlaySound;
    }

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

        // Forzar resuscripción porque OnEnable ya se ejecutó con triggerZone null
        if (triggerZone != null)
            triggerZone.OnPlayerEntered += PlaySound;
    }

    void PlaySound()
    {
        Debug.Log($"[NPCAudio] PlaySound en {gameObject.name}, clip: {audioClip}");
        if (audioSource.isPlaying) return;
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}