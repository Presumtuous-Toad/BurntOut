using UnityEngine;
using UnityEngine.InputSystem;

public class MousePosition : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Transform player;
    [SerializeField] private float maxDistance;

    // Update is called once per frame
    void Update()
    {
        MouseFollow();
    }

    /// <summary>
    /// Move object with script to mouse position in world 
    /// </summary>
    private void MouseFollow()
    {
        // object follows mouse in scene  
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, layerMask))
        {
            Vector3 mousePosition = hit.point;

            // apply mouse follow bounds  
            mousePosition.x = Mathf.Clamp(mousePosition.x,
                player.position.x - maxDistance,
                player.position.x + maxDistance);
            mousePosition.z = Mathf.Clamp(mousePosition.z,
                player.position.z - maxDistance,
                player.position.z + maxDistance);

            this.transform.position = mousePosition;
        }
    }
}
