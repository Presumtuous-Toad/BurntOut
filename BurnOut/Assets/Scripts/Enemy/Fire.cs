using UnityEngine;

/// <summary>
/// Fire is a static enemy type that spreads itself
/// </summary>
public class Fire : Enemy
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize position of itself. In case of fire it will never be updated again.
        position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
