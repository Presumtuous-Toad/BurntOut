using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Room Manager holds reference of all the rooms. This wil be the central system that identifies fires and deal damage to the house
/// </summary>
public class RoomManager : MonoBehaviour
{
    private List<Room> rooms;
    private HouseHealth health;

    public int damagePreFire = 1;
    public float damageTickRate = 5f;
    private float currentTime;
    public int totalFire;

    private void Awake()
    {
        rooms = FindObjectsByType<Room>(FindObjectsSortMode.None).ToList();
        foreach(Room room in rooms)
        {
            room.roomManager = this;
        }

        health = FindFirstObjectByType<HouseHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if(currentTime >= damageTickRate)
        {
            currentTime = 0;
            health.TakeDamage(damagePreFire * totalFire);
        }
    }

    public void RemoveTotalFire()
    {
        totalFire--;
    }

    public void AddTotalFire()
    {
        totalFire++;
    }

    /// <summary>
    /// Finds the closest accessible room excluding the current room.
    /// </summary>
    public Room GetClosestAccessibleRoom(Vector3 searchOrigin, Room currentRoom)
    {
        Room closestRoom = null;
        float minDistance = float.MaxValue;

        foreach (Room room in rooms)
        {
            if (room == currentRoom) continue;

            // Must be marked accessible AND not at max fire capacity
            if (room.Accessibility && !room.IsMaxFiresReached())
            {
                float dist = Vector3.Distance(searchOrigin, room.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestRoom = room;
                }
            }
        }

        return closestRoom;
    }
}
