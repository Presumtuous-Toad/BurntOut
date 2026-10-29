using UnityEngine;

/// <summary>
/// Fire is a static enemy type that spreads itself. It deals damage to player as well.
/// </summary>
public class Fire : Enemy
{
    public Room room;

    public GameObject FirePrefab;
    public int MaxChildFires = 6;
    public float MaxSpawnRate = 6f;
    public float MinSpawnRate = 4f;
    public float SpawnRaidus = 3f;
    public float SafetyRaius = 3f;
    public LayerMask layerMask;

    private float timeElapsed;
    private float spawnRate;
    private int childFires;
    private const int MAX_ATTEMPTS = 10;
    private bool stopSapwning = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        room = GetComponentInParent<Room>();

        SetSpawnRate();

        // Initialize position of itself. In case of fire it will never be updated again.
        position = transform.position;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (room != null)
        {
            if (!stopSapwning && !room.IsMaxFiresReached())
            {
                timeElapsed += Time.deltaTime;
                if (timeElapsed >= spawnRate)
                {
                    timeElapsed = 0;
                    SetSpawnRate();
                    SpawnFire();
                }
            }
        }

        base.Update();
    }

    protected override void Death()
    {
        state = EnemyState.Dead;
        gameObject.SetActive(false);
        stopSapwning = true;

        if (room != null)
        {
            room.RemoveFire();
        }
    }

    private void SpawnFire()
    {
        Debug.Log("attempting to spawn fire");
        int attempts = 0;
        while(attempts < MAX_ATTEMPTS)
        {
            attempts++;

            Vector3 spawnPos = Random.insideUnitSphere * SpawnRaidus + transform.localToWorldMatrix.GetPosition();
            spawnPos.y = 0;
            Debug.Log("Fire Spawn Position: " +  spawnPos);
            if (!Physics.CheckSphere(spawnPos, SafetyRaius, layerMask))
            {
                Debug.Log("Fire Spawn Position: " +  spawnPos + " , Room: " + room.name);
                // spawn fire and end early
                Instantiate(FirePrefab, spawnPos, Quaternion.identity, room.transform);
                room.AddFire();
                childFires++;
                if (childFires == MaxChildFires) stopSapwning = true;
                return;     
            }
        }
        // could not place fire, stops spawning in the future
        stopSapwning = true;
    }

    private void SetSpawnRate()
    {
        spawnRate = Random.Range(MinSpawnRate, MaxSpawnRate);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, SafetyRaius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.transform.position, SpawnRaidus);
    }
}
