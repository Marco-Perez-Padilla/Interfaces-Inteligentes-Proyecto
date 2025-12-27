using UnityEngine;

public class TriggerNotificator : MonoBehaviour
{
    public delegate void TriggerEvent();
    public event TriggerEvent OnPlayerEntered;

    public string targetTag = "Player";
    //public bool notifyOnlyOnce = true;

    //private bool notified = false;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (notified && notifyOnlyOnce)
            //return;

        if (!other.CompareTag(targetTag))
            return;

        OnPlayerEntered?.Invoke();
        //notified = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetTag))
            return;

        OnPlayerExited?.Invoke();
    }
}
