using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StatusAffectable : NetworkBehaviour
{

    private NetworkList<int> statusIds = new();
    private Dictionary<int, float> durations = new();

    public void AddStatus(Status status, float duration)
    {
        int statusId = (int) status;
        if (!statusIds.Contains(statusId))
        {
            statusIds.Add(statusId);
            durations.Add(statusId, duration);
            status.OnStart(gameObject);
        }
        else
        {
            durations[statusId] = Mathf.Min(durations[statusId], duration);
        }
    }

    private void Update()
    {
        List<Status> statusesToRemove = new();

        foreach (int statusId in statusIds)
        {
            Status status = (Status) statusId;
            status.OnUpdate(gameObject);
            durations[statusId] -= Time.deltaTime;
            if (durations[statusId] < 0)
            {
                statusesToRemove.Add(status);
            }
        }

        foreach (Status statusToRemove in statusesToRemove)
        {
            RemoveStatus(statusToRemove);
        }
    }

    public void RemoveStatus(Status status)
    {
        int statusId = (int) status;
        if (statusIds.Remove(statusId))
        {
            durations.Remove(statusId);
            status.OnEnd(gameObject);
        }
    }

}