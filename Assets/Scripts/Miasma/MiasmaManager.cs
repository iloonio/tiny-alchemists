using System.Collections.Generic;
using UnityEngine;

public class MiasmaManager : MonoBehaviour
{
    public static MiasmaManager Instance;

    [Header("Grid")]
    [SerializeField] private float cellSize = 0.5f;
    [SerializeField] private Vector3 origin;
    [SerializeField] private Vector3Int gridSize;

    [Header("Growth")]
    [SerializeField] private float growthInterval = 1f;
    [SerializeField] private float growthBaseRate = 0.1f;
    [SerializeField] private float spawnBaseRate = 0.01f;
    [SerializeField] private float influenceDecay = 0.5f;

    [Header("Debug")]
    public bool debugDraw = true;
    public float debugSphereSize = 0.1f;
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.red;

    private Node[,,] grid;
    private HashSet<Node> activeNodes = new HashSet<Node>();
    private HashSet<Node> inactiveNodes = new HashSet<Node>();

    private void Awake()
    {
        Instance = this;
        grid = new Node[gridSize.x, gridSize.y, gridSize.z];
    }

    private void Start()
    {
        InvokeRepeating(nameof(Grow), growthInterval, growthInterval);
    }

    public void AddNode(Node node)
    {
        Vector3Int coord = node.coord;
        if (!InBounds(coord)) return;

        Node currentNode = grid[coord.x, coord.y, coord.z];
        if (currentNode == null)
        {
            grid[coord.x, coord.y, coord.z] = node;
            inactiveNodes.Add(node);
        }
        else
        {
            MergeNodes(currentNode, node);
        }
    }

    public Node GetNode(Vector3Int coord)
    {
        if (!InBounds(coord)) return null;
        return grid[coord.x, coord.y, coord.z];
    }

    private void MergeNodes(Node node, Node other)
    {
        node.rate = Mathf.Min(node.rate, other.rate);
        node.threshold = Mathf.Max(node.threshold, other.threshold);
    }

    public void ActivateNode(Node node)
    {
        inactiveNodes.Remove(node);
        activeNodes.Add(node);
    }

    public void DeactivateNode(Node node)
    {
        activeNodes.Remove(node);
        inactiveNodes.Add(node);
    }

    public List<Vector3> GetActiveNodePositions()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (Node node in activeNodes)
            positions.Add(node.worldPos);
        return positions;
    }

    private void Grow()
    {
        List<Node> nodesToActivate = new List<Node>();

        float activeRatio = activeNodes.Count / (float)(activeNodes.Count + inactiveNodes.Count);

        foreach (Node node in inactiveNodes)
        {
            if (activeRatio > node.threshold 
                && Random.value < spawnBaseRate * node.rate)
            {
                nodesToActivate.Add(node);
                node.influence = 1f;
            }
        }

        foreach (Node node in activeNodes)
        {
            for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && y == 0 && z == 0) continue;

                Vector3Int neighborCoord = node.coord + new Vector3Int(x, y, z);
                if (!InBounds(neighborCoord)) continue;

                Node neighbor = GetNode(neighborCoord);
                if (neighbor != null 
                    && !activeNodes.Contains(neighbor) 
                    && activeRatio > neighbor.threshold 
                    && Random.value < growthBaseRate * neighbor.rate * node.influence)
                {
                    nodesToActivate.Add(neighbor);
                    neighbor.influence = Mathf.Max(neighbor.influence, node.influence * influenceDecay);
                }
            }
        }

        foreach (Node node in nodesToActivate)
            ActivateNode(node);
    }

    public Vector3Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt((worldPos.x - origin.x) / cellSize),
            Mathf.FloorToInt((worldPos.y - origin.y) / cellSize),
            Mathf.FloorToInt((worldPos.z - origin.z) / cellSize)
        );
    }

    public Vector3 GridToWorld(Vector3Int coord)
    {
        return new Vector3(
            origin.x + coord.x * cellSize + cellSize / 2,
            origin.y + coord.y * cellSize + cellSize / 2,
            origin.z + coord.z * cellSize + cellSize / 2
        );
    }

    public bool InBounds(Vector3Int c)
    {
        return c.x >= 0 && c.x < gridSize.x &&
               c.y >= 0 && c.y < gridSize.y &&
               c.z >= 0 && c.z < gridSize.z;
    }

    private void OnDrawGizmos()
    {
        if (!debugDraw || grid == null) return;

        foreach (Node node in activeNodes)
        {
            Gizmos.color = activeColor;
            Gizmos.DrawSphere(node.worldPos, debugSphereSize);
        }

        foreach (Node node in inactiveNodes)
        {
            Gizmos.color = inactiveColor;
            Gizmos.DrawSphere(node.worldPos, debugSphereSize);
        }
    }
}

public class Node
{
    public Vector3Int coord;
    public Vector3 worldPos;

    public float rate;
    public float threshold;
    public float influence;
}