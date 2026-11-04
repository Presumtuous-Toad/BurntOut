using Ink.Runtime;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Animator portraitAnimator;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    [Header("Text Settings")]
    [SerializeField] private InputActionReference progressInput;
    public float letterWaitTime;

    [Header("Auto Play Settings")]
    [SerializeField] private InputActionReference autoToggleInput;
    public bool autoPlay = false;
    public float autoPlayWait = 5;
    private Coroutine autoPlayCoroutine;

    private Story currentStory;
    private string currentSentence;
    private bool displayingChoice = false;
    private bool sentenceIsComplete = true;            // used for skipping dialogue writing
    private bool dialogueIsPlaying = false;

    // dialogue tags
    private const string NAME_TAG = "name";
    private const string PORTRAIT_TAG = "portrait";


    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Found more than one DialogueManager in scene");
            Destroy(Instance);
        }
    }

    void Start()
    {
        // removed for testing purposes
        //dialogueCanvas.SetActive(false);

        // get all choice text
        choicesText = new TextMeshProUGUI[choices.Length];
        for (int i = 0; i < choices.Length; i++)
        {
            choicesText[i] = choices[i].GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    /// <summary>
    /// Play new dialogue
    /// </summary>
    /// <param name="inkJSON">dialogue to play</param>
    public void StartDialogue(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialogueCanvas.SetActive(true);

        // add input listeners
        progressInput.action.started += ProgressSentence;
        autoToggleInput.action.started += ToggleAuto;

        ProgressSentence();
    }

    /// <summary>
    /// Calls base progress sentence on input
    /// </summary>
    public void ProgressSentence(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            ProgressSentence();
        }
    }

    /// <summary>
    /// Completes sentence early if incomplete or 
    /// plays next sentence 
    /// </summary>
    public void ProgressSentence()
    {
        // displaying all dialogue early 
        if (!sentenceIsComplete)
        {
            StopAllCoroutines();
            dialogueText.text = currentSentence;
            sentenceIsComplete = true;

            // disable dialogue progression until choice is made
            if (displayingChoice)
            {
                progressInput.action.started -= ProgressSentence;
            }
            
            return;
        }

        DisplayNextSentence();
        HandleTags(currentStory.currentTags);
        DisplayChoices();
    }

    /// <summary>
    /// Plays next sentence of dialogue
    /// </summary>
    public void DisplayNextSentence()
    {
        sentenceIsComplete = false;

        // end dialogue
        if (!currentStory.canContinue)
        {
            EndDialogue();
            return;
        }

        currentSentence = currentStory.Continue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }

    /// <summary>
    /// Types a letter at a time, 
    /// plays next sentence if auto play is on 
    /// </summary>
    /// <param name="sentence">sentence to write</param>
    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(letterWaitTime);
        }
        sentenceIsComplete = true;

        // wait to play next line
        if(autoPlay && !displayingChoice)
        {
            autoPlayCoroutine = StartCoroutine(AutoNext());
        }
    }

    /// <summary>
    /// Turn off dialogue UI 
    /// </summary>
    private void EndDialogue()
    {
        // disconnect input
        progressInput.action.started -= ProgressSentence;
        autoToggleInput.action.started += ToggleAuto;

        dialogueText.text = "";
        dialogueIsPlaying = false;
        dialogueCanvas.SetActive(false);
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count == 0) return;
        
        // check if there are enough UI choices for dialogue
        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices in dialogue than UI can support.");
        }

        // display all choices
        displayingChoice = true;
        for (int i = 0; i < currentChoices.Count; i++)
        {
            choices[i].SetActive(true);
            choicesText[i].text = currentChoices[i].text;
        }

        StartCoroutine(SelectFirstChoice());
    }

    public void MakeChoice(int choiceIndex)
    {
        currentStory.ChooseChoiceIndex(choiceIndex);
        ProgressSentence();

        // reenable dialogue progression
        progressInput.action.started += ProgressSentence;
        displayingChoice = false;

        // turn off choices
        foreach (GameObject choice in choices)
        {
            choice.SetActive(false);
        }
    }

    /// <summary>
    /// Set starting choice to first choice
    /// </summary>
    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    /// <summary>
    /// Apply changes to dialogue UI based on tags set
    /// </summary>
    /// <param name="currentTags"></param>
    private void HandleTags(List<string> currentTags)
    {
        // loop through all tags
        foreach(string tag in currentTags)
        {
            // parse tag
            string[] splitTag = tag.Split(':');
            if(splitTag.Length != 2 )
            {
                Debug.LogError("Tag could not be pased " + tag);
            }
            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            // identify tag
            switch(tagKey)
            {
                case NAME_TAG:
                    nameText.text = tagValue;
                    break;
                case PORTRAIT_TAG:
                    portraitAnimator.Play(tagValue);
                    break;
                default:
                    Debug.LogWarning("Tag not being handled: " + tag);
                    break;
            }
        }
    }

    private IEnumerator AutoNext()
    {
        Debug.Log("Auto play started");
        yield return new WaitForSeconds(autoPlayWait);
        sentenceIsComplete = false;
        ProgressSentence();
    }

    private void ToggleAuto(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            autoPlay = !autoPlay;
            if (autoPlay)
            {
                autoPlayCoroutine = StartCoroutine(AutoNext());
            }
            else
            {
                if(autoPlayCoroutine != null) StopCoroutine(autoPlayCoroutine);
            }
        }
    }
}
