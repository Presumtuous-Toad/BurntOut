using Ink.Runtime;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Animator portraitAnimator;
    [SerializeField] private Animator canvasAnimator;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    [Header("Text Settings")]
    [SerializeField] private InputActionReference progressInput;
    public float letterWaitTime;

    [Header("Auto Advance Settings")]
    [SerializeField] private InputActionReference autoToggleInput;
    public bool autoAdvance = false;
    public float advanceWait = 2.5f;
    private Coroutine advanceCoroutine;

    private Story currentStory;
    private string currentSentence;
    private bool displayingChoice = false;
    private bool sentenceIsComplete = true;            // used for skipping dialogue writing
    public bool DialogueIsPlaying { get; private set; }

    public event EventHandler OnDialogueComplete;

    // dialogue tags
    private const string NAME_TAG = "name";
    private const string PORTRAIT_TAG = "portrait";
    private const string END_TAG = "end";
    private const string CANVAS_TAG = "canvas";

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

        DialogueIsPlaying = false;
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
    /// Play new dialogue using character dialogue box
    /// </summary>
    /// <param name="inkJSON">dialogue to play</param>
    public void StartDialogue(TextAsset inkJSON)
    {
        StopAllCoroutines();

        currentStory = new Story(inkJSON.text);
        EnableCanvas();

        // add input listeners
        progressInput.action.started += ProgressSentence;
        autoToggleInput.action.started += ToggleAutoAdvance;

        ProgressSentence();
    }

    /// <summary>
    /// Start dialogue and atach a method to play when dialogue is complete
    /// </summary>
    /// <param name="inkJson">dialogue to paly</param>
    /// <param name="listenerMethod">method to play when dialogue ends</param>
    public void StartDialogue(TextAsset inkJson, EventHandler listenerMethod)
    {
        OnDialogueComplete = null;  // temp fix
        OnDialogueComplete += listenerMethod;
        StartDialogue(inkJson);
    }

    private void EnableCanvas()
    {
        DialogueIsPlaying = true;
        dialogueCanvas.SetActive(true);
        canvasAnimator.enabled = true;
    }

    /// <summary>
    /// Calls base progress sentence on input
    /// </summary>
    private void ProgressSentence(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started )
        {
            ProgressSentence();
        }
    }

    /// <summary>
    /// Completes sentence early if incomplete or 
    /// plays next sentence 
    /// </summary>
    private void ProgressSentence()
    {
        // displaying all dialogue early 
        if (!sentenceIsComplete) 
        {
            StopAllCoroutines();
            dialogueText.text = currentSentence;
            sentenceIsComplete = true;
            return;
        }

        if (displayingChoice)
        {
            progressInput.action.started -= ProgressSentence;
            return;
        }

        DisplayNextSentence();
        HandleTags(currentStory.currentTags);
        DisplayChoices();
    }

    /// <summary>
    /// Plays next sentence of dialogue
    /// </summary>
    private void DisplayNextSentence()
    {
        sentenceIsComplete = false;

        // end dialogue
        if (!currentStory.canContinue)
        {
            sentenceIsComplete = true;
            EndDialogue();
            return;
        }

        currentSentence = currentStory.Continue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }

    /// <summary>
    /// Play a sentence from a given start point in dialogue
    /// </summary>
    /// <param name="inkJSON">file with all dialogue</param>
    /// <param name="pathName">location of dialogue to start at</param>
    public void JumpToSentenceInStory(TextAsset inkJSON, string pathName)
    {
        StopAllCoroutines();

        currentStory = new Story(inkJSON.text);
        currentStory.ChoosePathString(pathName);
        EnableCanvas();
        ProgressSentence();
    }

    /// <summary>
    /// Play a sentence from a given start point in dialogue
    /// </summary>
    /// <param name="inkJSON">dialogue to paly</param>
    /// <param name="pathName">location of dialogue to start at</param>
    /// <param name="listenerMethod">method to play when dialogue ends</param>
    public void JumpToSentenceInStory(TextAsset inkJSON, string pathName, EventHandler listenerMethod)
    {
        OnDialogueComplete = null;
        OnDialogueComplete += listenerMethod;
        JumpToSentenceInStory(inkJSON, pathName);
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
        if(autoAdvance && !displayingChoice)
        {
            advanceCoroutine = StartCoroutine(AutoAdvance());
        }
    }

    /// <summary>
    /// Turn off dialogue UI 
    /// </summary>
    private void EndDialogue()
    {
        // disconnect input
        progressInput.action.started -= ProgressSentence;
        autoToggleInput.action.started -= ToggleAutoAdvance;

        // run listening/clear events (if applicable)
        OnDialogueComplete?.Invoke(this, EventArgs.Empty);
        OnDialogueComplete = null;

        // turn off dialogue
        dialogueText.text = "";
        DialogueIsPlaying = false;
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
        // reenable dialogue progression
        progressInput.action.started += ProgressSentence;

        // turn off choices
        displayingChoice = false;
        foreach (GameObject choice in choices)
        {
            choice.SetActive(false);
        }

        currentStory.ChooseChoiceIndex(choiceIndex);
        ProgressSentence();
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
                case NAME_TAG:      // switch name
                    nameText.text = tagValue;
                    break;
                case PORTRAIT_TAG:  // switch portait
                    portraitAnimator.Play(tagValue);
                    break;
                case END_TAG:       // end early
                    EndDialogue();
                    break;
                case CANVAS_TAG:    // swap canvas position
                    canvasAnimator.Play(tagValue);
                    // disable dialogue controls for pop UI
                    if (tagValue == "pop_up")
                    {
                        autoAdvance = true;
                        DialogueIsPlaying = false;
                        progressInput.action.started -= ProgressSentence;
                        autoToggleInput.action.started -= ToggleAutoAdvance;
                    }
                    else if(tagValue == "default")
                    {
                        autoAdvance = false;
                        DialogueIsPlaying = true;
                        progressInput.action.started += ProgressSentence;
                        autoToggleInput.action.started += ToggleAutoAdvance;
                    }
                    break;
                default:
                    Debug.LogWarning("Tag not being handled: " + tag);
                    break;
            }
        }
    }

    private IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(advanceWait);
        ProgressSentence();
    }

    private void ToggleAutoAdvance(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            autoAdvance = !autoAdvance;
            if (autoAdvance)
            {
                advanceCoroutine = StartCoroutine(AutoAdvance());
            }
            else
            {
                if(advanceCoroutine != null) StopCoroutine(advanceCoroutine);
            }
        }
    }
}
