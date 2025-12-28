using UnityEngine;
using System;

public class PlayerEnemyCollisionEvent : MonoBehaviour
{
    [Header("Tags")]
    public string enemyTag = "Enemy";

    public delegate void PlayerHitByEnemy(GameObject enemy);
    public event PlayerHitByEnemy OnPlayerHit;

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(enemyTag))
            return;

        OnPlayerHit?.Invoke(collision.gameObject);
    }
}
