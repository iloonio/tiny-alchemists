
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Potion : NetworkBehaviour
{
    private IngredientType _baseIngredient;
    private List<IngredientType> _modifierIngredients = new();
    private Renderer _renderer;

    private NetworkVariable<int> _baseIngredientId = new();
    private NetworkList<int> _modifierIngredientIds = new();

    public void Initialize(int baseIngredientId, List<int> modifierIngredientIds)
    {
        _baseIngredientId.Value = baseIngredientId;
        foreach (var modifierIngredientId in modifierIngredientIds)
        {
            _modifierIngredientIds.Add(modifierIngredientId);
        }
    }

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        _baseIngredient = (BaseIngredientType) _baseIngredientId.Value;
        foreach (var modifierIngredientId in _modifierIngredientIds)
        {
            _modifierIngredients.Add((ModifierIngredientType) modifierIngredientId);
        }

        SetColor();
    }

    private void SetColor()
    {
        Color sum = _baseIngredient.Color;

        foreach (var modifierIngredient in _modifierIngredients)
        {
            sum += modifierIngredient.Color;
        }

        _renderer.material.color = sum / (1f + _modifierIngredients.Count);
    }
}