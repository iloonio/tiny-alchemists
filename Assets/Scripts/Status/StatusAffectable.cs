using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StatusAffectable : NetworkBehaviour
{
    private NetworkList<int> statusIds = new();
    private Dictionary<int, float> durations = new();
    private readonly List<Status> _toRemove = new();

    public void AddStatus(Status status, float duration)
    {
        if (!IsServer) return;

        int statusId = (int) status;
        if (!statusIds.Contains(statusId))
        {
            statusIds.Add(statusId);
            durations.Add(statusId, duration);
            status.OnStart(gameObject);
        }
        else
        {
            durations[statusId] = Mathf.Max(durations[statusId], duration);
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        _toRemove.Clear();

        foreach (int statusId in statusIds)
        {
            Status status = (Status) statusId;
            status.OnUpdate(gameObject);
            durations[statusId] -= Time.deltaTime;
            if (durations[statusId] < 0)
                _toRemove.Add(status);
        }

        foreach (Status s in _toRemove)
            RemoveStatus(s);
    }

    public void RemoveStatus(Status status)
    {
        if (!IsServer) return;

        int statusId = (int) status;
        if (statusIds.Remove(statusId))
        {
            durations.Remove(statusId);
            status.OnEnd(gameObject);
        }
    }
}
