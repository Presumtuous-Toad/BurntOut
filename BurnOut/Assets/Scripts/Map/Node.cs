using UnityEngine;

/// <summary>
/// Node class is one cell in given map. It manages its own area.
/// </summary>
public class Node 
{
    public bool walkable;
    public Vector3 worldPosition;

    public Node(bool _walkable, Vector3 _worldPos)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
    }
}
