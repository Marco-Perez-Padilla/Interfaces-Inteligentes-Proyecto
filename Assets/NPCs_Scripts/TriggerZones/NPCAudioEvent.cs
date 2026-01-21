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
    public AudioClip audioClip;              // Clip de audio que se reproducirá
    public TriggerNotificator triggerZone;   // Zona trigger que activa el sonido

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = audioClip;
    }

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

    void PlaySound()
    {
        if (!audioSource.isPlaying)
            audioSource.Play();
    }
}
