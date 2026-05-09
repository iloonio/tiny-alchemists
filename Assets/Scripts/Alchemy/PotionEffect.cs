using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PotionEffect : NetworkBehaviour
{
    private int _baseEffectId;
    private List<int> _modifierEffectIds = new();
    private BaseEffect _baseEffect;
    private List<ModifierEffect> _modifierEffects = new();

    private NetworkVariable<int> _baseEffectIdNetwork = new();
    private NetworkList<int> _modifierEffectIdsNetwork = new();

    public void Initialize(int baseIngredientId, List<int> modifierEffectIds)
    {
        _baseEffectId = baseIngredientId;
        foreach (var modifierEffectId in modifierEffectIds)
        {
            _modifierEffectIds.Add(modifierEffectId);
        }
    }

    private void Start()
    {
        if (IsServer) 
        {
            _baseEffectIdNetwork.Value = _baseEffectId;
            foreach (var modifierEffectId in _modifierEffectIds)
            {
                _modifierEffectIdsNetwork.Add(modifierEffectId);
            }
        } 
         
        _baseEffect = (BaseEffect) ((IngredientType) _baseEffectIdNetwork.Value).CreateEffect();
        foreach (var modifierEffectId in _modifierEffectIdsNetwork)
        {
            _modifierEffects.Add((ModifierEffect) ((IngredientType) modifierEffectId).CreateEffect());
        }

        EffectSetup();
        EffectStart();
        StartCoroutine(DespawnAfter(_baseEffect.Duration));
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
        _baseEffect.OnEffectSetup(this);
        foreach (var modifierEffect in _modifierEffects)
        {
            modifierEffect.OnEffectSetup(this, _baseEffect, _modifierEffects);
        }
    }

    private void EffectStart()
    {
        _baseEffect.OnEffectStart(this);
        foreach (var modifierEffect in _modifierEffects)
        {
            modifierEffect.OnEffectStart(this, _baseEffect, _modifierEffects);
        }
    }

    private void EffectUpdate()
    {
        _baseEffect.OnEffectUpdate(this);
        foreach (var modifierEffect in _modifierEffects)
        {
            modifierEffect.OnEffectUpdate(this, _baseEffect, _modifierEffects);
        }
    }

    private void EffectEnd()
    {
        _baseEffect.OnEffectEnd(this);
        foreach (var modifierEffect in _modifierEffects)
        {
            modifierEffect.OnEffectEnd(this, _baseEffect, _modifierEffects);
        }
    }

    private void EffectOnTriggerEnter(Collider other)
    {
        _baseEffect.OnEffectTriggerEnter(other, this);
        foreach (var modifierEffect in _modifierEffects)
        {
            modifierEffect.OnEffectTriggerEnter(other, this, _baseEffect, _modifierEffects);
        }
    }

    private void EffectOnTriggerExit(Collider other)
    {
        _baseEffect.OnEffectTriggerExit(other, this);
        foreach (var modifierEffect in _modifierEffects)
        {
            modifierEffect.OnEffectTriggerExit(other, this, _baseEffect, _modifierEffects);
        }
    }
}