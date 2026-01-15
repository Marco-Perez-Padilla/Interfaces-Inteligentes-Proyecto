using System.Collections.Generic;

/**
 * @file SubPathCooldownResolver.cs
 * @brief Aplica reglas de nodos primordiales y cooldown de subrutas.
 *
 * Reglas:
 * - Los primeros N nodos son primordiales
 * - Tras un Pi, hay un cooldown de M nodos
 * - El cooldown bloquea SALIDAS, nunca ENTRADAS
 */
public static class SubPathCooldownResolver
{
  /**
   * @param mainPath Camino principal
   * @param primordialCount Nodos iniciales protegidos
   * @param cooldownLength Número de nodos de cooldown tras un Pi
   */
  public static void Apply(
      List<PathNode> mainPath,
      int primordialCount,
      int cooldownLength)
  {
    if (mainPath == null || mainPath.Count == 0)
      return;

    int cooldownRemaining = 0;

    for (int i = 0; i < mainPath.Count; i++)
    {
      PathNode node = mainPath[i];

      // -------------------------
      // PRIMORDIALES
      // -------------------------

      if (i < primordialCount)
      {
        node.isPrimordial = true;
        node.canStartSubPath = false;
        node.canReceiveSubPath = false;
        continue;
      }

      node.isPrimordial = false;

      // -------------------------
      // RECEPCIÓN
      // -------------------------
      // Siempre puede recibir salvo primordial
      node.canReceiveSubPath = true;

      // -------------------------
      // SALIDA (COOLDOWN)
      // -------------------------

      if (cooldownRemaining > 0)
      {
        node.canStartSubPath = false;
        cooldownRemaining--;
      }
      else
      {
        node.canStartSubPath = true;
      }

      // Si este nodo es Pi, activamos cooldown
      if (node.isPi)
      {
        cooldownRemaining = cooldownLength;
        node.canStartSubPath = true;
      }
    }
  }
}
