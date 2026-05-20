using System.Collections.Generic;
using UnityEngine;
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
        if (input.magnitude < 0.3f) return;

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

        cart.Choose(exits[dir]);
        exits = null;
        Debug.Log($"[Decision] Elegida dirección {dir}");
    }
}