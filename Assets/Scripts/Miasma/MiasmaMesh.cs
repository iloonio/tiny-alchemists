using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class MiasmaMesh : NetworkBehaviour 
{
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    private MeshRenderer _meshRenderer;
    public float tileScale = 1f;
    private bool _isBatchUpdate;

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _mesh = new Mesh {
            name = "MiasmaMesh"
        };
        _meshFilter.mesh = _mesh;
        Redraw();
    }

    public override void OnNetworkSpawn()
    {
        MiasmaManager.Instance.ActiveCells.OnListChanged += OnActiveCellsChanged;
        MiasmaManager.Instance.IsBatchUpdate.OnValueChanged += OnBatchUpdate;
    }

    public override void OnNetworkDespawn()
    {
        MiasmaManager.Instance.ActiveCells.OnListChanged -= OnActiveCellsChanged;
        MiasmaManager.Instance.IsBatchUpdate.OnValueChanged -= OnBatchUpdate;
    }

    private void OnBatchUpdate(bool previous, bool current)
    {  
        _isBatchUpdate = current;
    }

    private void OnActiveCellsChanged(NetworkListEvent<Vector3Int> changeEvent)
    {
        if (!_isBatchUpdate)
        {
            Redraw();
        }
    }

    private void Redraw()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        int vertexIndex = 0;

        foreach (Node node in MiasmaManager.Instance.GetActiveNodes())
        {
            AddCube(node, vertices, triangles, ref vertexIndex, uvs);
        }

        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.SetUVs(0, uvs);
        _mesh.SetTriangles(triangles, 0);
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }

    private void AddCube(Node node, List<Vector3> vertices, List<int> triangles, ref int vertexIndex, List<Vector2> uvs)
    {
        float h = MiasmaManager.Instance.CellSize * 0.5f;

        Vector3Int c = node.coord;
        Vector3 center = node.worldPos;

        bool HasNeighbor(Vector3Int offset)
        {
            Node n = MiasmaManager.Instance.GetNode(c + offset);
            return n != null && MiasmaManager.Instance.IsNodeActive(n);
        }

        Vector3 v000 = center + new Vector3(-h,-h,-h);
        Vector3 v100 = center + new Vector3( h,-h,-h);
        Vector3 v110 = center + new Vector3( h,-h, h);
        Vector3 v010 = center + new Vector3(-h,-h, h);
        Vector3 v001 = center + new Vector3(-h, h,-h);
        Vector3 v101 = center + new Vector3( h, h,-h);
        Vector3 v111 = center + new Vector3( h, h, h);
        Vector3 v011 = center + new Vector3(-h, h, h);

        void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, ref int vertexIndex)
        {
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);

            // Compute face normal to decide projection plane for continuous world-space UVs
            Vector3 normal = Vector3.Cross(b - a, d - a).normalized;
            float scale = tileScale / MiasmaManager.Instance.CellSize;

            // Helper to compute UV from a world-space vertex based on face orientation
            Vector2 WorldUV(Vector3 v)
            {
                // Top/bottom faces -> project XZ
                if (Mathf.Abs(normal.y) > Mathf.Abs(normal.x) && Mathf.Abs(normal.y) > Mathf.Abs(normal.z))
                {
                    return new Vector2(v.x, v.z) * scale;
                }
                // Left/right faces -> project ZY
                else if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y) && Mathf.Abs(normal.x) > Mathf.Abs(normal.z))
                {
                    return new Vector2(v.z, v.y) * scale;
                }
                // Front/back faces -> project XY
                else
                {
                    return new Vector2(v.x, v.y) * scale;
                }
            }

            uvs.Add(WorldUV(a));
            uvs.Add(WorldUV(b));
            uvs.Add(WorldUV(c));
            uvs.Add(WorldUV(d));

            triangles.Add(vertexIndex + 0);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);

            triangles.Add(vertexIndex + 0);
            triangles.Add(vertexIndex + 3);
            triangles.Add(vertexIndex + 2);

            vertexIndex += 4;
        }

        // +X face
        if (!HasNeighbor(Vector3Int.right))
            AddQuad(v100, v110, v111, v101, ref vertexIndex);

        // -X face
        if (!HasNeighbor(Vector3Int.left))
            AddQuad(v000, v001, v011, v010, ref vertexIndex);

        // +Y face
        if (!HasNeighbor(Vector3Int.up))
            AddQuad(v001, v101, v111, v011, ref vertexIndex);

        // -Y face
        if (!HasNeighbor(Vector3Int.down))
            AddQuad(v000, v010, v110, v100, ref vertexIndex);

        // +Z face
        if (!HasNeighbor(Vector3Int.forward))
            AddQuad(v010, v011, v111, v110, ref vertexIndex);

        // -Z face
        if (!HasNeighbor(Vector3Int.back))
            AddQuad(v000, v100, v101, v001, ref vertexIndex);
    }

}