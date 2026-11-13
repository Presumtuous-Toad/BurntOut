using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject interactIcon;
    [SerializeField] private bool canInteract;

    public bool isOpen = false;

    public GameObject Icon { get => interactIcon; }
    public bool CanInteract { get => canInteract; }

    private void Start()
    {
        if (isOpen)
        {
            animator.Play("open");
        }
        else
        {
            animator.Play("close");
        }
    }

    /// <summary>
    /// Run dialogue 
    /// </summary>
    public void Interact(PlayerController player)
    {
        if (CanInteract)
        {
            if (isOpen)
            {
                animator.Play("close");
            }
            else
            {
                animator.Play("open");
            }
        }
    }

    public void RemoveIcon()
    {
        if (Icon != null) Icon.SetActive(false);
    }
}
