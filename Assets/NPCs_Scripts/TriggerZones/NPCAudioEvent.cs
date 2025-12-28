using UnityEngine;

public class NPCAudio : MonoBehaviour
{
    public AudioClip audioClip;
    public TriggerNotificator triggerZone;

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
