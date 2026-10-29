using UnityEngine;
using System.Collections.Generic;

public class FireMonster : Enemy
{
    [Header("Movement")]
    public float moveSpeed = 3.0f;
    public float chasePathUpdateInterval = 0.25f;
    public float waypointThreshold = 0.4f;

    private Rigidbody rb;
    private PathFinding pathFinding;
    private List<Vector3> currentPath = new List<Vector3>();
    private int currentWaypointIndex = 0;

    public Transform player;
    public float viewRadius = 12f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask obstacleMask;

    private Vector3 lastPlayerLocation = Vector3.zero;

    [Header("Room & Patrol")]
    public Room currentRoom;
    public int patrolOptionID = -1;
    public List<Vector3> patrolPoints = new List<Vector3>();
    private int currentPatrolIndex = 0;

    public enum MonsterBehavior
    {
        Patrol,
        Chase,
        ChangingRoom
    }

    public MonsterBehavior currentBehavior;

    [Header("Fire Spawning")]
    public GameObject firePrefab;
    public float spawnFireEveryDistance = 2.5f; // Drop 1 fire every X units
    private Vector3 lastFireSpawnPosition;

    [Header("Patrol Pause Settings")]
    public float loopPauseDuration = 2.0f; // Pause duration after completing 1 full loop
    private float pauseTimer = 0f;
    private bool isPausingBetweenLoops = false;

    protected override void Start()
    {
        base.Start();

        foreach (Transform child in transform)
        {
            child.localRotation = Quaternion.identity;
        }

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        else
        {
            Debug.LogError($"[FireMonster] {gameObject.name} is missing a Rigidbody component!");
        }

        pathFinding = FindFirstObjectByType<PathFinding>();
        lastFireSpawnPosition = transform.position;

        SetBehavior(MonsterBehavior.Patrol);
    }

    protected override void Update()
    {
        base.Update();
        timeSinceDmg += Time.deltaTime;

        bool canSee = CanSeePlayer();

        if (canSee && currentBehavior != MonsterBehavior.Chase)
        {
            SetBehavior(MonsterBehavior.Chase);
        }
        else if (!canSee && currentBehavior == MonsterBehavior.Chase)
        {
            SetBehavior(MonsterBehavior.Patrol);
        }

        switch (currentBehavior)
        {
            case MonsterBehavior.Patrol:
                ProcessPatrolState();
                break;

            case MonsterBehavior.ChangingRoom:
                CheckRoomTransitionComplete();
                break;
        }

        // SPAWN FIRE TRAIL IN ALL STATES (Patrol, Chase, ChangingRoom)
        CheckTrailFireSpawn();
    }

    void FixedUpdate()
    {
        if (currentBehavior == MonsterBehavior.Patrol || currentBehavior == MonsterBehavior.Chase || currentBehavior == MonsterBehavior.ChangingRoom)
        {
            FollowPath();
        }
    }

    public void SetBehavior(MonsterBehavior newBehavior)
    {
        currentBehavior = newBehavior;
        CancelInvoke(nameof(UpdateChasePath));

        if (newBehavior == MonsterBehavior.Chase)
        {
            InvokeRepeating(nameof(UpdateChasePath), 0f, chasePathUpdateInterval);
        }
        else if (newBehavior == MonsterBehavior.Patrol)
        {
            InitializePatrolRoute();
        }
    }

