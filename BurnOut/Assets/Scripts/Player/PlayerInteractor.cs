using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private InputActionReference interactInput;
    [SerializeField] private PlayerController player;

    private List<IInteractable> interactablesInRange;

    private void Start()
    {
        interactInput.action.started += Interact;
        interactablesInRange = new List<IInteractable>();
    }

    private void OnDestroy()
    {
        interactInput.action.started -= Interact;
    }

    /// <summary>
    /// Run interact for all objects in range
    /// </summary>
    /// <param name="context"></param>
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            foreach(IInteractable interactable in interactablesInRange)
            {
                interactable.Interact(player);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // add interactable and turn on icon
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactable.DisplayIcon();
            interactablesInRange.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // remove interactable and turn off icon
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactable.RemoveIcon();
            interactablesInRange.Remove(interactable);
        }
    }
}
