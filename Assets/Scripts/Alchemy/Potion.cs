
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Potion : NetworkBehaviour
{
    [SerializeField] private float _breakSpeed = 10f;

    private int _baseIngredientId;
    private List<int> _modifierIngredientIds = new();
    private Renderer _renderer;
    private Rigidbody _rb;
    public Rigidbody Rb => _rb;

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

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
             _baseIngredientIdNetwork.Value = _baseIngredientId;
            foreach (var modifierIngredientId in _modifierIngredientIds)
            {
                _modifierIngredientIdsNetwork.Add(modifierIngredientId);
            }
        }
        else
        {
            _baseIngredientId = _baseIngredientIdNetwork.Value;
            foreach (var modifierIngredientId in _modifierIngredientIdsNetwork)
            {
                _modifierIngredientIds.Add(modifierIngredientId);
            }
        }

        SetColor();
    }

    private void SetColor()
    {
        Color sum = ((IngredientType) _baseIngredientId).Color;

        foreach (var modifierIngredientId in _modifierIngredientIds)
        {
            sum += ((IngredientType) modifierIngredientId).Color;
        }

        _renderer.material.color = sum / (1f + _modifierIngredientIds.Count);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return; 

        if (collision.relativeVelocity.magnitude < _breakSpeed) return;

        ContactPoint contact = collision.GetContact(0);
        Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, contact.normal), contact.normal);

        PotionEffect effect = Instantiate(((BaseIngredientType) _baseIngredientId).PotionEffectPrefab, contact.point, rotation);

        effect.Initialize(_baseIngredientId, _modifierIngredientIds);
        effect.NetworkObject.Spawn();

        NetworkObject.Despawn();
    }
}