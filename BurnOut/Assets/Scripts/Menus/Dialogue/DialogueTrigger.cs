using Unity.VisualScripting;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private TextAsset inkJSON;
    [SerializeField] private bool play = true;
    [SerializeField] private bool playAgain = false;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(inkJSON);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (play)
        {
            play = playAgain;
            DialogueManager.Instance.StartDialogue(inkJSON);
        }
    }
}
