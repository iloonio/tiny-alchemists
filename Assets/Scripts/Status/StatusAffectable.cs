using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class StatusAffectable : NetworkBehaviour
{
    private List<Status> _statuses = new();
    private NetworkList<int> _statusIds = new();
    private Dictionary<int, float> _durations = new();


    public override void OnNetworkSpawn()
    {
        _statusIds.OnListChanged += OnStatusChanged;
    }

    public override void OnNetworkDespawn()
    {
        _statusIds.OnListChanged -= OnStatusChanged;
    }

    private void OnStatusChanged(NetworkListEvent<int> changeEvent)
    {
        Status status = (Status)changeEvent.Value;

        switch (changeEvent.Type)
        {
            case NetworkListEvent<int>.EventType.Add:
                status.OnStatusStart(gameObject);
                _statuses.Add(status);
                Debug.Log("Added status " + status.name + " to " + gameObject.name);
                break;

            case NetworkListEvent<int>.EventType.Remove:
                status.OnStatusEnd(gameObject);
                _statuses.Remove(status);
                Debug.Log("Removed status " + status.name + " from " + gameObject.name);
                break;

            default:
                break;
        }
    }

    private void FixedUpdate()
    {
        foreach (Status status in new List<Status>(_statuses))
        {
            status.OnStatusFixedUpdate(gameObject);
        }
    }

    public void AddStatus(Status status, float duration)
    {
        AddStatusRpc((int)status, duration);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddStatusRpc(int statusId, float duration)
    {
        if (!_statusIds.Contains(statusId))
        {
            _statusIds.Add(statusId);
            _durations[statusId] = duration;
        }
        else
        {
            _durations[statusId] = Mathf.Max(_durations[statusId], duration);
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        List<int> remove = new();

        foreach (var statusId in _durations.Keys.ToList())
        {
            _durations[statusId] -= Time.deltaTime;
            if (_durations[statusId] < 0)
            {
                remove.Add(statusId);
            }
        }

        foreach (int statusId in remove)
        {
            _statusIds.Remove(statusId);
            _durations.Remove(statusId);
        }
    }

    public void RemoveStatus(Status status)
    {
        RemoveStatusRpc((int)status);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RemoveStatusRpc(int statusId)
    {
        _statusIds.Remove(statusId);
        _durations.Remove(statusId);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
    public void InstatiateFXPrefabRpc(int statusId)
    {

    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
    public void DestroyFXPrefabRpc(int statusId)
    {

    }

    public bool HasStatus(Status status)
    {
        return _statusIds.Contains((int)status);
    }
}
