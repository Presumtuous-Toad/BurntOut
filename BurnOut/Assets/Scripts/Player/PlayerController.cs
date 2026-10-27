using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;

    private Rigidbody rb;
    private Vector3 direction;
    private Vector3 velocity;
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        // move rigidbody 
        float deltaTime = Time.fixedDeltaTime;
        Vector3 position = gameObject.transform.position;

        if(direction == Vector3.zero)
        {
            return;
        }
        velocity = direction.normalized * speed;

        position.x += velocity.x * deltaTime;
        position.z += velocity.z * deltaTime;
        rb.MovePosition(position);

        // animate
        float animationVelX = Vector3.Dot(direction.normalized, transform.forward);
        float animationVelZ = Vector3.Dot(direction.normalized, transform.right);

        animator.SetFloat("VelocityX", animationVelX, 0.1f, Time.fixedDeltaTime);
        animator.SetFloat("VelocityZ", animationVelZ, 0.1f, Time.fixedDeltaTime);
    }

    public void Move(InputAction.CallbackContext context)
    {
        direction = new Vector3(context.ReadValue<Vector2>().x, 0 , context.ReadValue<Vector2>().y);
    }
}
