using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.AI;

/// <summary>
/// Every room should have a Room object holding this script with the collider adjusted to its width & height
/// </summary>
public class Room : MonoBehaviour
{
    // Make sure 'this' room's trigger collider is assigned
    [Header("Collider MUST be TRIGGER collider.")]
    private Collider roomTrigger;

    private Bounds bounds;

    // NOT ADDED YET
    // The enemy view radius should not exceed this. 
    // (TALK ABOUT THIS WITH TEAM) - Is restricting maximum view distance per room good or do we want it to be unique components of enemy?
    public float MaximumViewDistance { get; private set; } 

    [Header("PREPLACED monster before runtime should be added to this list for registeration.")]
    public List<FireMonster> fireMonsters;     

    // Event that is triggered when monster enters room - might not be needed

    // NOT IMPLEMENTED
    // Number of fire instances in each room. 
    private int fires;
    private int MaxFires;   // Depends on room size. If number of fire reaches the amount of max fire, add behavior to move monster to another room.

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
    public List<Vector3> GenerateLoopPatrolPoints()
    {
        return null;
    }

    /// <summary>
    /// Returns a list of patrol points that form a line. 
    /// </summary>
    /// <returns>A list of patrol points</returns>
    public List<Vector3> GenerateLinePatrolPoints()
    {
        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Enemy inside room triggered!");
            foreach (FireMonster monster in fireMonsters)
            {
                monster.Alert();    // Update enemy state
            }
        }

        if(other.CompareTag("Fire Monster"))
        {
            Debug.Log("Fire Monster Registered.");
            FireMonster monster = other.transform.parent.GetComponent<FireMonster>();
            fireMonsters.Add(monster);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Fire Monster"))
        {
            Debug.Log("Fire Monster Unregistered.");
            FireMonster monster = other.transform.parent.GetComponent<FireMonster>();
            fireMonsters.Remove(monster);
        }
    }

    // Increase amount of fire in room - called when monster spawns new fire in room while patrolling
    public void AddFire()
    {
        fires++;
    }
}
