using UnityEngine;

public class MiasmaSurface : MonoBehaviour
{
    [Header("Surfaces")]
    [SerializeField] private bool top = true;
    [SerializeField] private bool sides = false;
    [SerializeField] private bool bottom = false;
    [SerializeField] private float thickness = 1f;

    [Header("Growth Behavior")]
    [SerializeField] private float rate = 1f;
    [SerializeField] private float threshold = 0f;
    [SerializeField] private bool startActive = false;

    void Start()
    {
        var grid = MiasmaManager.Instance;

        Bounds bounds = GetComponent<BoxCollider>().bounds;

        Vector3Int min = grid.WorldToGrid(bounds.min - Vector3.one * thickness);
        Vector3Int max = grid.WorldToGrid(bounds.max + Vector3.one * thickness);

        for (int x = min.x; x <= max.x; x++)
        for (int y = min.y; y <= max.y; y++)
        for (int z = min.z; z <= max.z; z++)
        {
            Vector3Int coord = new Vector3Int(x, y, z);
            Vector3 worldPos = grid.GridToWorld(coord);

            if (!grid.InBounds(coord) || bounds.Contains(worldPos) || !IsAllowedSurface(worldPos, bounds)) continue;

            if (worldPos.y > bounds.max.y && !top) continue;
            else if (worldPos.y < bounds.min.y && !bottom) continue;
            else if (worldPos.y >= bounds.min.y && worldPos.y <= bounds.max.y && !sides) continue;

            grid.AddNode(new Node
            {
                coord = coord,
                worldPos = worldPos,
                rate = rate,
                threshold = threshold,
                influence = 0f
            });

            if (startActive) {
                Node node = grid.GetNode(coord);
                grid.ActivateNode(node);
                node.influence = 1f;
            }
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

        return (isTop && top) ||
            (isBottom && bottom) ||
            (isSide && sides);
    }
}