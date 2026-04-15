using System.Collections;
using UnityEngine;



//  All pickup/drop physics are handled by PlayerInteraction.
//  This script manages growth state only:
//    SetHeld(true)  → pause growth
//    SetHeld(false) → resume growth


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
    public GameObject[] ingredientPrefabs;
    public Transform harvestSpawnPoint;

    // ── Internal ──
    public bool IsHeld { get; private set; }
    private IngredientType _plantedType;
    private float _growProgress;
    private Coroutine _growRoutine;
    private bool _harvestCooldown;

    // Called by PlayerInteraction on grab/release.
    // Pauses/resumes growth — no physics logic here.
    public void SetHeld(bool held)
    {
        IsHeld = held;

        if (held)
        {
            // Pause growth
            if (_growRoutine != null)
            {
                StopCoroutine(_growRoutine);
                _growRoutine = null;
                Debug.Log($"<color=green>[Pot]</color> Growth paused at {_growProgress:F1}/{growTime}s");
            }
        }
        else
        {
            // Resume growth if we were growing
            if (_state == PotState.Growing)
            {
                _growRoutine = StartCoroutine(GrowRoutine());
                Debug.Log("<color=green>[Pot]</color> Growth resumed");
            }
        }
    }


    //  PLANTING
    public void ReceiveIngredient(Ingredient ingredient)
    {
        if (_state != PotState.Empty || _harvestCooldown) return;
        if (ingredient.IsHeld) return;

        _plantedType = ingredient.type;
        _growProgress = 0f;
        _state = PotState.Growing;

        Destroy(ingredient.gameObject);

        Debug.Log($"<color=green>[Pot]</color> Planted {_plantedType}! Growing for {growTime}s...");

        if (!IsHeld)
        {
            _growRoutine = StartCoroutine(GrowRoutine());
        }
    }

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

        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = Color.green;
    }

    //  HARVEST
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
            Rigidbody rb = spawned.GetComponent<Rigidbody>();
            if (rb != null) rb.AddForce(Vector3.up * 2f + offset.normalized, ForceMode.Impulse);
        }

        Debug.Log($"<color=green>[Pot]</color> Harvested 2x {_plantedType}!");

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
