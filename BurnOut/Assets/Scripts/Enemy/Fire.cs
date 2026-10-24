using UnityEngine;

/// <summary>
/// Fire is a static enemy type that spreads itself. It deals damage to player as well.
/// </summary>
public class Fire : Enemy
{
    public Room room;

    public GameObject FirePrefab;
    public int MaxChildFires = 6;
    public float SpawnRate = 6f;
    public float SpawnRaidus = 3f;
    public float SafetyRaius = 3f;
    public LayerMask layerMask;

    private float timeElapsed;
    private int childFires;
    private const int MAX_ATTEMPTS = 10;
    private bool stopSapwning = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        room = GetComponentInParent<Room>();

        // Initialize position of itself. In case of fire it will never be updated again.
        position = transform.position;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if(!stopSapwning)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= SpawnRate)
            {
                timeElapsed = 0;
                SpawnFire();
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
        int attempts = 0;
        while(attempts < MAX_ATTEMPTS)
        {
            attempts++;

            Vector3 spawnPos = Random.insideUnitSphere * SpawnRaidus;
            spawnPos.y = 0;

            if (Physics.CheckSphere(spawnPos, SafetyRaius, layerMask))
            {
                // spawn fire and end early
                if (room != null) Instantiate(FirePrefab, spawnPos, Quaternion.identity, room.transform);
                room.AddFire();
                childFires++;
                if (childFires == MaxChildFires) stopSapwning = true;
                return;     
            }
        }
        // could not place fire, stops spawning in the future
        stopSapwning = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, SafetyRaius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.transform.position, SpawnRaidus);
    }
}
