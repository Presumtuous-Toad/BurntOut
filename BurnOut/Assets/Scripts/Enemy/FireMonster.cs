using UnityEngine;

/// <summary>
/// FireMonster script is attached to FireMonster who spawns fire at certain intervals, 
/// and chases the player if they can see them at the field of view. Player Tag is required for it to actually pursue what they see.
/// </summary>
public class FireMonster : Enemy, IMoveable
{

        

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(float x, float y, float z)
    {

    }
}
