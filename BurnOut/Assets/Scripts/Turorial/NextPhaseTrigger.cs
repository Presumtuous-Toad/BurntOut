using UnityEngine;

public class NextPhaseTrigger : MonoBehaviour
{
    public TutorialPhases changeTo;
    public TutorialPhases checkPrev;    // if none previous phase does not matter 

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("current phase: " + TutorialManager.Instance.CurrentPhase);
        if(checkPrev == TutorialPhases.None)
        {
            TutorialManager.Instance.SetPhase(changeTo);
            Destroy(this.gameObject);
        }
        else if(checkPrev == TutorialManager.Instance.CurrentPhase)
        {
            TutorialManager.Instance.SetPhase(changeTo);
            Destroy(this.gameObject);
        }
    }
}
