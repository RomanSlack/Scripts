using UnityEngine;
using System.Collections.Generic;

public class NewGridManager : MonoBehaviour
{
    public int gridWidth;
    public int gridHeight;
    public float nodeSize;
    public LayerMask obstacleLayer;
    public bool showDebugVisuals = true;  // Toggle for debug visualization

    private Node[,] grid;
    private Vector3 gridOrigin;

    private void Start()
    {
        CreateGrid();
    }

    private void OnDrawGizmos()
    {
        if (!showDebugVisuals || grid == null) return;

        // Draw grid bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWidth * nodeSize, gridHeight * nodeSize, 0.1f));

        if (gridWidth > 0 && gridHeight > 0 && grid.Length == gridWidth * gridHeight)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y] != null)
                    {
                        // Draw node positions
                        Gizmos.color = grid[x, y].walkable ? Color.white : Color.red;
                        Gizmos.DrawWireCube(grid[x, y].worldPosition, Vector3.one * (nodeSize - 0.1f));
                        
                        // Optional: Draw debug text for coordinates
                        // UnityEditor.Handles.Label(grid[x,y].worldPosition, $"({x},{y})");
                    }
                }
            }
        }
    }

    public void CreateGrid()
    {
        if (gridWidth <= 0 || gridHeight <= 0)
        {
            Debug.LogError("Grid dimensions must be positive!");
            return;
        }

        // Calculate grid origin based on the GridManager's position
        gridOrigin = CalculateGridOrigin();
        grid = new Node[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPoint = CalculateWorldPosition(x, y);
                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeSize * 0.4f, obstacleLayer);
                
                grid[x, y] = new Node(walkable, worldPoint, x, y);
                
                // Debug visualization of node creation
                Debug.DrawLine(worldPoint, worldPoint + Vector3.up * 0.1f, Color.green, 1f);
            }
        }

        Debug.Log($"Grid created at {transform.position} with dimensions {gridWidth}x{gridHeight}");
    }

    private Vector3 CalculateGridOrigin()
    {
        // Calculate the bottom-left corner of the grid based on the GridManager's position
        return transform.position - 
               Vector3.right * (gridWidth * nodeSize * 0.5f) - 
               Vector3.up * (gridHeight * nodeSize * 0.5f);
    }

    private Vector3 CalculateWorldPosition(int x, int y)
    {
        return gridOrigin + 
               Vector3.right * (x * nodeSize + nodeSize * 0.5f) + 
               Vector3.up * (y * nodeSize + nodeSize * 0.5f);
    }

    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        // Calculate position relative to grid origin
        Vector3 relativePos = worldPosition - gridOrigin;
        
        // Calculate grid coordinates
        float percentX = relativePos.x / (gridWidth * nodeSize);
        float percentY = relativePos.y / (gridHeight * nodeSize);
        
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        
        int x = Mathf.Clamp(Mathf.FloorToInt(percentX * gridWidth), 0, gridWidth - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(percentY * gridHeight), 0, gridHeight - 1);

        Node node = grid[x, y];
        
        // Debug visualization of node lookup
        Debug.DrawLine(worldPosition, node.worldPosition, Color.yellow, 0.5f);
        
        return node;
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        // Check all 8 surrounding nodes
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridWidth && 
                    checkY >= 0 && checkY < gridHeight)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }

    // Utility method to convert grid coordinates to world position
    public Vector3 GridToWorldPosition(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return grid[x, y].worldPosition;
        }
        return Vector3.zero;
    }

    // Utility method to check if a position is within the grid bounds
    public bool IsInGrid(Vector3 worldPosition)
    {
        Vector3 relativePos = worldPosition - gridOrigin;
        float percentX = relativePos.x / (gridWidth * nodeSize);
        float percentY = relativePos.y / (gridHeight * nodeSize);

        return percentX >= 0 && percentX <= 1 && percentY >= 0 && percentY <= 1;
    }

#if UNITY_EDITOR
    // Optional: Method to recalculate grid in editor
    public void RecalculateGrid()
    {
        CreateGrid();
    }
#endif
}