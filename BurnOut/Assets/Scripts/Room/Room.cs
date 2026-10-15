using UnityEngine;

/// <summary>
/// Every room should have a Room object holding this script with the collider adjusted to its width & height
/// </summary>
public class Room : MonoBehaviour
{
    // Make sure 'this' room's trigger collider is assigned
    private collider roomTrigger;

    private Bounds bounds;

    private float maximumViewDistance;  // The enemy view radius should not exceed this.

    // Event that is triggered when monster enters room

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Retrieve bound for this room
        bounds = roomTrigger.bounds;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Returns a list of patrol points that forms a square.
    /// </summary>
    /// <returns>A list of patrol points</returns>
    private List<Vector3> GenerateLoopPatrolPoints()
    {

    }

    /// <summary>
    /// Returns a list of patrol points that form a line. 
    /// </summary>
    /// <returns>A list of patrol points</returns>
    private List<Vector3> GenerateLinePatrolPoints()
    {

    }

    // ONTRIGGER - Call event
}
