using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.VisualScripting;

/// <summary>
/// FireMonster script is attached to FireMonster who spawns fire at certain intervals, 
/// and chases the player if they can see them at the field of view. 
/// ** If wanted we can draw the FOV using Unity Gizmos for debugging purposes
/// </summary>
public class FireMonster : Enemy
{
    public float moveSpeed = 3.0f;
    public float chasePathUpdateInterval = 0.25f; // Calculating new path happens 4 times per second when chasing player.
    public float waypointThreshold = 0.2f;

    private PathFinding pathFinding;
    private List<Vector3> currentPath = new List<Vector3>();    // Current path from enemy to player (Only active when player spotted)
    private int currentWaypointIndex = 0;

    private float timer;

    public Transform player;
    public float viewRadius = 12f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public LayerMask obstacleMask;         // This might have to change - currently unwalkable = obstacle mask but there could be unwalkable places where it is not an obstacle
    
    
    private bool hasSpottedPlayer = false;
    private Vector3 lastPlayerLocation = Vector3.zero;

    public Room currentRoom;              // Room that the monster is currently in

    public enum PatrolOption
    {
        Loop,   // Enemy patrols in a loop
        Line    // Enemy patrols in a line
    }

    public PatrolOption currentPatrolOption;

    private List<Vector3> patrolPoints = new List<Vector3>();
    private int currentPatrolIndex = 0;

    public enum MonsterBehavior
    {
        Patrol,     // Picks patrol points whem in a room. 
        Chase,      // Need to optimize A* for enemy
        Attack,     // Implemented but currently it is in collision state. I
                    // if that is true, this might not need to be present.
        SpawningFire
    }
    
    public MonsterBehavior currentBehavior; // Later will be used for state changes

    public GameObject firePrefab;
    public float spawnFireEveryDistance = 3.0f;   
    public float fireOffsetDistance = 1.2f;
    public float pauseAndSpawnDuration = 1.0f;  // Pause Duration
    private Vector3 lastFireSpawnPosition;
    private bool isSpawningFire = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        // Just in case the parent 3D object is misaligned with the component
        foreach (Transform child in transform)
        {
            child.localRotation = Quaternion.identity;
        }

        pathFinding = FindFirstObjectByType<PathFinding>(); 
    
        if (pathFinding == null)
        {
            Debug.LogError("No PathFinding script found in the scene!");
            return;
        }

        // Current Position and Current Behavior
        position = transform.position;
        lastFireSpawnPosition = transform.position;
        currentBehavior = MonsterBehavior.Patrol;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        timeSinceDmg += Time.deltaTime;

        // Update Monster Behavior
        bool canSee = CanSeePlayer();

        // If monster spots player and is patrolling - start chasing player
        if (canSee && currentBehavior != MonsterBehavior.Chase)
        {
            SetBehavior(MonsterBehavior.Chase);
        }
        // If monster cannot see player and was chasing - start patrolling again
        else if (!canSee && currentBehavior == MonsterBehavior.Chase)
        {
            SetBehavior(MonsterBehavior.Patrol);
        }
        

