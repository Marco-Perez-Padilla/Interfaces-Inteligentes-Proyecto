using System.Collections.Generic;
using UnityEngine;

/**
 * @file CartDecisionController.cs
 * @brief Controla decisiones del jugador en bifurcaciones reales.
 *
 * INPUT (RELATIVO A LA CÁMARA):
 * - A / ← : izquierda
 * - D / → : derecha
 * - W / ↑ : frente
 * - Space : confirmar
 *
 * REGLAS:
 * - Si la dirección no existe, NO se mueve
 * - NO se reasigna otra salida automáticamente
 * - Solo se muestra mensaje por consola
 */
public class CartDecisionController : MonoBehaviour
{
  public CartMovement cart;

  private Dictionary<Direction, PathNode> exits;
  private Direction selected;

  private enum Direction
  {
    Left,
    Forward,
    Right
  }

  void Update()
  {
    if (!cart.isWaitingDecision)
      return;

    if (exits == null)
      BuildExits();

    HandleInput();
  }

  // ======================================================
  // BUILD EXITS (RELATIVE TO CAMERA)
  // ======================================================

  private void BuildExits()
  {
    exits = new Dictionary<Direction, PathNode>();

    Vector3 camForward = Camera.main.transform.forward;
    camForward.y = 0;
    camForward.Normalize();

    Vector3 camRight = Camera.main.transform.right;
    camRight.y = 0;
    camRight.Normalize();

    Vector3 origin = cart.Current.position;

    foreach (var next in cart.Current.connections)
    {
      if (next == cart.Previous)
        continue;

      Vector3 dir = next.position - origin;
      dir.y = 0;
      dir.Normalize();

      float forwardDot = Vector3.Dot(camForward, dir);
      float rightDot = Vector3.Dot(camRight, dir);

      if (forwardDot > 0.7f)
        exits[Direction.Forward] = next;
      else if (rightDot > 0.7f)
        exits[Direction.Right] = next;
      else if (rightDot < -0.7f)
        exits[Direction.Left] = next;
    }

    // Selección inicial: frente si existe
    if (exits.ContainsKey(Direction.Forward))
      selected = Direction.Forward;
    else if (exits.Count > 0)
      selected = new List<Direction>(exits.Keys)[0];
  }

  // ======================================================
  // INPUT
  // ======================================================

  private void HandleInput()
  {
    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
      TrySelect(Direction.Left);

    if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
      TrySelect(Direction.Right);

    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
      TrySelect(Direction.Forward);

    if (Input.GetKeyDown(KeyCode.Space))
      Confirm();
  }

  private void TrySelect(Direction dir)
  {
    if (!exits.ContainsKey(dir))
    {
      Debug.Log($"[Decision] Dirección inválida: {dir}");
      return;
    }

    selected = dir;
  }

  private void Confirm()
  {
    if (!exits.ContainsKey(selected))
    {
      Debug.Log("[Decision] No hay salida válida seleccionada");
      return;
    }

    cart.Choose(exits[selected]);
    exits = null;
  }
}
