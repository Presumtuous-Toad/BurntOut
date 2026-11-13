using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TutorialPhases
{
    None,
    Move,
    Shoot,
    Reload,
    Chair,
    Leave
}

public class TutorialManager : MonoBehaviour
{
    [Header("Controls UI to Display")]
    public GameObject Move_UI;
    public GameObject Shoot_UI;
    public GameObject Reload_UI;

    [Header("Dialogue to Play")]
    public TextAsset Tutorial_Json;

    [Header("Path in Dialogue to Play")]
    public string Move_Path;
    public string Shoot_Path;
    public string Reload_Path;
    public string Chair_Path;
    public string Leave_Path;

    [Header("Remove at Start of Phase")]
    public List<GameObject> Chair_RemoveItems;

    [Header("Standee Count Info")]
    public int standeeTotal;
    private int standeeKnocked = 0;
    public string knock50_Path;
    public string knock75_Path;
    public string knock100_Path;
    private int trackRanDialogue = 0; // so dialouge only runs once 0 = run 50, 1 = run 75, 2 = run 100

    [Header("Special Move next Phase")]
    public GunController ammoCheck;
    public GameObject fireGone;
    public InputActionReference reloadAction;

    [SerializeField] private TutorialPhases currentPhase = TutorialPhases.Move;

    public TutorialPhases CurrentPhase { get { return currentPhase; } }
    public static TutorialManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Found more than one TutorialManager in scene");
            Destroy(Instance);
        }
    }

    private void Start()
    {
        MovePhase();
    }

    //Update is called once per frame
    void Update()
    {
        // check if ammo reached 0 then move to next phase
        if (currentPhase == TutorialPhases.Shoot && ammoCheck.ammo <= 0)
        {
            Debug.Log("ammo " + ammoCheck.ammo);
            NextPhase();
        }

        // if fire goes out next phase
        if(currentPhase == TutorialPhases.Chair && !fireGone.activeSelf)
        {
            NextPhase();
        }

        // run knock standee dialogue 
        if ((int)currentPhase >= (int)TutorialPhases.Shoot)
        {
            if (standeeKnocked == standeeTotal && trackRanDialogue == 2)
            {
                trackRanDialogue++;
                DialogueManager.Instance.JumpToSentenceInStory(Tutorial_Json, knock100_Path);
            }
            else if (standeeKnocked >= standeeTotal * 0.75 && trackRanDialogue == 1)
            {
                trackRanDialogue++;
                DialogueManager.Instance.JumpToSentenceInStory(Tutorial_Json, knock75_Path);
            }
            else if (standeeKnocked >= standeeTotal * 0.50 && trackRanDialogue == 0)
            {
                trackRanDialogue++;
                DialogueManager.Instance.JumpToSentenceInStory(Tutorial_Json, knock50_Path, (sender, e) => TurnOn(Shoot_UI));
            }
        }
    }

    public void NextPhase()
    {
        currentPhase++;
        switch (currentPhase)
        {
            case TutorialPhases.Move:
                MovePhase();
                break;
            case TutorialPhases.Shoot:
                ShootPhase();
                break;
            case TutorialPhases.Reload:
                ReloadPhase();
                break;
            case TutorialPhases.Chair:
                ChairPhase();
                break;
            case TutorialPhases.Leave:
                LeavePhase();
                break;
        }
    }

    public void NextPhase(InputAction.CallbackContext context)
    {
        NextPhase();
    }

    public void SetPhase(TutorialPhases phase)
    {
        currentPhase = phase;
        switch (currentPhase)
        {
            case TutorialPhases.Move:
                MovePhase();
                break;
            case TutorialPhases.Shoot:
                ShootPhase();
                break;
            case TutorialPhases.Reload:
                ReloadPhase();
                break;
            case TutorialPhases.Chair:
                ChairPhase();
                break;
            case TutorialPhases.Leave:
                LeavePhase();
                break;
        }
    }


    public void KnockStandee()
    {
        standeeKnocked++;
    }

    private void TurnOn(GameObject go)
    {
        go.SetActive(true);
    }


    private void MovePhase()
    {
        DialogueManager.Instance.StartDialogue(Tutorial_Json, (sender, e) => TurnOn(Move_UI));
    }

    private void ShootPhase()
    {
        Move_UI.SetActive(false);

        DialogueManager.Instance.JumpToSentenceInStory(Tutorial_Json, Shoot_Path, (sender, e) => TurnOn(Shoot_UI));
    }

    private void ReloadPhase()
    {
        Shoot_UI.SetActive(false);
        reloadAction.action.started += NextPhase;

        DialogueManager.Instance.JumpToSentenceInStory(Tutorial_Json, Reload_Path, (sender, e) => TurnOn(Reload_UI));
    }

    private void ChairPhase()
    {
        reloadAction.action.started -= NextPhase;
        Reload_UI.SetActive(false);

        foreach(GameObject item in Chair_RemoveItems)
        {
            item.SetActive(false);
        }

        DialogueManager.Instance.JumpToSentenceInStory(Tutorial_Json, Chair_Path);
    }

    private void LeavePhase()
    {
        DialogueManager.Instance.JumpToSentenceInStory(Tutorial_Json, Leave_Path);
    }

}
