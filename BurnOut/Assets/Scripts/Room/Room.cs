using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.AI;

/// <summary>
/// Every room should have a Room object holding this script with the collider adjusted to its width & height
/// </summary>
public class Room : MonoBehaviour
{
    public float wallInset = 2.5f;

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
        roomTrigger = GetComponent<Collider>();
        if(roomTrigger != null)
        {
            // Retrieve bound for this room
            bounds = roomTrigger.bounds;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Enemy inside room triggered!");
            foreach (FireMonster monster in fireMonsters)
            {
                monster.SetBehavior(FireMonster.MonsterBehavior.Chase);    // Update enemy state
            }
        }

        if(other.CompareTag("Fire Monster"))
        {
            Debug.Log("Fire Monster Registered.");
            FireMonster monster = other.GetComponent<FireMonster>();
            if (monster == null) monster = other.GetComponentInParent<FireMonster>();

            if (monster != null && !fireMonsters.Contains(monster))
            {
                fireMonsters.Add(monster);
                monster.currentRoom = this;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        FireMonster monster = other.GetComponent<FireMonster>();
        if (monster == null) monster = other.GetComponentInParent<FireMonster>();

        if (monster != null && fireMonsters.Contains(monster))
        {
            fireMonsters.Remove(monster);
        }
    }

    // Increase amount of fire in room - called when monster spawns new fire in room while patrolling
    public void AddFire()
    {
        fires++;
        if(IsMaxFiresReached())
        {
            OnMaxFiresReached();
        }
    }

    public bool IsMaxFiresReached()
    {
        return fires >= MaxFires;
    }

    private void OnMaxFiresReached()
    {
        // Move Monsters to adjacent room or any other behavior
    }

    /// <summary>
    /// Returns a list of patrol points that forms a square. Allow Jitter to ensure not all enemies have the same path
    /// </summary>
    /// <returns>A list of patrol points</returns>
    public List<Vector3> GenerateLoopPatrolPoints(float offsetJitter = 0.8f)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3[] baseCorners = new Vector3[]
        {
            new Vector3(center.x - extents.x + wallInset, center.y, center.z - extents.z + wallInset),
            new Vector3(center.x - extents.x + wallInset, center.y, center.z + extents.z - wallInset),
            new Vector3(center.x + extents.x - wallInset, center.y, center.z + extents.z - wallInset),
            new Vector3(center.x + extents.x - wallInset, center.y, center.z - extents.z + wallInset)
        };

        foreach (Vector3 corner in baseCorners)
        {
            float jitterX = Random.Range(-offsetJitter, offsetJitter);
            float jitterZ = Random.Range(-offsetJitter, offsetJitter);
            points.Add(corner + new Vector3(jitterX, 0f, jitterZ));
        }

        return points;
    }

    /// <summary>
    /// Returns a list of patrol points that form a line. 
    /// </summary>
    /// <returns>A list of patrol points</returns>
    public List<Vector3> GenerateLinePatrolPoints(float offsetJitter = 0.8f)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3 p1 = new Vector3(center.x - extents.x + wallInset, center.y, center.z);
        Vector3 p2 = new Vector3(center.x + extents.x - wallInset, center.y, center.z);

        float jitterX1 = Random.Range(-offsetJitter, offsetJitter);
        float jitterZ1 = Random.Range(-offsetJitter, offsetJitter);
        float jitterX2 = Random.Range(-offsetJitter, offsetJitter);
        float jitterZ2 = Random.Range(-offsetJitter, offsetJitter);

        points.Add(p1 + new Vector3(jitterX1, 0f, jitterZ1));
        points.Add(p2 + new Vector3(jitterX2, 0f, jitterZ2));

        return points;
    }

    /// <summary>
    /// Generates 4 search points around last spotted location of player.
    /// Returns null if points are unwalkable or blocked by obstacles.
    /// </summary>
    public List<Vector3> GenerateSearchPointsAround(Vector3 centerPoint, PathFinding pathfinding, float searchRadius = 1.5f)
    {
        List<Vector3> validPoints = new List<Vector3>();

        Vector3[] candidates = new Vector3[]
        {
            centerPoint + new Vector3(searchRadius, 0f, 0f),
            centerPoint + new Vector3(-searchRadius, 0f, 0f),
            centerPoint + new Vector3(0f, 0f, searchRadius),
            centerPoint + new Vector3(0f, 0f, -searchRadius)
        };

        foreach (Vector3 pos in candidates)
        {
            // Must be inside room bounds
            if (!bounds.Contains(pos)) continue;

            // Must be walkable according to A* Grid
            if (pathfinding != null && pathfinding.grid != null)
            {
                Node node = pathfinding.grid.NodeFromWorldPoint(pos);
                if (node != null && node.walkable)
                {
                    validPoints.Add(pos);
                }
            }
        }

        // Return search points if at least 2 valid points exist; otherwise return null to trigger fallback
        return (validPoints.Count >= 2) ? validPoints : null;
    }
}
