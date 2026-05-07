// using System.Collections.Generic;
// using System;
// using System.Linq;
// using UnityEngine;
// using Unity.Netcode;
// using Random = UnityEngine.Random;

// public class MiasmaManager : NetworkBehaviour
// {

//     public static MiasmaManager Instance;

//     [Header("Grid")]
//     [SerializeField] private float cellSize = 0.5f;
//     public float CellSize => cellSize;
//     [SerializeField] private Vector3 origin;
//     public Vector3 Origin => origin;
//     [SerializeField] private Vector3Int gridSize;
//     public Vector3Int GridSize => gridSize;
    
//     [Header("Growth")]
//     [SerializeField] private float growthInterval = 1f;
//     [SerializeField] private float growthBaseRate = 0.1f;
//     [SerializeField] private float spawnBaseRate = 0.01f;
//     [SerializeField] private float influenceDecay = 0.5f;

//     [Header("Debug")]
//     [SerializeField] private bool debugDraw = true;
//     [SerializeField] private bool debugShowInactive = true;
//     [SerializeField] private float debugSphereSize = 0.1f;
//     [SerializeField] private Color activeColor = Color.green;
//     [SerializeField] private Color inactiveColor = Color.red;

//     private Node[,,] grid;
//     private HashSet<Node> allNodes;
//     private HashSet<Node> activeNodes;

//     private NetworkList<Vector3Int> activeCells;
//     public NetworkList<Vector3Int> ActiveCells => activeCells;
//     private NetworkVariable<bool> isBatchUpdate;
//     public NetworkVariable<bool> IsBatchUpdate => isBatchUpdate;

//     private void Awake()
//     {
//         Instance = this;
//         grid = new Node[gridSize.x, gridSize.y, gridSize.z];
//         allNodes = new();
//         activeNodes = new();
//         activeCells = new NetworkList<Vector3Int>();
//         isBatchUpdate = new();
//     }

//     public override void OnNetworkSpawn()
//     {

//         if (!IsServer) return;
        
//         foreach (Node node in activeNodes)
//         {
//             activeCells.Add(node.coord);
//         }
        
//         isBatchUpdate.Value = false;
        
//         activeCells.OnListChanged += OnActiveCellsChanged;

//         InvokeRepeating(nameof(Grow), growthInterval, growthInterval);  
//     }

//     public override void OnNetworkDespawn()
//     {
//         activeCells.OnListChanged -= OnActiveCellsChanged;
//     }

//     private void OnActiveCellsChanged(NetworkListEvent<Vector3Int> changeEvent)
//     {
//         switch (changeEvent.Type)
//         {
//             case NetworkListEvent<Vector3Int>.EventType.Add:
//                 activeNodes.Add(GetNode(changeEvent.Value));
//                 break;

//             case NetworkListEvent<Vector3Int>.EventType.Remove:
//                 activeNodes.Remove(GetNode(changeEvent.Value));
//                 break;

//             case NetworkListEvent<Vector3Int>.EventType.Clear:
//                 activeNodes.Clear();
//                 break;
//         }
//     }

//     public void AddNode(Node node, bool startActive)
//     {
//         Vector3Int coord = node.coord;
//         if (!InBounds(coord)) return;

//         Node currentNode = grid[coord.x, coord.y, coord.z];
//         if (currentNode == null)
//         {
//             grid[coord.x, coord.y, coord.z] = node;
//             allNodes.Add(node);
//             if (startActive) activeNodes.Add(node);
//         }
//         else
//         {
//             MergeNodes(currentNode, node);
//         }
//     }

//     public Node GetNode(Vector3Int coord)
//     {
//         if (!InBounds(coord)) return null;
//         return grid[coord.x, coord.y, coord.z];
//     }

//     private void MergeNodes(Node node, Node other)
//     {
//         node.rate = Mathf.Min(node.rate, other.rate);
//         node.threshold = Mathf.Max(node.threshold, other.threshold);
//     }

//     public void ActivateNode(Node node)
//     {
//         if (!IsServer) return;
//         activeCells.Add(node.coord);
//     }

//     public void DeactivateNode(Node node)
//     {
//         if (!IsServer) return;
//        activeCells.Remove(node.coord);
//     }

