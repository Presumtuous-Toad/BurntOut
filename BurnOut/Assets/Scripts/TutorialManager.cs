using UnityEngine;




public class TutorialManager : MonoBehaviour
{
    [Header("Controls UI to Display")]
    public GameObject Move_UI;
    public GameObject Shoot_UI;
    public GameObject Reload_UI;

    [Header("Dialogue to Play")]
    public TextAsset Move_Json;
    public TextAsset Shoot_Json;
    public TextAsset Reload_Json;
    public TextAsset Chair_Json;

    [SerializeField] private Phases currentPhase = Phases.Move;
    private Phases prevPhase = Phases.None;

    private enum Phases
    {
        None,
        Move,
        Shoot,
        Reload, 
        Chair
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NextPhase()
    {
        currentPhase++;
        switch (currentPhase)
        {
            case Phases.Move:
                MovePhase();
                break;
            case Phases.Shoot:
                ShootPhase();
                break;
            case Phases.Reload:
                ReloadPhase();
                break;
            case Phases.Chair:
                break;
        }
    }

    public void ChangePhase(int phase)
    {
        currentPhase = (Phases)phase;
    }

    private void MovePhase()
    {
        Move_UI.SetActive(true);
        DialogueManager.Instance.StartDialogue(Move_Json);
    }

    private void ShootPhase()
    {
        Shoot_UI.SetActive(true);
        DialogueManager.Instance.StartDialogue(Shoot_Json);
    }

    private void ReloadPhase()
    {
        Reload_UI.SetActive(true);
        DialogueManager.Instance.StartDialogue(Reload_Json);
    }

    private void ChairPhase()
    {
        DialogueManager.Instance.StartDialogue(Chair_Json);
    }

}
