using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Camera mainCamera;
    [SerializeField]private ParticleSystem hose;
    [SerializeField] private InputActionReference hoseAction;


    private void Start()
    {
        mainCamera = Camera.main;
        hoseAction.action.started += OnHoseStarted;
        hoseAction.action.canceled += OnHoseStopped;
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 targetPoint = ray.GetPoint(rayDistance);
            Vector3 lookDirection = targetPoint - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-lookDirection);
            }
        }
    }

    private void OnHoseStarted(InputAction.CallbackContext context)
    {
        if(!hose.isPlaying)  hose.Play();
    }

    private void OnHoseStopped(InputAction.CallbackContext context)
    {
        if(hose.isPlaying) hose.Stop();
    }

    
}
