using System.Collections.Generic;
using UnityEngine;

public abstract class Status : ScriptableObject
{
    private static readonly Dictionary<int, Status> _byId = new();

    [SerializeField] private int _id;

    public int Id => _id;

    protected virtual void OnEnable()
    {
        if (_byId.ContainsKey(_id) && _byId[_id] != this)
        {
            Debug.LogError($"Duplicate Status ID detected: {_id}");
            return;
        }

        _byId[_id] = this;
    }

    protected virtual void OnDisable()
    {
        if (_byId.ContainsKey(_id) && _byId[_id] == this)
        {
            _byId.Remove(_id);
        }
    }

    public static explicit operator int(Status status)
    {
        return status._id;
    }

    public static explicit operator Status(int id)
    {
        return _byId.TryGetValue(id, out var value) ? value : null;
    }

    public virtual void OnStart(GameObject target) {}
    public virtual void OnUpdate(GameObject target) {}
    public virtual void OnEnd(GameObject target) {}
}