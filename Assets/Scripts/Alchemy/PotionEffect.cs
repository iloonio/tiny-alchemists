using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PotionEffect : NetworkBehaviour
{
    private int _baseIngredientId;
    private List<int> _modifierIngredientIds = new();
    private BaseIngredientType _baseIngredient;
    private List<ModifierIngredientType> _modifierIngredients = new();

    private NetworkVariable<int> _baseIngredientIdNetwork = new();
    private NetworkList<int> _modifierIngredientIdsNetwork = new();

    public void Initialize(int baseIngredientId, List<int> modifierIngredientIds)
    {
        _baseIngredientId = baseIngredientId;
        foreach (var modifierIngredientId in modifierIngredientIds)
        {
            _modifierIngredientIds.Add(modifierIngredientId);
        }
    }

    private void Start()
    {
        if (IsServer) 
        {
            _baseIngredientIdNetwork.Value = _baseIngredientId;
            foreach (var modifierIngredientId in _modifierIngredientIds)
            {
                _modifierIngredientIdsNetwork.Add(modifierIngredientId);
            }
        } 
         
        _baseIngredient = (BaseIngredientType) _baseIngredientIdNetwork.Value;
        foreach (var modifierIngredientId in _modifierIngredientIdsNetwork)
        {
            _modifierIngredients.Add((ModifierIngredientType) modifierIngredientId);
        }

        EffectSetup();
        EffectStart();
        StartCoroutine(DespawnAfter(_baseIngredient.Duration));
    }

    private void Update()
    {
        EffectUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        EffectOnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        EffectOnTriggerExit(other);
    }

    private IEnumerator DespawnAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        EffectEnd();
        NetworkObject.Despawn();
    }

    private void EffectSetup()
    {
        _baseIngredient.OnEffectSetup(this);
        foreach (var modifierIngredient in _modifierIngredients)
        {
            modifierIngredient.OnEffectSetup(this, _baseIngredient, _modifierIngredients);
        }
    }

    private void EffectStart()
    {
        _baseIngredient.OnEffectStart(this);
        foreach (var modifierIngredient in _modifierIngredients)
        {
            modifierIngredient.OnEffectStart(this, _baseIngredient, _modifierIngredients);
        }
    }

    private void EffectUpdate()
    {
        _baseIngredient.OnEffectUpdate(this);
        foreach (var modifierIngredient in _modifierIngredients)
        {
            modifierIngredient.OnEffectUpdate(this, _baseIngredient, _modifierIngredients);
        }
    }

    private void EffectEnd()
    {
        _baseIngredient.OnEffectEnd(this);
        foreach (var modifierIngredient in _modifierIngredients)
        {
            modifierIngredient.OnEffectEnd(this, _baseIngredient, _modifierIngredients);
        }
    }

    private void EffectOnTriggerEnter(Collider other)
    {
        _baseIngredient.OnEffectTriggerEnter(other, this);
        foreach (var modifierIngredient in _modifierIngredients)
        {
            modifierIngredient.OnEffectTriggerEnter(other, this, _baseIngredient, _modifierIngredients);
        }
    }

    private void EffectOnTriggerExit(Collider other)
    {
        _baseIngredient.OnEffectTriggerExit(other, this);
        foreach (var modifierIngredient in _modifierIngredients)
        {
            modifierIngredient.OnEffectTriggerExit(other, this, _baseIngredient, _modifierIngredients);
        }
    }
}