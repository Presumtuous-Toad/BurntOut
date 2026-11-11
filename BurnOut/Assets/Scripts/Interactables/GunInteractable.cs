using UnityEngine;

public class GunInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject playerGun;
    [SerializeField] private GameObject ammoUI;
    [SerializeField] private GameObject interactIcon;
    [SerializeField] private bool canInteract;
    public GameObject Icon { get => interactIcon; }

    public bool CanInteract { get => canInteract; }

    public void Interact(PlayerController player)
    {
        if(canInteract)
        {
            // turn on player gun, turn off object
            canInteract = false;
            RemoveIcon();
            playerGun.SetActive(true);
            ammoUI.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }

    public void RemoveIcon()
    {
        if (Icon != null) Icon.SetActive(false);
    }
}
