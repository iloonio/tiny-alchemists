using System;
using System.Collections;
using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════
//  PlantPot.cs — Farmable ingredient duplicator (Network-aware)
//
//  Works in both single-player and networked scenes.
//  If the ingredient has a NetworkObject, uses Despawn/Spawn.
//  If not, falls back to Destroy/Instantiate.
// ═══════════════════════════════════════════════════════════════

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlantPot : MonoBehaviour
{
    public enum PotState { Empty, Growing, Grown }

    [Header("State")]
    [SerializeField] private PotState _state = PotState.Empty;
    public PotState State => _state;

    [Header("Growth Settings")]
    public float growTime = 15f;

    [Header("Harvest")]
    [Tooltip("Networked ingredient prefabs, one per IngredientType in enum order")]
    public GameObject[] ingredientPrefabs;
    public Transform harvestSpawnPoint;

    public bool IsHeld { get; private set; }
    private IngredientType _plantedType;
    private float _growProgress;
    private Coroutine _growRoutine;
    private bool _harvestCooldown;

    // ── Held state (called by PlayerInteraction) ──

    public void SetHeld(bool held)
    {
        IsHeld = held;

        if (held)
        {
            if (_growRoutine != null)
            {
                StopCoroutine(_growRoutine);
                _growRoutine = null;
            }
        }
        else
        {
            if (_state == PotState.Growing)
                _growRoutine = StartCoroutine(GrowRoutine());
        }
    }

    // ── Planting ──

    public void ReceiveIngredient(Ingredient ingredient)
    {
        if (_state != PotState.Empty || _harvestCooldown) return;
        if (ingredient.IsHeld) return;

        _plantedType = ingredient.type;
        _growProgress = 0f;
        _state = PotState.Growing;

        // Despawn networked or Destroy local
        var netObj = ingredient.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
            netObj.Despawn();
        else
            Destroy(ingredient.gameObject);

        UnityEngine.Debug.Log($"<color=green>[Pot]</color> Planted {_plantedType}! Growing for {growTime}s...");

        if (!IsHeld)
            _growRoutine = StartCoroutine(GrowRoutine());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ingredient")) return;
        Ingredient ing = other.GetComponent<Ingredient>();
        if (ing != null) ReceiveIngredient(ing);
    }

    // ── Growth ──

    private IEnumerator GrowRoutine()
    {
        while (_growProgress < growTime)
        {
            _growProgress += Time.deltaTime;
            yield return null;
        }

        _state = PotState.Grown;
        _growRoutine = null;
        UnityEngine.Debug.Log($"<color=green>[Pot]</color> {_plantedType} is ready to harvest!");

        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = Color.green;
    }

    // ── Harvest ──

    public bool TryHarvest()
    {
        if (_state != PotState.Grown) return false;

        int prefabIndex = (int)_plantedType;
        if (ingredientPrefabs == null || prefabIndex >= ingredientPrefabs.Length
            || ingredientPrefabs[prefabIndex] == null)
        {
            UnityEngine.Debug.LogError($"[Pot] Missing prefab for {_plantedType} at index {prefabIndex}!");
            return false;
        }

        Vector3 spawnPos = harvestSpawnPoint != null
            ? harvestSpawnPoint.position
            : transform.position + Vector3.up;

        for (int i = 0; i < 2; i++)
        {
            Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), 0.2f * i, UnityEngine.Random.Range(-0.3f, 0.3f));
            GameObject spawned = Instantiate(ingredientPrefabs[prefabIndex], spawnPos + offset, Quaternion.identity);

            // Spawn on network if it has NetworkObject
            var netObj = spawned.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Spawn();

            Rigidbody rb = spawned.GetComponent<Rigidbody>();
            if (rb != null) rb.AddForce(Vector3.up * 2f + offset.normalized, ForceMode.Impulse);
        }

        UnityEngine.Debug.Log($"<color=green>[Pot]</color> Harvested 2x {_plantedType}!");

        _state = PotState.Empty;
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = Color.white;

        StartCoroutine(HarvestCooldownRoutine());
        return true;
    }

    private IEnumerator HarvestCooldownRoutine()
    {
        _harvestCooldown = true;
        yield return new WaitForSeconds(2f);
        _harvestCooldown = false;
    }
}