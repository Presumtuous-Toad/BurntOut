using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractable : MonoBehaviour
{
    [SerializeField] private InputActionReference interactInput;

    private void Start()
    {
        interactInput.action.started += Interact;
    }

    public void Interact(InputAction.CallbackContext context)
    {
        //context.cancel
    }



    private void OnDestroy()
    {
        
    }
}
