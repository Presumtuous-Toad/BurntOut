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
    public float pathUpdateInterval = 1.0f; // Calculating new path happens 5 times per second when chasing player.
    public float waypointThreshold = 0.2f;

    private float timer;

    public float viewRadius = 12f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public LayerMask obstacleMask;         // This might have to change - currently unwalkable = obstacle mask but there could be unwalkable places where it is not an obstacle
    
    private PathFinding pathFinding;
    private List<Vector3> currentPath = new List<Vector3>();    // Current path from enemy to player (Only active when player spotted)
    private int currentWaypointIndex = 0;
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

        position = transform.position;
        InvokeRepeating(nameof(UpdatePath), 0f, pathUpdateInterval);    // Request Path updates on a timer
    }

    // Update is called once per frame
    void Update()
    {
        // Only move along path when player is spotted
        if(hasSpottedPlayer)
        {
            FollowPath();
        }
    }

    void UpdatePath()
    {
        if (player == null || pathFinding == null) return;

        if (CanSeePlayer())
        {
            hasSpottedPlayer = true;

            List<Node> rawPath = pathFinding.FindPath(transform.position, player.position);

            if (rawPath != null && rawPath.Count > 1)
            {
                currentPath.Clear();

                // Skip node 0 (the node we are currently standing on)
                for (int i = 1; i < rawPath.Count; i++)
                {
                    currentPath.Add(rawPath[i].worldPosition);
                }

                currentWaypointIndex = 0;
            }
        }
        else
        {
            // Line-of-sight broken, but let monster will smoothly finish walking to the player's last known position.
            hasSpottedPlayer = false; 
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
}
