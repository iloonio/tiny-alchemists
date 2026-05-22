using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class MiasmaManager : NetworkBehaviour
{

    public static MiasmaManager Instance;

    [Header("Grid")]
    [SerializeField] private float _cellSize = 0.5f;
    public float CellSize => _cellSize;
    [SerializeField] private Vector3 _origin;
    public Vector3 Origin => _origin;
    [SerializeField] private Vector3Int _gridSize;
    public Vector3Int GridSize => _gridSize;
    
    [Header("Growth")]
    [SerializeField] private float _growthInterval = 1f;
    [SerializeField] private float _growthBaseRate = 0.1f;
    [SerializeField] private float _spawnBaseRate = 0.01f;
    [SerializeField] private float _influenceDecay = 0.5f;

    [Header("Status")]
    [SerializeField] private Status _status;
    [SerializeField] private CapsuleCollider _playerCollider;
    [SerializeField] private float _lossTimeThreshold = 5f;

    [Header("Debug")]
    [SerializeField] private bool _debugDraw = true;
    [SerializeField] private bool _debugShowInactive = true;
    [SerializeField] private float _debugSphereSize = 0.1f;
    [SerializeField] private Color _debugActiveColor = Color.green;
    [SerializeField] private Color _debugInactiveColor = Color.red;

    private Node[,,] _grid;
    private HashSet<Node> _allNodes;
    private HashSet<Node> _activeNodes;
    private Vector3Int _playerCellExtents;
    private float _lossTimer = 0f;

    private NetworkList<Vector3Int> _activeCells;
    public NetworkList<Vector3Int> ActiveCells => _activeCells;
    private NetworkVariable<bool> _isBatchUpdate;
    public NetworkVariable<bool> IsBatchUpdate => _isBatchUpdate;

    private void Awake()
    {
        Instance = this;
        _grid = new Node[_gridSize.x, _gridSize.y, _gridSize.z];
        _allNodes = new();
        _activeNodes = new();
        _activeCells = new NetworkList<Vector3Int>();
        _isBatchUpdate = new();
        _playerCellExtents = CalculatePlayerCellExtents();
    }

    private Vector3Int CalculatePlayerCellExtents()
    {
        Bounds bounds = _playerCollider.bounds;
        return new Vector3Int(
            Mathf.CeilToInt(bounds.extents.x / CellSize) + 1,
            Mathf.CeilToInt(bounds.extents.y / CellSize) + 1,
            Mathf.CeilToInt(bounds.extents.z / CellSize) + 1
        );
    }

    public override void OnNetworkSpawn()
    {
        _activeCells.OnListChanged += OnActiveCellsChanged;

        foreach (Vector3Int coord in _activeCells)
        {
            _activeNodes.Add(GetNode(coord));
        }

        if (IsServer)
        {    
            foreach (Node node in _allNodes)
            {
                if (node.startActive)
                {
                    ActivateNode(node);   
                }
            }
            
            _isBatchUpdate.Value = false;        

            InvokeRepeating(nameof(Grow), _growthInterval, _growthInterval);  
        }
    }

    public override void OnNetworkDespawn()
    {
        _activeCells.OnListChanged -= OnActiveCellsChanged;
    }

    private void OnActiveCellsChanged(NetworkListEvent<Vector3Int> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<Vector3Int>.EventType.Add:
                _activeNodes.Add(GetNode(changeEvent.Value));
                break;

            case NetworkListEvent<Vector3Int>.EventType.Remove:
                _activeNodes.Remove(GetNode(changeEvent.Value));
                break;

            case NetworkListEvent<Vector3Int>.EventType.Clear:
                _activeNodes.Clear();
                break;
        }
    }

    public void AddNode(Node node)
    {
        Vector3Int coord = node.coord;
        if (!InBounds(coord)) return;

        Node currentNode = _grid[coord.x, coord.y, coord.z];
        if (currentNode == null)
        {
            _grid[coord.x, coord.y, coord.z] = node;
            _allNodes.Add(node);
        }
        else
        {
            MergeNodes(currentNode, node);
        }
    }

    public Node GetNode(Vector3Int coord)
    {
        if (!InBounds(coord)) return null;
        return _grid[coord.x, coord.y, coord.z];
    }

    private void MergeNodes(Node node, Node other)
    {
        node.rate = Mathf.Min(node.rate, other.rate);
        node.threshold = Mathf.Max(node.threshold, other.threshold);
    }

    public void ActivateNode(Node node)
    {
        if (!IsServer) return;
        _activeCells.Add(node.coord);
    }

    public void DeactivateNode(Node node)
    {
        if (!IsServer) return;
       _activeCells.Remove(node.coord);
    }

    public IEnumerable<Node> GetActiveNodes()
    {
        foreach (Node node in _activeNodes)
        {
            yield return node;
        }
    }

    public bool IsNodeActive(Node node)
    {
        return _activeNodes.Contains(node);
    }

    private void Grow()
    {
        if (!IsServer) return;

        List<Node> nodesToActivate = new List<Node>();

        float activeRatio = _activeNodes.Count / (float) _allNodes.Count;

        foreach (Node node in _allNodes)
        {
            if (IsNodeActive(node)) continue;

            if (activeRatio >= node.threshold 
                && Random.value < _spawnBaseRate * node.rate)
            {
                nodesToActivate.Add(node);
                node.influence = 1f;
            }
        }

        foreach (Node node in _activeNodes)
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
                    && !IsNodeActive(neighbor)
                    && activeRatio > neighbor.threshold 
                    && Random.value < _growthBaseRate * neighbor.rate * node.influence)
                {
                    nodesToActivate.Add(neighbor);
                    neighbor.influence = Mathf.Max(neighbor.influence, node.influence * _influenceDecay);
                }
            }
        }

        BatchUpdate(nodesToActivate);
    }

    private void BatchUpdate(List<Node> nodesToActivate)
    {
        if (nodesToActivate.Count == 0) return;

        _isBatchUpdate.Value = true;
        Node firstNode = nodesToActivate.First();
        nodesToActivate.Remove(firstNode);

        foreach (Node node in nodesToActivate)
        {
            ActivateNode(node);
        }

        _isBatchUpdate.Value = false;
        ActivateNode(firstNode);
    }

    public Vector3Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt((worldPos.x - _origin.x) / _cellSize),
            Mathf.FloorToInt((worldPos.y - _origin.y) / _cellSize),
            Mathf.FloorToInt((worldPos.z - _origin.z) / _cellSize)
        );
    }

    public Vector3 GridToWorld(Vector3Int coord)
    {
        return new Vector3(
            _origin.x + coord.x * _cellSize + _cellSize / 2,
            _origin.y + coord.y * _cellSize + _cellSize / 2,
            _origin.z + coord.z * _cellSize + _cellSize / 2
        );
    }

    public bool InBounds(Vector3Int c)
    {
        return c.x >= 0 && c.x < _gridSize.x &&
               c.y >= 0 && c.y < _gridSize.y &&
               c.z >= 0 && c.z < _gridSize.z;
    }

    private void Update()
    {
        if (!IsServer) return;

        _lossTimer += Time.deltaTime;
        if (_lossTimer >= _lossTimeThreshold)
        {
            LoseClientRpc();
            FindAnyObjectByType<NetworkSceneManager>().Shutdown();
            return;
        }

        foreach (NetworkClient player in NetworkClient.Players)
        {
            Vector3Int center = WorldToGrid(player.transform.position);

            for (int x = -_playerCellExtents.x; x <= _playerCellExtents.x; x++)
            for (int y = -_playerCellExtents.y; y <= _playerCellExtents.y; y++)
            for (int z = -_playerCellExtents.z; z <= _playerCellExtents.z; z++)
            {
                Node node = GetNode(center + new Vector3Int(x, y, z));

                if (_activeNodes.Contains(node))
                {
                    player.GetComponent<StatusAffectable>().AddStatus(_status, 10000f);
                    goto Next;
                }
            }

            player.GetComponent<StatusAffectable>().RemoveStatus(_status);
            _lossTimer = 0f;

            Next:;
        }       
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void LoseClientRpc()
    {
        foreach (NetworkClient client in NetworkClient.Players)
        {
            client.GetComponent<PlayerUI>().ShowMajor("DEFEAT!");
        }
    }

    private void OnDrawGizmos()
    {
        if (!_debugDraw) return;
        
        Vector3 size = new Vector3(
            _gridSize.x * _cellSize,
            _gridSize.y * _cellSize,
            _gridSize.z * _cellSize
        );

        Vector3 center = _origin + size / 2f;

        Gizmos.color = _debugActiveColor;
        Gizmos.DrawWireCube(center, size);
        
        if (_grid == null) return;

        foreach (Node node in _allNodes)
        {
            if (IsNodeActive(node))
            {
                Gizmos.color = _debugActiveColor;
            } 
            else
            {
                if (!_debugShowInactive) continue;
                Gizmos.color = _debugInactiveColor;
            }
            Gizmos.DrawSphere(node.worldPos, _debugSphereSize);
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
    public bool startActive;
}