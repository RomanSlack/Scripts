using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int gridWidth;
    public int gridHeight;
    public int gridDepth; // New for 3D
    public float nodeSize;
    public LayerMask obstacleLayer;

    private Node[,,] grid; // 3D grid
    private Vector3 gridOrigin;

    private void Start()
    {
        CreateGrid();
    }

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWidth * nodeSize, gridHeight * nodeSize, gridDepth * nodeSize));

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    if (grid[x, y, z] != null)
                    {
                        Gizmos.color = grid[x, y, z].walkable ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
                        Gizmos.DrawCube(grid[x, y, z].worldPosition, Vector3.one * (nodeSize - 0.1f));
                    }
                }
            }
        }
    }

    public void CreateGrid()
    {
        grid = new Node[gridWidth, gridHeight, gridDepth];
        gridOrigin = transform.position - Vector3.right * (gridWidth * nodeSize * 0.5f) 
                                       - Vector3.up * (gridHeight * nodeSize * 0.5f) 
                                       - Vector3.forward * (gridDepth * nodeSize * 0.5f);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    Vector3 worldPoint = gridOrigin + Vector3.right * (x * nodeSize + nodeSize * 0.5f) 
                                                      + Vector3.up * (y * nodeSize + nodeSize * 0.5f) 
                                                      + Vector3.forward * (z * nodeSize + nodeSize * 0.5f);
                    bool walkable = Physics.OverlapSphere(worldPoint, nodeSize / 2, obstacleLayer).Length == 0;


                    grid[x, y, z] = new Node(walkable, worldPoint, x, y, z);
                }
            }
        }
    }

    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        Vector3 relativePos = worldPosition - gridOrigin;
        int x = Mathf.Clamp(Mathf.RoundToInt(relativePos.x / nodeSize), 0, gridWidth - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(relativePos.y / nodeSize), 0, gridHeight - 1);
        int z = Mathf.Clamp(Mathf.RoundToInt(relativePos.z / nodeSize), 0, gridDepth - 1);
        return grid[x, y, z];
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;
                    int checkZ = node.gridZ + z;

                    if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight && checkZ >= 0 && checkZ < gridDepth)
                    {
                        neighbors.Add(grid[checkX, checkY, checkZ]);
                    }
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
    public int gridZ; // Added for 3D
    public int gCost, hCost;
    public Node parent;

    public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY, int gridZ)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
        this.gridZ = gridZ;
    }

    public int fCost => gCost + hCost;
}