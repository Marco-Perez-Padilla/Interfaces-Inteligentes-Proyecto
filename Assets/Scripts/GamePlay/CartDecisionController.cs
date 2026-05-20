using System.Collections.Generic;
using UnityEngine;
<<<<<<< HEAD

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
=======
using UnityEngine.InputSystem;

/**
 * @file CartDecisionController.cs
 * @brief Controla decisiones del jugador en bifurcaciones usando Input System (VR + teclado).
 *        La dirección se elige inmediatamente al mover el joystick.
 */
public class CartDecisionController : MonoBehaviour
{
    [Header("References")]
    public CartMovement cart;

    [Header("Input Actions")]
    public InputActionReference moveAction;      // Acción Move (joystick)

    private Dictionary<Direction, PathNode> exits;

    private enum Direction { Left, Forward, Right }

    void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
    }

    void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
    }

    void Update()
    {
        if (!cart.isWaitingDecision)
        {
            if (exits != null) exits = null;
            return;
        }

        if (exits == null)
            BuildExits();

        // Leer el joystick
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        HandleDirectionInput(input);
    }

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
            if (next == cart.Previous) continue;

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

        Debug.Log($"[Decision] Nodo con {exits.Count} salidas.");
    }

    private void HandleDirectionInput(Vector2 input)
    {
        if (input.magnitude < 0.3f) return; // Umbral antirruido

        Direction dir;
        if (input.x < -0.5f)
            dir = Direction.Left;
        else if (input.x > 0.5f)
            dir = Direction.Right;
        else if (input.y > 0.5f)
            dir = Direction.Forward;
        else
            return;

        if (!exits.ContainsKey(dir))
        {
            Debug.Log($"[Decision] Dirección {dir} no disponible");
            return;
        }

        // Elegir inmediatamente
        cart.Choose(exits[dir]);
        exits = null; // Limpiar para la próxima vez
        Debug.Log($"[Decision] Elegida dirección {dir}");
    }
}
>>>>>>> flashlight
