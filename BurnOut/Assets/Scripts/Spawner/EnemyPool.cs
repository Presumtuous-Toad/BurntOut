using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object pool structure to get and register, and retrieve enemy objects.
/// Purpose to recycle objects if possible. 
/// </summary>
public class EnemyPool 
{
    private Dictionary<GameObject, Queue<GameObject>> pool = new Dictionary<GameObject, Queue<GameObject>>();

    // ** Note - only works when different subtypes of a specific monster type are being randomly spawned not controled 
    public GameObject Get(GameObject prefab)
    {
        // Is there a queue for this enemy type
        if(!pool.ContainsKey(prefab))
        {
            pool[prefab] = new Queue<GameObject>();
        }

        // If there is a saved instance
        if(pool[prefab].Count > 0)
        {
            GameObject obj = pool[prefab].Dequeue();

            obj.SetActive(true);

            return obj;
        }
        else
        {
            return GameObject.Instantiate(prefab);
        }
    }

    
    public void Return(GameObject prefab, GameObject obj)
    {
        pool[prefab].Enqueue(obj);
        if(obj != null)
            obj.SetActive(false);       // Set inactive when object returned
    }
}
