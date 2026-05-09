using System.Collections.Generic;
using UnityEngine;

public abstract class IngredientType : ScriptableObject
{
    private static readonly Dictionary<int, IngredientType> _byId = new();

    [SerializeField] private int _id;
    [SerializeField] private Color _color;
    [SerializeField] private GameObject _prefab;

    public int Id => _id;
    public Color Color => _color;
    public GameObject Prefab => _prefab;

    protected virtual void OnEnable()
    {
        if (_byId.ContainsKey(_id) && _byId[_id] != this)
        {
            Debug.LogError($"Duplicate IngredientType ID detected: {_id}");
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

    public static explicit operator int(IngredientType ingredient)
    {
        return ingredient._id;
    }

    public static explicit operator IngredientType(int id)
    {
        return _byId.TryGetValue(id, out var value) ? value : null;
    }
}