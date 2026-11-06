using UnityEngine;

public class DialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private TextAsset inkJSON;
    [SerializeField] private GameObject interactIcon;
    [SerializeField] private bool canInteract;

    public GameObject Icon { get => interactIcon; }
    public bool CanInteract { get => canInteract; }

    private void Start()
    {
        if(DialogueManager.Instance == null)
        {
            Debug.LogWarning("Object referencing dialogue manager and it does not exist");
        }
        interactIcon.SetActive(false);
    }

    /// <summary>
    /// Run dialogue 
    /// </summary>
    public void Interact(PlayerController player)
    {
        if(CanInteract && !DialogueManager.Instance.DialogueIsPlaying)
        {
            canInteract = false;
            RemoveIcon();
            DialogueManager.Instance.StartDialogue(inkJSON);
        }
    }

    public void RemoveIcon()
    {
        if (Icon != null) Icon.SetActive(false);
    }
}
