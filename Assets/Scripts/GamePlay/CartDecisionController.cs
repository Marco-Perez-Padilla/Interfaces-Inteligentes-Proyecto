using System.Collections.Generic;
using UnityEngine;

/**
 * @file CartDecisionController.cs
 * @brief Controla las decisiones de la vagoneta en nodos de decisión.
 *
 * Este script:
 * - Detecta cuando la vagoneta llega a un nodo de decisión
 * - Permite al jugador elegir una dirección
 * - Actualiza el main path del PathGenerator según la elección
 */
public class CartDecisionController : MonoBehaviour
{
  public CartMovement cart;
  public PathGenerator generator;

  private int selectedIndex;

  void Update()
  {
    PathNode node = cart.GetCurrentNode();
    if (node == null || !node.isDecisionNode)
      return;

    List<PathNode> exits = node.decisionExits;
    if (exits.Count < 2)
      return;

    // bloquear movimiento
    cart.allowAdvance = false;

    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
      selectedIndex = (selectedIndex - 1 + exits.Count) % exits.Count;

    if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
      selectedIndex = (selectedIndex + 1) % exits.Count;

    if (Input.GetKeyDown(KeyCode.Space))
    {
      AssignNewMainPath(exits[selectedIndex]);
      cart.allowAdvance = true;
    }
  }

  private void AssignNewMainPath(PathNode chosen)
  {
    foreach (var sub in generator.graph.subPaths)
    {
      if (sub.Contains(chosen))
      {
        generator.graph.mainPath = sub;
        return;
      }
    }
  }
}
