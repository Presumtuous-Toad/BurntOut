using UnityEngine;

public interface IInteractable
{
    /// <summary>
    /// Interact icon
    /// </summary>
    public GameObject Icon { get; }
    public bool CanInteract { get; }

    /// <summary>
    /// What happens when player interacts with object 
    /// </summary>
    public void Interact(PlayerController player);
    public virtual void DisplayIcon()
    {
        if(Icon != null && CanInteract) Icon.SetActive(true);
    }
    public virtual void RemoveIcon()
    {
        if(Icon != null) if(!CanInteract) Icon.SetActive(false);
    }

}
