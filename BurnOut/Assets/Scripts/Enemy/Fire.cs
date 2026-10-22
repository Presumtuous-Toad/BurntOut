using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Fire is a static enemy type that spreads itself. It deals damage to player as well.
/// </summary>
public class Fire : Enemy
{
    public Room room;

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

        base.Update();
    }

    protected override void Death()
    {
        state = EnemyState.Dead;
        gameObject.SetActive(false);

        if (room != null)
        {
            room.RemoveFire();
        }
    }
}
