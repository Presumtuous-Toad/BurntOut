using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// FireMonster script is attached to FireMonster who spawns fire at certain intervals, 
/// and chases the player if they can see them at the field of view. 
/// ** If wanted we can draw the FOV using Unity Gizmos for debugging purposes
/// </summary>
public class FireMonster : Enemy
{
    public Transform player;
    public float moveSpeed = 3.0f;
    public float pathUpdateInterval = 0.2f; // Calculating new path happens 5 times per second when chasing player.
    public float wayPointThreshold = 0.2f;

    private float timer;

    public float viewRadius = 12f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public LayerMask obstacleMask;         // This might have to change - currently unwalkable = obstacle mask but there could be unwalkable places where it is not an obstacle
    
    private PathFinding pathFinding;
    private List<Vector3> currentPath = new List<Vector3>();    // Current path from enemy to player (Only active when player spotted)
    private int currentWayPointIndex = 0;
    private bool hasSpottedPlayer = false;

    public enum MonsterBehavior
    {
        Patrol,     // Not Implemented
        Chase,      // Need to optimize A* for enemy
        Attack      // Not implemented
    }
    
    public MonsterBehavior currentBehavior; // Later will be used for state changes

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pathFinding = FindFirstObjectByType<PathFinding>(); 
    
        if (pathFinding == null)
        {
            Debug.LogError("No PathFinding script found in the scene!");
            return;
        }

        position = transform.position;
        InvokeRepeating(nameof(UpdatePath), 0f, pathUpdateInterval);    // Request Path updates on a timer
    }

    // Update is called once per frame
    void Update()
    {
        // If invoke repeating does not work
        // timer += Time.deltaTime;
        // if (timer >= pathUpdateInterval)
        // {
        //     timer = 0f;
        //     UpdatePath();
        // }

        // Only move along path when player is spotted
        if(hasSpottedPlayer)
        {
            FollowPath();
        }
    }

    void UpdatePath()
    {
        if(player == null || pathFinding == null || pathFinding.grid == null) return;

        // Only request path finding when player is in line of sight
        if(CanSeePlayer()) {
            Debug.Log("Player in sight!");
            hasSpottedPlayer = true;

            // Compute path to player
            List<Node> rawPath = pathFinding.FindPath(position, player.position);

            if(rawPath != null && rawPath.Count > 0)
            {
                Debug.Log($"Path found! Nodes in path: {rawPath.Count}");
                currentPath.Clear();
                foreach(Node node in rawPath)
                {
                    currentPath.Add(node.worldPosition);
                }
                currentWayPointIndex = 0;
            }
        }
        else
        {
            // Player is not in sight - clear existing path and switch to patrolling LATER (logic change might be needed)
            hasSpottedPlayer = false;
            currentPath.Clear();
            Debug.Log("Player not in sight.");
        }
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.forward * 1.0f;
        Vector3 target = player.position + Vector3.forward * 1.0f;

        Vector3 dirToPlayer = (target - origin).normalized;
        float distToPlayer = Vector3.Distance(origin, target);

        if (distToPlayer <= viewRadius)
        {
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            
            if (angle < viewAngle / 2f)
            {
                bool hitObstacle = Physics.Raycast(origin, dirToPlayer, distToPlayer - 0.1f, obstacleMask);
                
                // Visual Debug in Scene View
                Debug.DrawLine(origin, target, hitObstacle ? Color.red : Color.green, 0.1f);

                if (!hitObstacle)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void FollowPath()
    {
        if(currentPath == null || currentPath.Count == 0 || currentWayPointIndex >= currentPath.Count) 
            return;

        Vector3 targetWayPoint = currentPath[currentWayPointIndex];
        targetWayPoint.y = transform.position.y;                    // Monster should not fly

        // Move toward way point
        transform.position = Vector3.MoveTowards(transform.position, targetWayPoint, moveSpeed * Time.deltaTime);

        // Rotate toward movement direction
        Vector3 direction = (targetWayPoint - transform.position).normalized;
        if(direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // When close enough to next way point, advance index
        if(Vector3.Distance(transform.position, targetWayPoint) < wayPointThreshold)
        {
            currentWayPointIndex++;
        }
    }
}
