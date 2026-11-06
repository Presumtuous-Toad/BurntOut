using Unity.VisualScripting;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private TextAsset inkJSON;

    private bool hasRun = false;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(inkJSON);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasRun)
        {
            DialogueManager.Instance.StartDialogue(inkJSON);
        }
    }
}
