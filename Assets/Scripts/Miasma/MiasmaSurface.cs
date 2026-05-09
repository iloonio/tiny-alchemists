using UnityEngine;

public class MiasmaSurface : MonoBehaviour
{
    [Header("Surfaces")]
    [SerializeField] private bool _top = true; 
    [SerializeField] private bool _sides = false;
    [SerializeField] private bool _bottom = false;
    [SerializeField] private float _thickness = 1f;

    [Header("Growth Behavior")]
    [SerializeField] private float _rate = 1f;
    [SerializeField] private float _threshold = 0f;
    [SerializeField] private bool _startActive = false;

    void Start()
    {
        var manager = MiasmaManager.Instance;

        Bounds bounds = GetComponent<BoxCollider>().bounds;

        Vector3Int min = manager.WorldToGrid(bounds.min - Vector3.one * _thickness);
        Vector3Int max = manager.WorldToGrid(bounds.max + Vector3.one * _thickness);

        for (int x = min.x; x <= max.x; x++)
        for (int y = min.y; y <= max.y; y++)
        for (int z = min.z; z <= max.z; z++)
        {
            Vector3Int coord = new Vector3Int(x, y, z);
            Vector3 worldPos = manager.GridToWorld(coord);

            if (!manager.InBounds(coord) || bounds.Contains(worldPos) || !IsAllowedSurface(worldPos, bounds)) continue;

            if (worldPos.y > bounds.max.y && !_top) continue;
            else if (worldPos.y < bounds.min.y && !_bottom) continue;
            else if (worldPos.y >= bounds.min.y && worldPos.y <= bounds.max.y && !_sides) continue;

            manager.AddNode(new Node
            {
                coord = coord,
                worldPos = worldPos,
                rate = _rate,
                threshold = _threshold,
                influence = _startActive ? 1f : 0f
            }, _startActive);
        }
    }

    private bool IsAllowedSurface(Vector3 worldPos, Bounds bounds)
    {
        bool insideX = worldPos.x >= bounds.min.x && worldPos.x <= bounds.max.x;
        bool insideY = worldPos.y >= bounds.min.y && worldPos.y <= bounds.max.y;
        bool insideZ = worldPos.z >= bounds.min.z && worldPos.z <= bounds.max.z;

        bool isTop = insideX && insideZ && worldPos.y > bounds.max.y;
        bool isBottom = insideX && insideZ && worldPos.y < bounds.min.y;
        bool isSide = (insideY && insideX && !insideZ) || (insideY && insideZ && !insideX);

        return (isTop && _top) ||
            (isBottom && _bottom) ||
            (isSide && _sides);
    }
}