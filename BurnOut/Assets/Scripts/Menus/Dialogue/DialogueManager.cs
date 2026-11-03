using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Canvas diaogueCanvas;

    public float letterWaitTime;

    private Queue<Dialogue> dialogues;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogues = new Queue<Dialogue>();
    }

    /// <summary>
    /// Play new dialogue
    /// </summary>
    /// <param name="newDialogues">dialogue to play</param>
    public void StartDialogue(Dialogue[] newDialogues)
    {
        dialogues.Clear();
        foreach (Dialogue d in newDialogues)
        {
            dialogues.Enqueue(d);
        }

        DisplayNextSentence();
    }

    /// <summary>
    /// Plays next sentence of dialogue
    /// </summary>
    public void DisplayNextSentence()
    {
        // end dialogue
        if (dialogues.Count == 0)
        {
            EndDialogue();
            return;
        }

        Dialogue currentSentence = dialogues.Dequeue();
        nameText.text = currentSentence.name;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence.sentence));
    }

    /// <summary>
    /// Types a letter at a time 
    /// </summary>
    /// <param name="sentence">sentence to write</param>
    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach(char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(letterWaitTime);
        }
    }

    /// <summary>
    /// Turn off dialogue UI 
    /// </summary>
    private void EndDialogue()
    {
        diaogueCanvas.enabled = false;
    }
}
