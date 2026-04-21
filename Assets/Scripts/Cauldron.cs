using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//  Cauldron.cs — Manual-brew cauldron with validation
//
//  FLOW:
//    1. Ingredients fall into child TriggerZone → stored in _contents
//    2. Player looks at cauldron, presses Interact → PlayerInteraction
//       calls Brew()
//    3. Validation runs:
//       ✓ Valid   → Spawn 1 identical potions at SpawnPoint
//       ✗ Invalid → Explode (AoE knockback + fire), empty cauldron
//
//  VALIDATION RULES:
//    - Must have >= 1 ingredient
//    - Max 1 of EACH IngredientType (no duplicates)
//    - Max 1 Base ingredient
//    - Max 3 Modifier ingredients

public class Cauldron : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject potionPrefab;
    public Transform potionSpawnPoint;

    [Header("Explosion (Failed Brew)")]
    [Tooltip("Knockback radius on failed brew")]
    public float explosionRadius = 5f;
    [Tooltip("Knockback force on failed brew")]
    public float explosionForce = 12f;
    [Tooltip("Fire radius on failed brew")]
    public float fireRadius = 4f;

    [Header("Potion Spawn")]
    [Tooltip("How many potions a successful brew produces")]
    public int potionsToSpawn = 3;
    [Tooltip("Gentle upward pop force for spawned potions")]
    public float spawnPopForce = 2f;

  
    private List<IngredientType> _contents = new List<IngredientType>();


    public IReadOnlyList<IngredientType> Contents => _contents;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ingredient")) return;

        Ingredient item = other.GetComponent<Ingredient>();
        if (item == null || item.IsHeld) return;

        // Accept up to 4 ingredients (1 base + 3 modifiers max)
        if (_contents.Count >= 4)
        {
            Debug.Log("<color=cyan>[Cauldron]</color> Full! Cannot add more.");
            return;
        }

        _contents.Add(item.type);
        Debug.Log($"<color=cyan>[Cauldron]</color> Added {item.type}. Contents: {_contents.Count}");
        Destroy(item.gameObject);
    }


    //  BREW (called by PlayerInteraction)
    public bool Brew()
    {
        if (_contents.Count == 0)
        {
            Debug.Log("<color=cyan>[Cauldron]</color> Nothing to brew!");
            return false;
        }

        string validation = Validate();
        if (validation != null)
        {
            // INVALID
            Debug.Log($"<color=red>[Cauldron]</color> INVALID RECIPE: {validation}");
            Explode();
            _contents.Clear();
            return false;
        }

        // VALID
        PotionRecipe recipe = BuildRecipe();
        Debug.Log($"<color=cyan>[Cauldron]</color> Brewed {recipe}!");

        SpawnPotions(recipe);
        _contents.Clear();
        return true;
    }

    //  VALIDATION
    private string Validate()
    {
        // Rule: no duplicate ingredient types
        if (_contents.Count != _contents.Distinct().Count())
            return "Duplicate ingredient detected";

        int baseCount = 0;
        int modCount = 0;

        foreach (var ing in _contents)
        {
            if (IngredientHelper.GetCategory(ing) == IngredientCategory.Base)
                baseCount++;
            else
                modCount++;
        }

        // Rule: max 1 Base
        if (baseCount > 1)
            return $"Too many Bases ({baseCount}). Max 1 allowed";

        // Rule: max 3 Modifiers
        if (modCount > 3)
            return $"Too many Modifiers ({modCount}). Max 3 allowed";

        return null; // valid
    }

    //  RECIPE BUILDER
    private PotionRecipe BuildRecipe()
    {
        IngredientType? potionBase = null;
        List<IngredientType> modifiers = new List<IngredientType>();

        foreach (var ing in _contents)
        {
            if (IngredientHelper.GetCategory(ing) == IngredientCategory.Base)
                potionBase = ing;
            else
                modifiers.Add(ing);
        }

        return new PotionRecipe(potionBase, modifiers);
    }
    
    //  POTION SPAWNING
    private void SpawnPotions(PotionRecipe recipe)
    {
        Vector3 spawnPos = potionSpawnPoint != null
            ? potionSpawnPoint.position
            : transform.position + Vector3.up * 1.5f;

        for (int i = 0; i < potionsToSpawn; i++)
        {
            // Slight random spread so they don't stack perfectly
            Vector3 offset = new Vector3(
                Random.Range(-0.3f, 0.3f),
                0.1f * i,
                Random.Range(-0.3f, 0.3f)
            );

            GameObject obj = Instantiate(potionPrefab, spawnPos + offset, Quaternion.identity);
            Potion potion = obj.GetComponent<Potion>();
            potion.Initialize(recipe);

            // pop
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
            }
        }

        Debug.Log($"<color=cyan>[Cauldron]</color> Spawned {potionsToSpawn} potions.");
    }

    //  EXPLOSION (Failed Recipe)
    private void Explode()
    {
        Debug.Log("<color=red>[Cauldron]</color> BOOM! Failed recipe explosion!");

        Vector3 center = transform.position;

        // AoE Knockback
        Collider[] hits = Physics.OverlapSphere(center, explosionRadius);
        foreach (var col in hits)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, center, explosionRadius, 1f, ForceMode.Impulse);
            }
        }

        // Set surrounding flammables/players on fire
        Collider[] fireHits = Physics.OverlapSphere(center, fireRadius);
        foreach (var col in fireHits)
        {
            // Players
            StatusEffectManager sem = col.GetComponent<StatusEffectManager>();
            if (sem != null) sem.ApplyFire();

            // Flammable objects
            FlammableObject flam = col.GetComponent<FlammableObject>();
            if (flam != null) flam.Ignite();
        }
    }
}
