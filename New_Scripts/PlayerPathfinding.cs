using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerPathfinding : MonoBehaviour
{
    public Transform target;  // This should be the correct target (e.g., target_1 or target_2)
    public float moveSpeed = 5f;
    public float pathfindingMoveSpeed = 2f;
    public float acceleration = 50f;
    public float deceleration = 50f;
    public LayerMask solidObjectsLayer;
    public Vector3 initialPosition;

    private GridManager gridManager;
    private Animator animator;
    private Rigidbody2D rb;

    private bool isPathfinding = false;
    private List<Node> currentPath;
    private Vector2 movement;
    private Vector2 lastNonZeroMovement;
    private Vector2 smoothVelocity;

    private void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        animator = GetComponent<Animator>();  // Ensure each agent gets its own Animator
        rb = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
    }

    private void Start()
    {
        rb.gravityScale = 0f;
        lastNonZeroMovement = Vector2.down;
    }

    private void Update()
    {
        if (!isPathfinding)
        {
            HandleManualMovement();
        }
    }

    private void FixedUpdate()
    {
        if (isPathfinding)
        {
            MoveAlongPath();
        }
        else
        {
            HandleManualMovementPhysics();
        }
    }

    public void StartPathfinding()
    {
        if (target != null && !isPathfinding)
        {
            StopAllCoroutines();
            StartCoroutine(FindPath(transform.position, target.position));
        }
    }

    private IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = gridManager.GetNodeFromWorldPoint(startPos);
        Node targetNode = gridManager.GetNodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();

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
                isPathfinding = true;
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
        return OptimizePath(path);
    }

    private List<Node> OptimizePath(List<Node> path)
    {
        List<Node> optimizedPath = new List<Node>();
        Vector2 oldDirection = Vector2.zero;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 newDirection = new Vector2(
                path[i + 1].gridX - path[i].gridX, 
                path[i + 1].gridY - path[i].gridY
            );

            if (newDirection != oldDirection)
            {
                optimizedPath.Add(path[i]);
                oldDirection = newDirection;
            }
        }

        optimizedPath.Add(path[path.Count - 1]);
        return optimizedPath;
    }

    private void MoveAlongPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            isPathfinding = false;
            animator.SetBool("isMoving", false);
            return;
        }

        Node targetNode = currentPath[0];
        Vector3 targetPos = targetNode.worldPosition;

        Vector2 direction = (targetPos - transform.position).normalized;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPos, pathfindingMoveSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, targetPos) <= 0.1f)
        {
            transform.position = targetPos;
            currentPath.RemoveAt(0);
        }
        else
        {
            rb.MovePosition(newPosition);
            animator.SetFloat("moveX", direction.x);
            animator.SetFloat("moveY", direction.y);
            animator.SetBool("isMoving", true);
        }

        if (currentPath.Count == 0)
        {
            isPathfinding = false;
            animator.SetBool("isMoving", false);
        }
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        return distX + distY;
    }

    private void HandleManualMovement()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.x != 0) movement.y = 0;

        if (movement != Vector2.zero)
        {
            movement.Normalize();
            lastNonZeroMovement = movement;
        }

        animator.SetFloat("moveX", lastNonZeroMovement.x);
        animator.SetFloat("moveY", lastNonZeroMovement.y);
    }

    private void HandleManualMovementPhysics()
    {
        Vector2 targetPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        if (IsWalkable(targetPosition))
        {
            Vector2 targetVelocity = movement * moveSpeed;
            smoothVelocity = Vector2.MoveTowards(smoothVelocity, targetVelocity, GetAcceleration() * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + smoothVelocity * Time.fixedDeltaTime);
            animator.SetBool("isMoving", smoothVelocity.magnitude > 0.1f);
        }
        else
        {
            smoothVelocity = Vector2.zero;
            animator.SetBool("isMoving", false);
        }
    }

    private float GetAcceleration()
    {
        return movement.sqrMagnitude > 0 ? acceleration : deceleration;
    }

    private bool IsWalkable(Vector2 targetPos)
    {
        return Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) == null;
    }

    public void ResetPosition()
    {
        StopAllCoroutines();
        transform.position = initialPosition;
        rb.position = initialPosition;
        isPathfinding = false;
        currentPath = null;
        animator.SetBool("isMoving", false);
        animator.SetFloat("moveX", 0);
        animator.SetFloat("moveY", -1);
    }
}
