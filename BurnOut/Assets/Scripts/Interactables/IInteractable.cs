
using UnityEngine;

public interface IInteractable 
{
    public bool CanInteract { get; set; }

    /// <summary>
    /// What happens when player interacts with object 
    /// </summary>
    public void Interact();
}
