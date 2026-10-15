using UnityEngine;

public class Room : MonoBehaviour
{
    // Make sure 'this' room's trigger collider is assigned
    private collider roomTrigger;

    private Bounds bounds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bounds = roomTrigger.bounds;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
