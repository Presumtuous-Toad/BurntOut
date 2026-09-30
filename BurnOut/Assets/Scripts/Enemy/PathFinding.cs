using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements A* to find the closest path from starting position to target position
/// </summary>
public class PathFinding : MonoBehaviour
{
    public Transform seeker;
    public Transform target;

    public Grid grid;

    void Awake()
    {
        // This will not work bc there is a overlapping unity component
        // grid = GetComponent<Grid>();
    }

    void Update()
    {
        // This was for testing
        // FindPath(seeker.position, target.position);
    }

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        if(grid == null) return null;

        // ------------------------------------------------
        // TEMPORARY
        // Since in current implementations, each monster shares the same nodes, 
        // the shared resources should be reset manually when the function is called
        // This will be an issue if the process is run over multiple frames or if there are more than 20 enemies.
        // ------------------------------------------------
        grid.ResetGridNodes();

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        // Don't search if either start or end is invalid/unwalkable
        if (startNode == null || targetNode == null || !startNode.walkable || !targetNode.walkable)
            return null;

        List<Node> openSet = new List<Node>();          // The set of nodes to be evaluated
        HashSet<Node> closedSet = new HashSet<Node>();  // The set of nodes already evaluated
        openSet.Add(startNode);

        // While there are still nodes to be evaluated
        while(openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for(int i = 1; i < openSet.Count; i++)
            {
                if(openSet[i].fCost < currentNode.fCost || 
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if(currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);  // Path has been found
            }
                
            foreach (Node neighbor in grid.GetNeighbors(currentNode))
            {
                // If neighbor is not traversable or neighbor is in closed set, skip to next neighbor
                if(!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newMovementCostToNeighbor= currentNode.gCost + GetDistance(currentNode, neighbor);
               
               // If new path to neighbor is shorter or neighbor is not in open set
                if(newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;         
                    neighbor.hCost = GetDistance(neighbor, targetNode); // Set parent of neighbor to current
                    neighbor.parent = currentNode;

                    if(!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode && currentNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        return path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int disX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int disY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if(disX > disY) 
            return 14 * disY + 10*(disX - disY);

        return 14*disX + 10 * (disY - disX);
    }
}
