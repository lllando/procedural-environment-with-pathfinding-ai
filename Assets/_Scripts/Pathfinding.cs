using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour {

    public Transform seeker, target;
    NodeGrid grid;

    [SerializeField] private float moveSpeed = 0.5f;
    private float waypointTolerance = 0.1f;

    void Awake() {
        grid = GetComponent<NodeGrid> ();
    }

    void Update() {
        // FindPath(seeker.position, target.position);
    }

    public void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0) {
            Node node = openSet[0];
            for (int i = 1; i < openSet.Count; i ++) {
                if (openSet[i].fCost < node.fCost || openSet[i].fCost == node.fCost) {
                    if (openSet[i].hCost < node.hCost)
                    {
                        node = openSet[i];
                    }
                }
            }

            openSet.Remove(node);
            closedSet.Add(node);

            if (node == targetNode) {
                RetracePath(startNode,targetNode);
                StopCoroutine(FollowPath());
                StartCoroutine(FollowPath());
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(node)) {
                if (!neighbour.walkable || closedSet.Contains(neighbour)) {
                    continue;
                }

                int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = node;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
    }

    void RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        grid.path = path;

    }

    int GetDistance(Node nodeA, Node nodeB) {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14*dstY + 10* (dstX-dstY);
        return 14*dstX + 10 * (dstY-dstX);
    }
    
    IEnumerator FollowPath()
    {
        if (grid.path == null || grid.path.Count == 0)
        {
            yield break;
        }

        int targetIndex = 0;

        while (true)
        {
            // Check if we've reached the end of the path.
            if (targetIndex >= grid.path.Count)
            {
                yield break; // Exit the coroutine when we reach the end.
            }

            Vector3 currentWaypoint = grid.path[targetIndex].worldPosition;

            // Calculate a target position with the same X and Z coordinates as the waypoint
            // but with the Y coordinate adjusted to match the terrain's elevation.
            Vector3 targetPosition = new Vector3(currentWaypoint.x, seeker.position.y, currentWaypoint.z);

            // Move the seeker towards the target position.
            Vector3 moveDirection = (targetPosition - seeker.position).normalized;
            seeker.position += moveDirection * moveSpeed * Time.deltaTime;

            // Check if we are close enough to the waypoint.
            if (Vector3.Distance(seeker.position, targetPosition) < waypointTolerance)
            {
                targetIndex++;
            }

            yield return null;
        }
    }
}