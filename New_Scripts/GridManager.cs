using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int gridWidth;
    public int gridHeight;
    public float nodeSize;
    public LayerMask obstacleLayer;

    private Node[,] grid;
    private Vector3 gridOrigin;

    private void Start()
    {
        CreateGrid();
    }

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWidth * nodeSize, gridHeight * nodeSize, 0.1f));

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    Gizmos.color = grid[x, y].walkable ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
                    Gizmos.DrawCube(grid[x, y].worldPosition, Vector3.one * (nodeSize - 0.1f));
                }
            }
        }
    }

    public void CreateGrid()
    {
        grid = new Node[gridWidth, gridHeight];
        gridOrigin = transform.position - Vector3.right * (gridWidth * nodeSize * 0.5f) - Vector3.up * (gridHeight * nodeSize * 0.5f);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPoint = gridOrigin + Vector3.right * (x * nodeSize + nodeSize * 0.5f) + Vector3.up * (y * nodeSize + nodeSize * 0.5f);
                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeSize / 2, obstacleLayer);

                grid[x, y] = new Node(walkable, worldPoint, x, y);
                Debug.Log($"Node ({x},{y}) at {worldPoint} - Walkable: {walkable}");
            }
        }
    }

    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        Vector3 relativePos = worldPosition - gridOrigin;
        int x = Mathf.Clamp(Mathf.RoundToInt(relativePos.x / nodeSize), 0, gridWidth - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(relativePos.y / nodeSize), 0, gridHeight - 1);
        return grid[x, y];
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }
}

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int gCost, hCost;
    public Node parent;

    public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }

    public int fCost => gCost + hCost;
}