        // Execute specifics in behavior
        switch(currentBehavior)
        {
            case MonsterBehavior.Patrol:
                ProcessPatrolState();
                FollowPath();
                CheckMidTransitFireSpawn();     // Check whether it should stop and spawn fire
            break;

            case MonsterBehavior.Chase: 
                FollowPath();
            break;

            case MonsterBehavior.SpawningFire:
                // Monster does not move when spawning fires
            break;
        }
    }

    public void SetBehavior(MonsterBehavior newBehavior)
    {
        currentBehavior = newBehavior;
        CancelInvoke(nameof(UpdateChasePath));

        if(newBehavior == MonsterBehavior.Chase)
        {
            isSpawningFire = false;
            InvokeRepeating(nameof(UpdateChasePath), 0f, chasePathUpdateInterval);
        }
        else if(newBehavior == MonsterBehavior.Patrol)
        {
            if(currentRoom != null)
            {
                patrolPoints = (currentPatrolOption == PatrolOption.Loop) ?
                                currentRoom.GenerateLoopPatrolPoints(pathFinding) : currentRoom.GenerateLinePatrolPoints();
                currentPatrolIndex = 0;
                RequestPathTo(patrolPoints[currentPatrolIndex]);
            }
        }
    }

    void ProcessPatrolState()
    {
        if (isSpawningFire) return;

            // Trigger when monster has no path or has reached the end of its current path
            if (currentPath == null || currentPath.Count == 0 || currentWaypointIndex >= currentPath.Count)
            {
                // If monster knows last spotted location, try to make patrol there
                if (lastPlayerLocation != Vector3.zero)
                {
                    Debug.Log($"[FireMonster] Investigating last spotted location: {lastPlayerLocation}");

                    List<Vector3> searchPoints = (currentRoom != null)
                        ? currentRoom.GenerateSearchPointsAround(lastPlayerLocation, pathFinding)
                        : null;

                    if (searchPoints != null && searchPoints.Count > 0)
                    {
                        patrolPoints = searchPoints;
                        currentPatrolIndex = 0;
                        lastPlayerLocation = Vector3.zero; // Clear trigger so it only fires once upon arrival
                        
                        if (!RequestPathTo(patrolPoints[currentPatrolIndex]))
                        {
                            // Fallback if pathing fails
                            lastPlayerLocation = Vector3.zero;
                            patrolPoints.Clear();
                        }
                        return;
                    }
                    else
                    {
                        // Revert to room patrol
                        lastPlayerLocation = Vector3.zero;
                    }
                }

                // Room Patrol loop
                if (patrolPoints == null || patrolPoints.Count == 0)
                {
                    if (currentRoom != null)
                    {
                        patrolPoints = (currentPatrolOption == PatrolOption.Loop)
                            ? currentRoom.GenerateLoopPatrolPoints(pathFinding)
                            : currentRoom.GenerateLinePatrolPoints();

                        currentPatrolIndex = 0;
                    }
                }
                else
                {
                    // Advance to next waypoint index
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
                }

                // Request path to active patrol index
                if (patrolPoints != null && patrolPoints.Count > 0)
                {
                    bool pathSuccess = RequestPathTo(patrolPoints[currentPatrolIndex]);

                    // If pathfinding returned null, skip to next index so it doesn't stop moving
                    if (!pathSuccess)
                    {
                        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
                    }
                }
            }
    }

    /// <summary>
    /// WHEN PATROLLING - Checks distance moved and triggers a pause in movement to spawn fire
    /// </summary>
    void CheckMidTransitFireSpawn()
    {
        if(isSpawningFire || currentPath == null || currentPath.Count == 0) return;

        float distanceMoved = Vector3.Distance(transform.position, lastFireSpawnPosition);

        if(distanceMoved >= spawnFireEveryDistance)
        {
            StartCoroutine(SpawnFiresRoutine());
        }

    }

    // If we want more fires to be spawn, this can be supported
    private System.Collections.IEnumerator SpawnFiresRoutine()
    {
        isSpawningFire = true;
        currentBehavior = MonsterBehavior.SpawningFire;
        lastFireSpawnPosition = transform.position;

        // 4 locations to spawn fire
        Vector3[] offsets = new Vector3[]
        {
            new Vector3(fireOffsetDistance, 0f, 0f),   // Right
            new Vector3(-fireOffsetDistance, 0f, 0f),  // Left
            new Vector3(0f, 0f, fireOffsetDistance),   // Forward
            new Vector3(0f, 0f, -fireOffsetDistance)   // Back
        };

        // Instantiate 4 fires & register to current room
        foreach (Vector3 offset in offsets)
        {
            Vector3 spawnPos = transform.position + offset;

            if (firePrefab != null)
            {
                Instantiate(firePrefab, spawnPos, Quaternion.identity);
            }

            if (currentRoom != null)
            {
                currentRoom.AddFire();
            }
        }

        // Brief pause before moving again
        yield return new WaitForSeconds(pauseAndSpawnDuration);

        // Resume patrol if not spotted player during pause
        if (currentBehavior == MonsterBehavior.SpawningFire)
        {
            isSpawningFire = false;
            currentBehavior = MonsterBehavior.Patrol;
        }
    }

    void UpdateChasePath()
    {
        if(player != null && CanSeePlayer())
        {
            RequestPathTo(player.position);
        }
    }

    bool RequestPathTo(Vector3 targetPos)
    {
        if (pathFinding == null) return false;

        List<Node> rawPath = pathFinding.FindPath(transform.position, targetPos);

        if (rawPath == null || rawPath.Count <= 1)
        {
            return false; // Path failed or target unwalkable
        }

        currentPath.Clear();
        for (int i = 1; i < rawPath.Count; i++)
        {
            currentPath.Add(rawPath[i].worldPosition);
        }

        currentWaypointIndex = 0;
        return true; 
    }

    /// <summary>
    /// Fire Monster follows given path.
    /// </summary>
    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0 || currentWaypointIndex >= currentPath.Count)
            return;

        // Get current target waypoint
        Vector3 targetPos = currentPath[currentWaypointIndex];
        targetPos.y = transform.position.y; // Keep Y height locked to ground

        Vector3 dirToTarget = targetPos - transform.position;

        // Only rotate if we are reasonably far from the waypoint center
        if (dirToTarget.sqrMagnitude > 0.05f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget.normalized, Vector3.up);
            // Smooth out turning over time
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Move forward
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // Advance to next waypoint when within threshold distance
        if (Vector3.Distance(transform.position, targetPos) < waypointThreshold)
        {
            currentWaypointIndex++;
        }
    }

    public bool CanSeePlayer()
    {
        if (player == null) 
        {
            Debug.LogWarning("Player reference is NULL!");
            return false;
        }

        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 target = player.position + Vector3.up * 1.0f;

        Vector3 dirToPlayer = (target - origin).normalized;
        float distToPlayer = Vector3.Distance(origin, target);

        // Distance Check
        if (distToPlayer > viewRadius)
        {
            Debug.Log($"FAIL - Distance too far - Dist: {distToPlayer:F1} / Max: {viewRadius}");
            return false;
        }

        // Angle Check
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle / 2f)
        {
            Debug.Log($"FAIL - Outside cone! Angle: {angle:F1}° / Max allowed: {viewAngle / 2f}°");
            return false;
        }

        // Raycast Obstacle Check
        if (Physics.Raycast(origin, dirToPlayer, out RaycastHit hit, distToPlayer - 0.1f, obstacleMask))
        {
            Debug.Log($"FAIL - Raycast hit obstacle: '{hit.collider.name}' on Layer '{LayerMask.LayerToName(hit.collider.gameObject.layer)}'");
            return false;
        }

        Debug.Log("SUCCESS - Player spotted!");
        return true;
    }
}
