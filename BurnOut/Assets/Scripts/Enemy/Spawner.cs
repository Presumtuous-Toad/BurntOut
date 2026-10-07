using UnityEngine;

/// <summary>
/// Spawner class is attached to spawners that will spawn Fire Monsters in certain intervals.
/// Intervals should be controlled to balance difficulty
/// </summary>
public class Spawner : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        position = transform.position;
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}
