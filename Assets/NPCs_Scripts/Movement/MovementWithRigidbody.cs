using UnityEngine;
using UnityEngine.InputSystem;

public class MovementWithRigidbody : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody rigid;
    private Vector3 direction;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.aKey.isPressed) moveX -= 1f;
        if (Keyboard.current.dKey.isPressed) moveX += 1f;
        if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
        if (Keyboard.current.wKey.isPressed) moveZ += 1f;

        direction = new Vector3(moveX, 0f, moveZ).normalized;
    }

    private void FixedUpdate()
    {
        rigid.linearVelocity =
            direction * speed + new Vector3(0f, rigid.linearVelocity.y, 0f);
    }
}
