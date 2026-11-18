using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float normalSpeed;
    [SerializeField] private float encumberedSpeed;
    [SerializeField] private float recoilMultiplier;
    private float speed;
    [SerializeField] private LayerMask layerMask;

    private Camera cam;
    private Rigidbody rb;
    private Animator animator;
    private GunController gun;

    private Vector3 direction;
    public Vector3 velocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = FindAnyObjectByType<Camera>();
        rb = gameObject.GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        gun = GetComponentInChildren<GunController>();
    }

    void FixedUpdate()
    {
    
        // dialogue is playing, player does not move
        if(DialogueManager.Instance != null)
        {
            if (DialogueManager.Instance.DialogueIsPlaying) return;
        }

        // adjust speed when carrying fire hyrant
        if (FireHydrant.isHydrantPickedUp)
        {
            speed = encumberedSpeed;
        }
        else
        {
            speed = normalSpeed;
        }

        //if firing, apply force in direction opposite where firing
        

        // move rigidbody 
        float deltaTime = Time.fixedDeltaTime;
        Vector3 position = gameObject.transform.position;

        // rotate 
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit, 100f, layerMask))
        {
            Vector3 lookAt = hit.point;
            lookAt.y = transform.position.y;

            if (lookAt != Vector3.zero)
            {
                transform.LookAt(lookAt);
            }
        }
        
        if (direction == Vector3.zero && !gun.firing)
        {
            return;
        }


        velocity = direction.normalized * speed;
        if (gun.firing)
        {
            velocity += (-transform.forward * recoilMultiplier);
        }
        position += velocity * deltaTime;
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
