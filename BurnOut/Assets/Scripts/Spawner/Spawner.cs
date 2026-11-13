using UnityEngine;
using System.Collections.Generic;

public enum EnemyType
{
    FireMonster

}

[System.Serializable]
public struct SpawnTimer
{

    [Header("Only set spawn interval: time between last spawn. Rest of it is used when spawning, so DO NOT MODIFY VALUE")]
    public float spawnInterval;         // How much time since last spawn should this enemy type be spawned? 

    public float timer;                // Personal timer to track time and know when it should be spawned

    public float nextInterval;         // Next interval when enemy will be spawned
}

[System.Serializable]
public class SpawnData
{
    public EnemyType type;              // Enemy type to spawn

    public List<GameObject> prefabs;    // If this enemy has various types instead of one type

    public int activeCount;  

    public int maxCount;                // In this area/room, how many enemy should spawn? If max count reached, enemy will not be spawned.

    public SpawnTimer spawnTimer;
}

/// <summary>
/// Spawner script will be attached to spawner object.
//  One spawner object can support different enemy types (this does not refer to subtypes)
//  *If we want to spawn multiple enemy at once, that can be supported as well
/// </summary>
public class Spawner : Enemy
{
    [Header("Make sure enemy spawn data are given to this specific spawner in INSPECTOR")]
    public List<SpawnData> enemySpawnData;

    private EnemyPool enemyPool;        

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        position = transform.position;      // Assign spawner position on start

        enemyPool = new EnemyPool();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // Check whether interval has been reached and new enemy of this type should be spawned again
        foreach(SpawnData data in enemySpawnData)
        {
            data.spawnTimer.timer += Time.fixedDeltaTime;

            if(data.spawnTimer.timer < data.spawnTimer.nextInterval)
            {
                continue;
            }

            TrySpawn(data);
            data.spawnTimer.timer = 0f;
            data.spawnTimer.nextInterval += data.spawnTimer.spawnInterval;
        }
    }

    private void TrySpawn(SpawnData data)
    {
        // If there are enough, don't spawn
        if(data.activeCount >= data.maxCount)
        {
            return;
        }

        GameObject prefab; // If this enemy type has multiple subtypes, it needs a method to choose a prefab randomly

        if(data.type == EnemyType.FireMonster)
        {
            prefab = data.prefabs[0];   // Really JUST FOR NOW :) Since there is only one subtype
        }
        else
        {
            prefab = data.prefabs[0];   // Place holder for no error
        }

        // Instantiate enemy using pool - recycle if there are enemies not being used
        GameObject obj = enemyPool.Get(prefab);

        // JUST FOR NOW - we can have functions to create spawn points dependent on different types of enemies
        Vector3 temp = transform.position;
        temp.x += 1.0f;
        temp.y = 0.37f;
        temp.z -= 1.0f;
        obj.transform.position = temp;

        IDespawnNotifier notifier;

        notifier = obj.GetComponent<IDespawnNotifier>();

        if(notifier != null)
        {
            void OnDespawnHandler()
            {
                data.activeCount--;
                enemyPool.Return(prefab, obj);
                notifier.OnDespawn -= OnDespawnHandler;
            };

            notifier.OnDespawn += OnDespawnHandler;
        }
        else
        {
            Debug.Log($"{obj.name} does not have notifier.");
        }

        data.activeCount++; // Increase active count
    }
}