    private void InitializePatrolRoute()
    {
        if (currentRoom == null)
        {
            Debug.LogError($"[FireMonster] {gameObject.name} cannot initialize patrol: currentRoom is NULL!");
            return;
        }

        // Unsubscribe first to prevent duplicate event subscriptions
        currentRoom.RoomInaccessible -= HandleRoomInaccessible;
        currentRoom.RoomInaccessible += HandleRoomInaccessible;

        // Clear existing route if invalid
        if (patrolPoints == null) patrolPoints = new List<Vector3>();

        if (patrolPoints.Count == 0)
        {
            if (currentRoom.option == Room.PatrolPoints.Preset)
            {
                bool reserved = currentRoom.ReservePresetPatrolPoints(this);

                // If room preset reservation failed (full or unassigned), evacuate or generate grid loop fallback
                if (!reserved)
                {
                    Debug.LogWarning($"[FireMonster] {gameObject.name} could not reserve preset in {currentRoom.gameObject.name}. Falling back to generated grid loop.");
                    patrolPoints = currentRoom.GenerateLoopPatrolPoints(pathFinding);
                    patrolOptionID = -1;
                }
            }
            else
            {
                patrolPoints = currentRoom.GenerateLoopPatrolPoints(pathFinding);
                patrolOptionID = -1;
            }
        }

        currentPatrolIndex = 0;

        // FORCE initial movement toward point 0
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            bool success = RequestPathTo(patrolPoints[0]);
            if (!success)
            {
                Debug.LogWarning($"[FireMonster] {gameObject.name} failed initial pathing to patrol point 0. Trying point 1...");
                ProcessPatrolState(); // Step through to next valid waypoint
            }
        }
    }

    private void HandleRoomInaccessible()
    {
        Debug.Log($"[FireMonster] {gameObject.name} received RoomInaccessible event from {currentRoom.gameObject.name}!");

        // Chase mode takes precedence over evacuation
        if (currentBehavior == MonsterBehavior.Chase) return;

        EvacuateToNextRoom();
    }

    public void EvacuateToNextRoom()
    {
        if (currentRoom == null) return;

        if (currentRoom.roomManager == null)
        {
            Debug.LogError($"[FireMonster] {gameObject.name} cannot evacuate: RoomManager reference on {currentRoom.gameObject.name} is missing!");
            return;
        }

        Room targetRoom = currentRoom.roomManager.GetClosestAccessibleRoom(transform.position, currentRoom);

        if (targetRoom != null)
        {
            Debug.Log($"[FireMonster] {gameObject.name} evacuating from {currentRoom.gameObject.name} to {targetRoom.gameObject.name}.");

            // Release preset in old room
            if (patrolOptionID != -1)
            {
                currentRoom.ReleasePresetPatrolPoints(patrolOptionID);
                patrolOptionID = -1;
            }

            currentRoom.RoomInaccessible -= HandleRoomInaccessible;
            patrolPoints.Clear();

            currentBehavior = MonsterBehavior.ChangingRoom;

            // Path directly to the center of the target room's bounds
            Vector3 targetCenter = targetRoom.GetComponent<Collider>().bounds.center;
            targetCenter.y = transform.position.y; // Ensure horizontal plane alignment

            bool pathSuccess = RequestPathTo(targetCenter);
            if (!pathSuccess)
            {
                Debug.LogError($"[FireMonster] {gameObject.name} failed to find path to evac room center {targetCenter}!");
            }
        }
        else
        {
            Debug.LogWarning($"[FireMonster] {gameObject.name} could not find any accessible target room to evacuate to!");
        }
    }

    void CheckRoomTransitionComplete()
    {
        // Once monster arrives at target room center, assign new room and resume patrol
        if (currentPath == null || currentWaypointIndex >= currentPath.Count)
        {
            // Re-detect room standing inside
            Collider[] hits = Physics.OverlapSphere(transform.position, 1.0f);
            foreach (var hit in hits)
            {
                Room newRoom = hit.GetComponent<Room>();
                if (newRoom == null) newRoom = hit.GetComponentInParent<Room>();

                if (newRoom != null && newRoom != currentRoom)
                {
                    currentRoom = newRoom;
                    if (!currentRoom.fireMonsters.Contains(this))
                    {
                        currentRoom.fireMonsters.Add(this);
                    }
                    break;
                }
            }

            SetBehavior(MonsterBehavior.Patrol);
        }
    }

    void ProcessPatrolState()
    {
        if (isPausingBetweenLoops)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPausingBetweenLoops = false;
                RequestPathTo(patrolPoints[currentPatrolIndex]);
            }
            return;
        }

        if (currentPath == null || currentPath.Count == 0 || currentWaypointIndex >= currentPath.Count)
        {
            // If last player location known generate patrol points around there - this might be taken out.
            if (lastPlayerLocation != Vector3.zero)
            {
                List<Vector3> searchPoints = (currentRoom != null)
                    ? currentRoom.GenerateSearchPointsAround(lastPlayerLocation, pathFinding)
                    : null;

                lastPlayerLocation = Vector3.zero;

                if (searchPoints != null && searchPoints.Count > 0)
                {
                    patrolPoints = searchPoints;
                    currentPatrolIndex = 0;
                    RequestPathTo(patrolPoints[0]);
                    return;
                }
            }

            if (patrolPoints != null && patrolPoints.Count > 0)
            {
                // If we are at the end of the list, wrap around with pause
                int nextIndex = (currentPatrolIndex + 1) % patrolPoints.Count;

                if (nextIndex == 0 && currentPath != null && currentPath.Count > 0)
                {
                    currentPatrolIndex = 0;
                    isPausingBetweenLoops = true;
                    pauseTimer = loopPauseDuration;
                    currentPath.Clear();
                    return;
                }

                currentPatrolIndex = nextIndex;
                bool success = RequestPathTo(patrolPoints[currentPatrolIndex]);

                // If this point failed pathing (e.g. Y offset or wall snag), 
                // skip it immediately so the monster doesn't get stuck forever!
                int safetyCounter = 0;
                while (!success && safetyCounter < patrolPoints.Count)
                {
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
                    success = RequestPathTo(patrolPoints[currentPatrolIndex]);
                    safetyCounter++;
                }
            }
        }
    }

    /// <summary>
    /// Spawns fire trail continuously during movement regardless of active state
    /// </summary>
    void CheckTrailFireSpawn()
    {
        if (currentRoom == null || firePrefab == null) return;

        // Stop dropping fires if current room is at capacity
        if (currentRoom.IsMaxFiresReached()) return;

        // Must be actively moving along a path
        if (currentPath == null || currentPath.Count == 0 || currentWaypointIndex >= currentPath.Count) return;

        float distanceMoved = Vector3.Distance(transform.position, lastFireSpawnPosition);

        if (distanceMoved >= spawnFireEveryDistance)
        {
            lastFireSpawnPosition = transform.position;

            Instantiate(firePrefab, transform.position, Quaternion.identity, currentRoom.transform);
            currentRoom.AddFire();
        }
    }

    void UpdateChasePath()
    {
        if (player != null && CanSeePlayer())
        {
            lastPlayerLocation = player.position;
            RequestPathTo(player.position);
        }
    }

    bool RequestPathTo(Vector3 targetPos)
    {
        if (pathFinding == null || pathFinding.grid == null) return false;

        // Get nodes for start and target
        Node startNode = pathFinding.grid.NodeFromWorldPoint(transform.position);
        Node targetNode = pathFinding.grid.NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null)
        {
            Debug.LogWarning($"[FireMonster] Out of grid bounds! Start: {startNode != null}, Target: {targetNode != null}");
            return false;
        }

        // If start node is unwalkable (e.g. monster spawned inside obstacle layer), find nearest walkable node
        Vector3 validStartPos = transform.position;
        if (!startNode.walkable)
        {
            Debug.LogWarning($"[FireMonster] Start position {transform.position} is UNWALKABLE! Finding nearest walkable node...");
            Node nearestStart = GetNearestWalkableNode(startNode);
            if (nearestStart != null) validStartPos = nearestStart.worldPosition;
            else return false;
        }

        Vector3 validTargetPos = targetPos;
        if (!targetNode.walkable)
        {
            Node nearestTarget = GetNearestWalkableNode(targetNode);
            if (nearestTarget != null) validTargetPos = nearestTarget.worldPosition;
            else return false;
        }

        // 3. Find path using validated positions
        List<Node> rawPath = pathFinding.FindPath(validStartPos, validTargetPos);

        if (rawPath == null || rawPath.Count <= 1)
        {
            return false;
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
    /// Helper to find the closest walkable node if a start/target point lands inside an obstacle.
    /// </summary>
    private Node GetNearestWalkableNode(Node originNode)
    {
        if (pathFinding == null || pathFinding.grid == null) return null;

        int searchRadius = 3; // Check up to 3 nodes outward
        Node closestNode = null;
        float minDistance = float.MaxValue;

        for (int x = -searchRadius; x <= searchRadius; x++)
        {
            for (int z = -searchRadius; z <= searchRadius; z++)
            {
                int checkX = originNode.gridX + x;
                int checkZ = originNode.gridY + z;

                if (checkX >= 0 && checkX < pathFinding.grid.gridSizeX && checkZ >= 0 && checkZ < pathFinding.grid.gridSizeY)
                {
                    Node neighbor = pathFinding.grid.grid[checkX, checkZ];
                    if (neighbor.walkable)
                    {
                        float dist = Vector3.Distance(originNode.worldPosition, neighbor.worldPosition);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            closestNode = neighbor;
                        }
                    }
                }
            }
        }

        return closestNode;
    }


    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0 || currentWaypointIndex >= currentPath.Count)
        return;

        // Force target waypoint to align perfectly with Rigidbody height
        Vector3 targetPos = currentPath[currentWaypointIndex];
        targetPos.y = rb.position.y;

        Vector3 currentPos = rb.position;
        currentPos.y = targetPos.y; // Flatten horizontal plane

        Vector3 dirToTarget = (targetPos - currentPos).normalized;

        // Smooth Rotation
        if (dirToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f));
        }

        // Move Position horizontally
        Vector3 nextPos = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPos);

        // Horizontal Distance Check (Ignore vertical height variations)
        float horizontalDistance = Vector3.Distance(
            new Vector3(rb.position.x, 0f, rb.position.z),
            new Vector3(targetPos.x, 0f, targetPos.z)
        );

        if (horizontalDistance < waypointThreshold)
        {
            currentWaypointIndex++;
        }
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 target = player.position + Vector3.up * 1.0f;

        Vector3 dirToPlayer = (target - origin).normalized;
        float distToPlayer = Vector3.Distance(origin, target);

        if (distToPlayer > viewRadius) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle / 2f) return false;

        if (Physics.Raycast(origin, dirToPlayer, distToPlayer - 0.1f, obstacleMask)) return false;

        return true;
    }
}