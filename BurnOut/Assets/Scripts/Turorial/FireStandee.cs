using UnityEngine;

public class FireStandee : MonoBehaviour
{
    private float tipThreshold = 0.6f;

    void Update()
    {
        // check if tipped over 
        if(transform.up.y < tipThreshold)
        {
            Debug.Log("knocked");
            TutorialManager.Instance.KnockStandee();
            Destroy(this);  // destory script
        }
    }
}
