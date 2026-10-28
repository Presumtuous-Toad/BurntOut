using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.AI;

// Struct to store room patrol points with name 
[System.Serializable]
public struct PresetPatrolData
{
    public int ID;  // This will be given to monster when in use and returned when not in use.

    [Header("One set of preset points should have at least 2 elements.")]
    public GameObject[] presetPoints;

    public bool beingUsed;      // Default is not being used but when a monster claims this patrol data, it checks as true, ensuring monsters don't choose the same patrol path
}

/// <summary>
/// Every room should have a Room object holding this script with the collider adjusted to its width & height
/// </summary>
public class Room : MonoBehaviour
{
    [System.Serializable]
    public enum PatrolPoints
    {
        Generate,
        Preset
    }

    public PatrolPoints option;

    public List<PresetPatrolData> presetPatrolChoices;

    public float wallInset = 2.5f;
    public RoomManager roomManager;

    // Make sure 'this' room's trigger collider is assigned
    [Header("Collider MUST be TRIGGER collider.")]
    public List<Collider> roomColliders = new List<Collider>();

    private Bounds bounds;

    // NOT ADDED YET
    // The enemy view radius should not exceed this. 
    // (TALK ABOUT THIS WITH TEAM) - Is restricting maximum view distance per room good or do we want it to be unique components of enemy?
    public float MaximumViewDistance { get; private set; } 

    [Header("PREPLACED monster before runtime should be added to this list for registeration.")]
    public List<FireMonster> fireMonsters;     

    // Event that is triggered when monster enters room - might not be needed

    // Number of fire instances in each room. 
    public int fires;
    public int MaxFires;   // Depends on room size. If number of fire reaches the amount of max fire, add behavior to move monster to another room.

    public bool Accessibility;

    // TODO: Event to trigger Maximum Fire --> Room inaccessible and alert monster to go out if they are inside (Drive them oustide)
    // Room layer -> Unwalkable | Room Not accessible
    public delegate void WhenRoomInaccessible();
    public event WhenRoomInaccessible RoomInaccessible;

    // TODO: Event to trigger when amount of fire is under maximum fire --> Room accessible again 
    public delegate void WhenRoomAccessible();
    public event WhenRoomAccessible  RoomAccessible;

    void Awake()
    {
        // Retrieve ALL colliders attached to this GameObject and its children
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliders)
        {
            if (collider.isTrigger) // Ensure we only collect trigger colliders
            {
                roomColliders.Add(collider);
            }
        }

        if (roomColliders.Count > 0)
        {
            // Start bounds with the first collider
            bounds = roomColliders[0].bounds;

            // Encapsulate all additional colliders into one master bounding box
            for (int i = 1; i < roomColliders.Count; i++)
            {
                bounds.Encapsulate(roomColliders[i].bounds);
            }
        }
        else
        {
            Debug.LogError($"{gameObject.name} has no trigger colliders attached!");
        }

        // Subscribe to events
        RoomAccessible += RoomAccessibilityUpdate;
        RoomInaccessible += RoomAccessibilityUpdate;
    }

    void Start()
    {
        // For monsters pre-registered, give them room reference
        if(fireMonsters.Count > 0)
        {
            foreach(FireMonster monster in fireMonsters)
            {
                monster.currentRoom = this;
            }
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
                                                                           // Give Enemy player position and make them turn towards(look towards) player 
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
        roomManager.AddTotalFire();

        if (IsMaxFiresReached())
        {
            RoomInaccessible.Invoke();  // Method to make and subscribe - monster chooses the closest room without fire and moves towards the center of that room(pathfinding)
        }
    }

    public void RemoveFire()
    {
        if (fires > 0)
        {
            fires--;
            roomManager.RemoveTotalFire();
        }

        if(!IsMaxFiresReached())
        {
            RoomAccessible.Invoke();
        }
    }

    public bool IsMaxFiresReached()
    {
        return fires >= MaxFires;
    }

    private void RoomAccessibilityUpdate()
    {
        Accessibility = !Accessibility;
    }

    /// <summary>
    /// Sets monster's patrol point list to a list of preset patrol points if it is not being used. 
    //  Assumes each given patrol point set will contain more than 1 patrol points.
    /// </summary>
    public void ReservePresetPatrolPoints(FireMonster monster)
    {
        PresetPatrolData data;

        // Choose first option that is not being used
        for(int i = 0; i < presetPatrolChoices.Count; i++)
        {
            if(!presetPatrolChoices[i].beingUsed)
            {
                data = presetPatrolChoices[i];
                data.beingUsed = true;
                presetPatrolChoices[i] = data;
                continue;
            }
        }

        // Assign patrol data ID to monster
        monster.patrolOptionID = data.ID;

        // Populate List of vector to return
        List<Vector3> result = new List<Vector3>();

        if(data.presetPoints != null && data.presetPoints.Length > 1)
        {
            for(int i = 0; i < data.presetPoints.Length; i++)
            {
                result.Add(data.presetPoints[i].transform.position);
            }
        }
        
        // Give found data to monster
        monster.patrolPoints = result;
    }


    /// <summary>
    /// Returns a list of patrol points that forms a square. Allow Jitter to ensure not all enemies have the same path
    /// </summary>
    /// <returns>A list of patrol points</returns>
    public List<Vector3> GenerateLoopPatrolPoints(PathFinding pathFinding, float offsetJitter = 0.8f)
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
            Vector3 candidate = corner + new Vector3(jitterX, 0f, jitterZ);

            // Validate point against A* grid node walkability
            if (pathFinding != null && pathFinding.grid != null)
            {
                Node node = pathFinding.grid.NodeFromWorldPoint(candidate);
                if (node != null && node.walkable)
                {
                    points.Add(node.worldPosition); // Use actual walkable node position
                }
            }
            else
            {
                points.Add(candidate);
            }
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
