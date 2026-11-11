using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeInteractable : MonoBehaviour, IInteractable
{
    public int sceneIndex;

    [SerializeField] private GameObject interactIcon;
    [SerializeField] private bool canInteract;

    public GameObject Icon { get => interactIcon; }
    public bool CanInteract { get => canInteract; }

    public void Interact(PlayerController player)
    {
        if (CanInteract)
        {
            canInteract = false;
            SceneManager.LoadScene(sceneIndex);
        }
    }

    public void RemoveIcon()
    {
        if (Icon != null) Icon.SetActive(false);
    }
}
