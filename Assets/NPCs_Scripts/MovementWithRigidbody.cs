using UnityEngine;

public class MovementWithRigidbody : MonoBehaviour {
  public float speed = 5f;

  private Rigidbody rigid;
  private Vector3 direction;

  void Start() {
    rigid = GetComponent<Rigidbody>();
  }

  void Update() {
    float moveX = Input.GetAxisRaw("Horizontal"); 
    float moveZ = Input.GetAxisRaw("Vertical");  

    direction = new Vector3(moveX, 0f, moveZ).normalized;
  }

  void FixedUpdate() {
    rigid.linearVelocity = direction * speed + new Vector3(0, rigid.linearVelocity.y, 0);
  }
}