//     public IEnumerable<Node> GetActiveNodes()
//     {
//         foreach (Node node in activeNodes)
//         {
//             yield return node;
//         }
//     }

//     public bool IsNodeActive(Node node)
//     {
//         return activeNodes.Contains(node);
//     }

//     private void Grow()
//     {
//         if (!IsServer) return;

//         List<Node> nodesToActivate = new List<Node>();

//         float activeRatio = activeNodes.Count / (float) allNodes.Count;

//         foreach (Node node in allNodes)
//         {
//             if (IsNodeActive(node)) continue;

//             if (activeRatio >= node.threshold 
//                 && Random.value < spawnBaseRate * node.rate)
//             {
//                 nodesToActivate.Add(node);
//                 node.influence = 1f;
//             }
//         }

//         foreach (Node node in activeNodes)
//         {
//             for (int x = -1; x <= 1; x++)
//             for (int y = -1; y <= 1; y++)
//             for (int z = -1; z <= 1; z++)
//             {
//                 if (x == 0 && y == 0 && z == 0) continue;

//                 Vector3Int neighborCoord = node.coord + new Vector3Int(x, y, z);
//                 if (!InBounds(neighborCoord)) continue;

//                 Node neighbor = GetNode(neighborCoord);
//                 if (neighbor != null 
//                     && !IsNodeActive(neighbor)
//                     && activeRatio > neighbor.threshold 
//                     && Random.value < growthBaseRate * neighbor.rate * node.influence)
//                 {
//                     nodesToActivate.Add(neighbor);
//                     neighbor.influence = Mathf.Max(neighbor.influence, node.influence * influenceDecay);
//                 }
//             }
//         }

//         BatchUpdate(nodesToActivate);
//     }

//     private void BatchUpdate(List<Node> nodesToActivate)
//     {
//         if (nodesToActivate.Count == 0) return;

//         isBatchUpdate.Value = true;
//         Node firstNode = nodesToActivate.First();
//         nodesToActivate.Remove(firstNode);

//         foreach (Node node in nodesToActivate)
//         {
//             ActivateNode(node);
//         }

//         isBatchUpdate.Value = false;
//         ActivateNode(firstNode);
//     }

//     public Vector3Int WorldToGrid(Vector3 worldPos)
//     {
//         return new Vector3Int(
//             Mathf.FloorToInt((worldPos.x - origin.x) / cellSize),
//             Mathf.FloorToInt((worldPos.y - origin.y) / cellSize),
//             Mathf.FloorToInt((worldPos.z - origin.z) / cellSize)
//         );
//     }

//     public Vector3 GridToWorld(Vector3Int coord)
//     {
//         return new Vector3(
//             origin.x + coord.x * cellSize + cellSize / 2,
//             origin.y + coord.y * cellSize + cellSize / 2,
//             origin.z + coord.z * cellSize + cellSize / 2
//         );
//     }

//     public bool InBounds(Vector3Int c)
//     {
//         return c.x >= 0 && c.x < gridSize.x &&
//                c.y >= 0 && c.y < gridSize.y &&
//                c.z >= 0 && c.z < gridSize.z;
//     }

//     private void OnDrawGizmos()
//     {
//         if (!debugDraw) return;
        
//         Vector3 size = new Vector3(
//             gridSize.x * cellSize,
//             gridSize.y * cellSize,
//             gridSize.z * cellSize
//         );

//         Vector3 center = origin + size / 2f;

//         Gizmos.color = activeColor;
//         Gizmos.DrawWireCube(center, size);
        
//         if (grid == null) return;

//         foreach (Node node in allNodes)
//         {
//             if (IsNodeActive(node))
//             {
//                 Gizmos.color = activeColor;
//             } 
//             else
//             {
//                 if (!debugShowInactive) continue;
//                 Gizmos.color = inactiveColor;
//             }
//             Gizmos.DrawSphere(node.worldPos, debugSphereSize);
//         }
//     }

// }

// public class Node
// {
//     public Vector3Int coord;
//     public Vector3 worldPos;

//     public float rate;
//     public float threshold;
//     public float influence;
// }