using UnityEngine;

/// <summary>
/// IMoveable Interface is applied to entities that implements their own move function.
/// </summary>
public interface IMoveable
{
    void Move(float x, float y, float z);
}
