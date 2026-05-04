using UnityEngine;

/// <summary>
/// Spawner automático de NPCs enemigos. Al iniciar la escena, instancia
/// los prefabs de enemigos en posiciones próximas a los TriggerNotificator
/// existentes en la escena, con un offset lateral aleatorio para evitar
/// solapamientos. Cada prefab instanciado debe tener EnemyInitializer,
/// que se encargará de su configuración completa.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
  [Header("Prefabs de enemigos")]
  [Tooltip("Lista de prefabs de NPC que pueden ser instanciados. Deben llevar EnemyInitializer.")]
  [SerializeField] private GameObject[] enemyPrefabs;

  [Header("Configuración de spawn")]
  [Tooltip("Número de enemigos a instanciar en total.")]
  [SerializeField] private int totalEnemiesToSpawn = 10;

  [Tooltip("Radio máximo de desplazamiento lateral respecto al TriggerNotificator.")]
  [SerializeField] private float spawnOffsetRadius = 2f;

  [Tooltip("Offset en Y para evitar que el NPC aparezca dentro del suelo.")]
  [SerializeField] private float spawnHeightOffset = 0.1f;

  void Start()
  {
    TriggerNotificator[] availableTriggers = FindObjectsByType<TriggerNotificator>(FindObjectsSortMode.None);

    if (availableTriggers.Length == 0)
    {
      Debug.LogWarning("[EnemySpawner] No hay TriggerNotificators en la escena. No se instanciará ningún enemigo.");
      return;
    }

    if (enemyPrefabs == null || enemyPrefabs.Length == 0)
    {
      Debug.LogError("[EnemySpawner] No hay prefabs de enemigos asignados en el Inspector.");
      return;
    }

    SpawnEnemies(availableTriggers);
  }

  /// <summary>
  /// Instancia los enemigos distribuyéndolos entre los triggers disponibles
  /// en orden aleatorio, con un offset lateral para evitar solapamientos.
  /// </summary>
  /// <param name="availableTriggers">Array de TriggerNotificator presentes en la escena.</param>
  private void SpawnEnemies(TriggerNotificator[] availableTriggers)
  {
    for (int spawnIndex = 0; spawnIndex < totalEnemiesToSpawn; spawnIndex++)
    {
      TriggerNotificator targetTrigger = availableTriggers[Random.Range(0, availableTriggers.Length)];
      GameObject prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

      Vector3 spawnPosition = ComputeSpawnPosition(targetTrigger.transform.position);

      Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
    }
  }

  /// <summary>
  /// Calcula una posición de spawn con un desplazamiento aleatorio en XZ
  /// respecto a la posición base del trigger, más el offset vertical configurado.
  /// </summary>
  /// <param name="triggerPosition">Posición mundial del TriggerNotificator de referencia.</param>
  /// <returns>Posición final de spawn.</returns>
  private Vector3 ComputeSpawnPosition(Vector3 triggerPosition)
  {
    Vector2 randomOffset = Random.insideUnitCircle * spawnOffsetRadius;

    return new Vector3(
        triggerPosition.x + randomOffset.x,
        triggerPosition.y + spawnHeightOffset,
        triggerPosition.z + randomOffset.y
    );
  }
}