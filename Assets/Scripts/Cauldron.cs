using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

//  Cauldron.cs — Manual-brew cauldron with validation
//
//  FLOW:
//    1. Ingredients fall into child TriggerZone → stored in _contents
//    2. Player looks at cauldron, presses Interact → PlayerInteraction
//       calls Brew()
//    3. Server Receives Brew Request, which then runs Validation:
//       ✓ Valid   → Creates a Potion Recipe from the valid ingredients,
//                  -> PotionRecipe is defined in IngredientData.
//       ✗ Invalid → Explode (AoE knockback + fire), empty cauldron
//    4. Spawns the potions across the network. at SpawnPoint
//    5. 
//
//  VALIDATION RULES:
//    - Must have >= 1 ingredient
//    - Max 1 of EACH IngredientType (no duplicates)
//    - Max 1 Base ingredient
//    - Max 3 Modifier ingredients

public class Cauldron : NetworkBehaviour
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

    
    // The Cauldron is stored as a NetworkList to ensure that all Clients
    // are synced.
    private NetworkList<IngredientNetworkElement> _contents;

    public void Awake()
    {
        _contents = new NetworkList<IngredientNetworkElement>();
    }

    public override void OnNetworkSpawn()
    {
        _contents.OnListChanged += OnContentsChanged;
    }

    public override void OnNetworkDespawn()
    {
        _contents.OnListChanged -= OnContentsChanged;
    }

    public void OnContentsChanged(NetworkListEvent<IngredientNetworkElement> changeEvent)
    {
        switch (changeEvent.Type)
        {
        case NetworkListEvent<IngredientNetworkElement>.EventType.Add:
            Debug.Log($"Added {changeEvent.Value.Type} at index {changeEvent.Index}");
            break;
            
        case NetworkListEvent<IngredientNetworkElement>.EventType.Remove:
            Debug.Log($"Removed item at index {changeEvent.Index}");
            break;

        case NetworkListEvent<IngredientNetworkElement>.EventType.Clear:
            Debug.Log("Cauldron was emptied!");
            break;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        // Only server should manage the Cauldron's state
        if (!IsServer) return;
        
        // Only Objects with an ingredient component should pass through here. 
        if (other.TryGetComponent(out Ingredient Ingredient))
        {
            if (Ingredient == null || Ingredient.IsHeld) return;
            // Accept up to 4 ingredients (1 base + 3 modifiers max)
            if (_contents.Count >= 4)
            {
                Debug.Log("<color=cyan>[Cauldron]</color> Full! Cannot add more.");
                return;
            }
            if (_contents.Count < 4)
            {
                _contents.Add(Ingredient.type);

                Debug.Log($"<color=cyan>[Cauldron]</color> Added {Ingredient.type}. Contents: {_contents.Count}");
                
                Ingredient.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    //  BREW (called by ALL Clients through PlayerInteraction)
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestBrewServerRpc()
    {
        if (Validate() != null) { Explode(); return; }

        PotionRecipe recipe = BuildRecipe();

        SpawnPotions(recipe);

        _contents = new(); //maybe this will fix it?
    }


    //  VALIDATION
    private string Validate()
    {
        if (!IsServer) return "Not Authority";

        // Duplicate check with Hash (I couldn't find any other way to make it work)
        HashSet<IngredientType> uniqueTypes = new();
        foreach (var item in _contents)
        {
            // HashSet.Add returns false if the item already exists
            if (!uniqueTypes.Add(item.Type)) 
            {
                return "Duplicate ingredient detected";
            }
        }

        int baseCount = 0;
        int modCount = 0;

        foreach (IngredientNetworkElement element in _contents)
        {
            if (IngredientHelper.GetCategory(element) == IngredientCategory.Base)
                baseCount++;
            else
                modCount++;
        }

        if (baseCount > 1) return $"Too many Bases ({baseCount}). Max 1 allowed";
        if (modCount > 3) return $"Too many Modifiers ({modCount}). Max 3 allowed";

        return null; // Valid
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
            NetworkedPotion potion = obj.GetComponent<NetworkedPotion>();
            potion.InitializeServer(recipe);
            
            // Handle potions on the network 
            if (obj.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn();
            }

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
        if(!IsServer) return;
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
            if (flam != null) flam.IgniteServer();
        }
    }
}
