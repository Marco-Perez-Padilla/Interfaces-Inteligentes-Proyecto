using UnityEngine;

public class DebugNPCs : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            // Ver si hay triggers en escena
            var triggers = FindObjectsByType<TriggerNotificator>(FindObjectsSortMode.None);
            Debug.Log($"TriggerNotificators en escena: {triggers.Length}");

            // Ver si hay player
            var player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log($"Player encontrado: {(player != null ? player.name : "NINGUNO")}");

            // Ver si hay NPCs
            var npcs = FindObjectsByType<NPCChasingEvent>(FindObjectsSortMode.None);
            Debug.Log($"NPCChasingEvent en escena: {npcs.Length}");
            foreach (var npc in npcs)
            {
                Debug.Log($"NPC: {npc.gameObject.name} | chasing={npc.enabled} | player={npc.player?.name} | trigger={npc.triggerZone?.name}");
            }
        }
    }
}