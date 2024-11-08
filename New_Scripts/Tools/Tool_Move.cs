using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tool_Move : MonoBehaviour
{
    public Transform target;  // The target position
    public float pathfindingMoveSpeed = 2f;

    private GridManager gridManager;
    private Animator animator;
    private Rigidbody2D rb;

    private bool isPathfinding = false;
    private List<Node> currentPath = new List<Node>();

    private void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        animator = GetComponent<Animator>();  // Ensure each agent gets its own Animator
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smooth out Rigidbody movement
    }

    private void Start()
    {
        rb.gravityScale = 0f; // Disable gravity for 2D top-down movement
    }

    // Main function to execute the move based on the destination
    public void ExecuteMove(string destination)
    {
        Vector3 targetPosition = ConvertDestinationToCoordinates(destination);

        // Set the target position and start pathfinding
        if (targetPosition != Vector3.zero)
        {
            target.position = targetPosition;
            Debug.Log("Target Position Set: " + targetPosition); // Debug: Check if the target position is set
            StartPathfinding(targetPosition);
        }
        else
        {
            Debug.LogWarning("Invalid destination provided.");
        }
    }

    // Function to convert destination names to coordinates
    private Vector3 ConvertDestinationToCoordinates(string destination)
    {
        // Updated coordinates for each location
        switch (destination.ToUpper())
        {
            case "PARK":
                return new Vector3(-10.41f, 11.53f, 0);  // Updated coordinates for Park
            case "HOME":
                return new Vector3(-4f, -7f, 0); // Updated coordinates for Home
            case "GYM":
                return new Vector3(14f, -1.82f, 0);  // Updated coordinates for Gym
            case "LIBRARY":
                return new Vector3(3.65f, 1.97f, 0);  // Updated coordinates for Library
            default:
                Debug.LogWarning("Unknown destination: " + destination);
                return Vector3.zero;
        }
    }

    // Function to start pathfinding to the target
    private void StartPathfinding(Vector3 targetPosition)
    {
        StopAllCoroutines(); // Stop any existing pathfinding coroutine
        Debug.Log("Starting pathfinding to: " + targetPosition); // Debug: Check if pathfinding is starting
        StartCoroutine(FindPath(transform.position, targetPosition)); // Begin pathfinding to the new target
    }

    private IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Debug.Log("Finding Path from " + startPos + " to " + targetPos); // Debug: Check start and target positions

        Node startNode = gridManager.GetNodeFromWorldPoint(startPos);
        Node targetNode = gridManager.GetNodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null)
        {
            Debug.LogError("Invalid Start or Target Node");
            yield break;
        }

        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();
        currentPath.Clear();

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                currentPath = RetracePath(startNode, targetNode);
                Debug.Log("Path found with " + currentPath.Count + " nodes."); // Debug: Check if path is found
                isPathfinding = true;
                StartCoroutine(MoveAlongPath(currentPath));
                yield break;
            }

            foreach (Node neighbor in gridManager.GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        Debug.LogError("Path not found.");
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private IEnumerator MoveAlongPath(List<Node> path)
{
    foreach (Node node in path)
    {
        Vector3 targetPos = node.worldPosition;
        Debug.Log("Moving to " + targetPos); // Debug: Check movement to each node
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPos, pathfindingMoveSpeed * Time.deltaTime);
            rb.MovePosition(newPosition);

            Vector2 direction = (targetPos - transform.position).normalized;
            animator.SetFloat("moveX", direction.x);
            animator.SetFloat("moveY", direction.y);
            animator.SetBool("isMoving", true);

            yield return null; // Use normal frame updates for better control
        }

        // Ensure exact position alignment with target node
        rb.MovePosition(targetPos);
    }

    isPathfinding = false;
    animator.SetBool("isMoving", false);
}


    // Get the distance between two nodes
    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        return distX + distY;
    }
}
