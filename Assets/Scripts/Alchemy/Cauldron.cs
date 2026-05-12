using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Cauldron : NetworkBehaviour, IInteractable
{
    [Header("Spawning")]
    [SerializeField] private Potion _potionPrefab;
    [SerializeField] private Transform _potionSpawnPoint;
    [SerializeField] private int _potionSpawnCount = 1;
    [SerializeField] private float _potionSpawnVerticalForce = 3f;
    [SerializeField] private float _potionSpawnHorizontalForce = 5f;
    [SerializeField] private float _potionSpawnInterval = 0.5f;
    [SerializeField] private BaseIngredientType _defaultBaseIngredient;

    [Header("Explosion")]
    [SerializeField] private float _explosionRadius = 5f;
    [SerializeField] private float _explosionForce = 12f;
    [SerializeField] private Status _explosionStatus;
    [SerializeField] private float _explosionStatusDuration = 5f;
    
    private List<int> _contents = new();
    private bool _isBrewing;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (!other.TryGetComponent(out Ingredient ingredient)) return;

        _contents.Add((int) ingredient.Type);
        ingredient.NetworkObject.Despawn();
    }

    public void Interact()
    {
        BrewServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void BrewServerRpc()
    {
        if (_contents.Count == 0 || _isBrewing) return;

        if (!BuildPotion(out int baseIngredientId, out List<int> modifierIngredientIds))
        { 
            Explode();
        }
        else
        {
            StartCoroutine(SpawnPotions(baseIngredientId, modifierIngredientIds));        
        }

        _contents.Clear();
    }

    private bool BuildPotion(out int baseIngredientId, out List<int> modifierIngredientIds)
    {
        baseIngredientId = (int) _defaultBaseIngredient;
        modifierIngredientIds = new(); 

        foreach (int id in _contents)
        {
            IngredientType type = (IngredientType) id;
            if (type is BaseIngredientType)
            {
                if (baseIngredientId != (int) _defaultBaseIngredient) return false;
                baseIngredientId = id;
            }
            else if (type is ModifierIngredientType)
            {
                if (modifierIngredientIds.Count >= 3 || modifierIngredientIds.Contains(id)) return false;
                modifierIngredientIds.Add(id);
            }
        }

        return true;
    }
    
    private IEnumerator SpawnPotions(int baseIngredientId, List<int> modifierIngredientIds)
    {
        _isBrewing = true;

        for (int i = 0; i < _potionSpawnCount; i++)
        {
            Potion potion = Instantiate(_potionPrefab, _potionSpawnPoint.position, Quaternion.identity);
            potion.Initialize(baseIngredientId, modifierIngredientIds);
            potion.NetworkObject.Spawn(); 
            
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            Vector3 horizontalDirection = new Vector3(randomCircle.x, 0f, randomCircle.y);
            Vector3 force = Vector3.up * _potionSpawnVerticalForce + horizontalDirection * _potionSpawnHorizontalForce;
            potion.Rb.AddForce(force, ForceMode.Impulse);

            if (i < _potionSpawnCount - 1)
                yield return new WaitForSeconds(_potionSpawnInterval);
        }

        _isBrewing = false;
    }

    private void Explode()
    {
        if(!IsServer) return;

        foreach (var collider in Physics.OverlapSphere(_potionSpawnPoint.position, _explosionRadius))
        {
            if (collider.TryGetComponent(out Rigidbody rb))
                rb.AddExplosionForce(_explosionForce, _potionSpawnPoint.position, _explosionRadius, 1f, ForceMode.Impulse);

            if (collider.TryGetComponent(out StatusAffectable statusAffectable))
                statusAffectable.AddStatus(_explosionStatus, _explosionStatusDuration);
        }
    }
}
