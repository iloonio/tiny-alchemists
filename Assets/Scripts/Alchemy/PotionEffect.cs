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
    private Color _color;
    public Color Color => _color;

    private NetworkVariable<int> _baseEffectIdNetwork = new();
    private NetworkList<int> _modifierEffectIdsNetwork = new();
    private NetworkVariable<Vector3> _colorNetwork = new();

    public void Initialize(int baseIngredientId, List<int> modifierEffectIds, Color color)
    {
        _baseEffectId = baseIngredientId;
        foreach (var modifierEffectId in modifierEffectIds)
        {
            _modifierEffectIds.Add(modifierEffectId);
        }
        _color = color;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _baseEffectIdNetwork.Value = _baseEffectId;
            foreach (var modifierEffectId in _modifierEffectIds)
            {
                _modifierEffectIdsNetwork.Add(modifierEffectId);
            }
            _colorNetwork.Value = new Vector3(_color.r, _color.g, _color.b);
        }

        _baseEffect = (BaseEffect)((IngredientType)_baseEffectIdNetwork.Value).CreateEffect();
        foreach (var modifierEffectId in _modifierEffectIdsNetwork)
        {
            _modifierEffects.Add((ModifierEffect)((IngredientType)modifierEffectId).CreateEffect());
        }
        _color = new Color(_colorNetwork.Value.x, _colorNetwork.Value.y, _colorNetwork.Value.z);
    }

    private void Start()
    {
        EffectSetup();

        if (!IsServer) return;

        EffectStart();
        StartCoroutine(DespawnAfter(_baseEffect.Duration));
    }

    private void Update()
    {
        if (!IsServer) return;

        EffectUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        EffectOnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

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
