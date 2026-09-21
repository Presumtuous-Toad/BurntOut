using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;

    private Rigidbody rb;
    private Vector3 direction;
    private Vector2 velocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.Log(this.name + " is missing rigidbody");
            gameObject.AddComponent<Rigidbody>();
        }
    }

    void FixedUpdate()
    {
        // move rigidbody 
        float deltaTime = Time.fixedDeltaTime;
        Vector3 position = gameObject.transform.position;

        velocity = direction.normalized * speed;
        position.x += velocity.x * deltaTime;
        position.z += velocity.y * deltaTime;
        rb.MovePosition(position);
    }

    public void Move(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>();
    }
}
