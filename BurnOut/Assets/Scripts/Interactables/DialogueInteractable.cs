using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private TextAsset inkJSON;
    [SerializeField] private bool canInteract;
    [SerializeField] private bool interactAgain;
    public bool CanInteract { get { return canInteract; } set { canInteract = value; } }

    private void Start()
    {
        CanInteract = canInteract;
    }

    /// <summary>
    /// Run dialogue 
    /// </summary>
    public void Interact()
    {
        if(CanInteract && !DialogueManager.Instance.DialogueIsPlaying)
        {
            DialogueManager.Instance.StartDialogue(inkJSON);
            canInteract = interactAgain;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {

        }
        // add interact listener 
        //progressInput.action.started += CanInteract;
    }

    private void OnTriggerExit(Collider other)
    {
        // remove listener 

    }
}
