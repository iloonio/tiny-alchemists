using System.Collections;
using UnityEngine;

//  PlantPot.cs — Farmable ingredient duplicator
//
//  STATE MACHINE:  Empty → Growing → Grown
//    - Empty:   Accepts an ingredient thrown into it
//    - Growing: Timer counts down (pauses when held by player)
//    - Grown:   Player interacts to harvest 2x of the planted type

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlantPot : MonoBehaviour
{
    public enum PotState { Empty, Growing, Grown }

    [Header("State")]
    [SerializeField] private PotState _state = PotState.Empty;
    public PotState State => _state;

    [Header("Growth Settings")]
    [Tooltip("Seconds to fully grow")]
    public float growTime = 15f;

    [Header("Harvest")]
    [Tooltip("One prefab per IngredientType in enum order. Assign in Inspector.")]
    public GameObject[] ingredientPrefabs;
    public Transform harvestSpawnPoint;

    public bool IsHeld { get; private set; }
    private IngredientType _plantedType;
    private float _growProgress;      // seconds elapsed
    private Coroutine _growRoutine;
    private Rigidbody _rb;
    private Collider _col;
    private bool _harvestCooldown;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }

    //  PICK-UP / DROP  (called by PlayerInteraction)
    public void OnPickedUp(Transform holdPoint)
    {
        IsHeld = true;
        _rb.isKinematic = true;
        _col.enabled = false;
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Pause growth
        if (_growRoutine != null)
        {
            StopCoroutine(_growRoutine);
            _growRoutine = null;
            Debug.Log($"<color=green>[Pot]</color> Growth paused at {_growProgress:F1}/{growTime}s");
        }
    }

    public void OnDropped(Vector3 throwForce)
    {
        IsHeld = false;
        transform.SetParent(null);
        _rb.isKinematic = false;
        _col.enabled = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.AddForce(throwForce, ForceMode.Impulse);

        // Resume growth if we were in Growing state
        if (_state == PotState.Growing)
        {
            _growRoutine = StartCoroutine(GrowRoutine());
            Debug.Log($"<color=green>[Pot]</color> Growth resumed");
        }
    }


    //  PLANTING  (ingredient lands in child trigger) Called by child TriggerZone's OnTriggerEnter relay.
    public void ReceiveIngredient(Ingredient ingredient)
    {
        if (_state != PotState.Empty || _harvestCooldown) return;
        if (ingredient.IsHeld) return;

        _plantedType = ingredient.type;
        _growProgress = 0f;
        _state = PotState.Growing;

        Destroy(ingredient.gameObject);

        Debug.Log($"<color=green>[Pot]</color> Planted {_plantedType}! Growing for {growTime}s...");

        // Only start growing if pot is on the ground (not held)
        if (!IsHeld)
        {
            _growRoutine = StartCoroutine(GrowRoutine());
        }
    }

    // Also catch direct trigger collisions on this object
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ingredient")) return;
        Ingredient ing = other.GetComponent<Ingredient>();
        if (ing != null) ReceiveIngredient(ing);
    }


    //  GROWTH
    private IEnumerator GrowRoutine()
    {
        while (_growProgress < growTime)
        {
            _growProgress += Time.deltaTime;
            yield return null;
        }

        _state = PotState.Grown;
        _growRoutine = null;
        Debug.Log($"<color=green>[Pot]</color> {_plantedType} is ready to harvest!");

        // Visual cue: tint green
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = Color.green;
    }


    //  HARVEST  (called by PlayerInteraction raycast)
    // Spawns 2 copies of the planted ingredient. Returns true if harvest succeeded.
    public bool TryHarvest()
    {
        if (_state != PotState.Grown) return false;

        int prefabIndex = (int)_plantedType;
        if (ingredientPrefabs == null || prefabIndex >= ingredientPrefabs.Length || ingredientPrefabs[prefabIndex] == null)
        {
            Debug.LogError($"[Pot] Missing prefab for {_plantedType} at index {prefabIndex}!");
            return false;
        }

        Vector3 spawnPos = harvestSpawnPoint != null ? harvestSpawnPoint.position : transform.position + Vector3.up;

        for (int i = 0; i < 2; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), 0.2f * i, Random.Range(-0.3f, 0.3f));
            GameObject spawned = Instantiate(ingredientPrefabs[prefabIndex], spawnPos + offset, Quaternion.identity);
            // Give a little pop
            Rigidbody rb = spawned.GetComponent<Rigidbody>();
            if (rb != null) rb.AddForce(Vector3.up * 2f + offset.normalized, ForceMode.Impulse);
        }

        Debug.Log($"<color=green>[Pot]</color> Harvested 2x {_plantedType}!");

        // Reset pot
        _state = PotState.Empty;
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = Color.white;

        // Cooldown so freshly spawned ingredients don't fall right back in
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
