using UnityEngine;
using System.Collections.Generic;

public enum EnemyType
{
    FireMonster

}

[System.Serializable]
public struct SpawnTimer
{
    public float spawnInterval;         // How much time since last spawn should this enemy type be spawned? 

    private float timer;                //  Personal timer to track time and know when it should be spawned
}

[System.Serializable]
public struct SpawnData
{
    public EnemyType type;              // Enemy type to spawn

    public List<GameObject> prefabs;    // If this enemy has various types instead of one type

    public int maxCount;                // In this area/room, how many enemy should spawn? If max count reached, enemy will not be spawned.

    public SpawnTimer spawnTimer;
}

/// <summary>
/// Spawner script will be attached to spawner object. One spawner object can support different enemy types
/// </summary>
public class Spawner : Enemy
{
    [Header("Make sure enemy spawn data are given to this specific spawner in INSPECTOR")]
    public List<SpawnData> enemySpawnData;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        position = transform.position;      // Assign spawner position on start
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}
