using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// Cauldron
/// =============================================================
/// Manages ingredient collection, brewing logic, and potion spawning.
/// Behavior summary:
/// - Collects Ingredient objects that enter the cauldron trigger and stores their IDs in _contents.
/// - Players invoke Interact() to request a brew; this calls BrewServerRpc which runs on the server.
/// - BrewServerRpc validates the recipe via BuildPotion(). If the recipe is valid it starts
///   SpawnPotions(...) to create networked Potion objects; otherwise it calls Explode().
/// - All networked spawns/despawns, physics impulses and status applications are executed on the server only.
/// =============================================================
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

        _contents.Add((int)ingredient.Type);
        ingredient.NetworkObject.Despawn();
    }

    public void Interact()
    {
        BrewServerRpc();
    }

    /// BrewServerRpc
    /// =============================================================
    /// A Remote Procedure Call (RPC) only to the server.
    /// 1. Will return early if the contents count is 0
    /// 2. Will call Explode() if BuildPotion returns False
    /// 3. Starts Coroutine for SpawnPotions(...)
    /// 4. Clears contents of cauldron if 2. or 3. happens
    /// ==============================================================
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

    /// BuildPotion
    /// =============================================================
    /// returns THREE values:
    /// - A boolean that says whether the recipe is valid
    ///     - Recipe is valid iff it has 0-1 bases, and 0-3 modifiers
    /// - the Id of the base ingredient, ranging from 0 to 2
    /// - A list of integers with modifier ingredient Ids
    /// ==============================================================
    private bool BuildPotion(out int baseIngredientId, out List<int> modifierIngredientIds)
    {
        baseIngredientId = (int)_defaultBaseIngredient;
        modifierIngredientIds = new();

        foreach (int id in _contents)
        {
            IngredientType type = (IngredientType)id;
            if (type is BaseIngredientType)
            {
                if (baseIngredientId != (int)_defaultBaseIngredient) return false;
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

    /// SpawnPotions
    /// =============================================================
    /// Spawns a series of networked Potion objects using the provided recipe IDs.
    /// - Waits _potionSpawnInterval between each spawn, up to _potionSpawnCount times.
    /// - Instantiates the Potion prefab at _potionSpawnPoint, calls Initialize(...) with
    ///   the base and modifier ingredient IDs, then spawns the NetworkObject.
    /// - Applies an upward + random horizontal impulse using _potionSpawnVerticalForce and
    ///   _potionSpawnHorizontalForce so potions are ejected from the cauldron.
    /// - Runs on the server since it performs NetworkObject.Spawn() and physics impulses.
    /// =============================================================
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

    /// Explode
    /// =============================================================
    /// Triggers a server-side explosion centered at the potion spawn point.
    /// - Applies an explosion force (_explosionForce) to any Rigidbody within _explosionRadius.
    /// - Applies a status effect (_explosionStatus) for _explosionStatusDuration to any
    ///   StatusAffectable found in the radius.
    /// - Early-returns when not running on the server (IsServer check in method body).
    /// =============================================================
    private void Explode()
    {
        if (!IsServer) return;

        foreach (var collider in Physics.OverlapSphere(_potionSpawnPoint.position, _explosionRadius))
        {
            if (collider.TryGetComponent(out Rigidbody rb))
                rb.AddExplosionForce(_explosionForce, _potionSpawnPoint.position, _explosionRadius, 1f, ForceMode.Impulse);

            if (collider.TryGetComponent(out StatusAffectable statusAffectable))
                statusAffectable.AddStatus(_explosionStatus, _explosionStatusDuration);
        }
    }
}
