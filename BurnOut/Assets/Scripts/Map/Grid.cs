using UnityEngine;

/// <summary>
/// Grid class holds information of the map divided by nodes. 
/// </summary>
public class Grid : MonoBehaviour
{
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    Node[,] grid;

    float nodeDiameter;
    int gridSizeX;
    int gridSizeY;

    void Start()
    {
        nodeDiameter = nodeRadius * 2;

        // Calculate grid Size (how many cells are there) 
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - (Vector3.right * gridWorldSize.x / 2) - (Vector3.forward * gridWorldSize.y / 2);

        for(int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Get current position (center) of current node
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius));     // Checks whether ANY collider overlaps with the sphere in given space
                grid[x, y] = new Node(walkable, worldPoint);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if(grid != null)
        {
            foreach (Node node in grid)
            {
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
