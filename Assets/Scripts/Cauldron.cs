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

    [Header("Ingredient Pooling")]
    [Tooltip("Where consumed ingredients are teleported to hide them")]
    public Transform ingredientGraveyard;

    [Header("Potion Spawn")]
    [Tooltip("How many potions a successful brew produces")]
    public int potionsToSpawn = 3;
    [Tooltip("Gentle upward pop force for spawned potions")]
    public float spawnPopForce = 2f;

    
    // The Cauldron is stored as a NetworkList to ensure that all Clients
    // are synced.
    private NetworkList<IngredientNetworkElement> _contents;

    // Add this near your _contents NetworkList declaration
    private List<NetworkObject> _physicalIngredients = new();


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
        if (!IsServer) return;
        
        if (other.TryGetComponent(out Ingredient ingredient))
        {
            if (ingredient == null || ingredient.IsHeld) return;
            
            // Using 5 for win-condition setup
            if (_contents.Count >= 5) 
            {
                Debug.Log("<color=cyan>[Cauldron]</color> Full!");
                return;
            }

            // 1. Add to synced NetworkList for recipe validation
            _contents.Add(ingredient.type);
            
            // 2. Track the physical object so we can despawn it during the brew
            _physicalIngredients.Add(ingredient.GetComponent<NetworkObject>());

            Debug.Log($"<color=cyan>[Cauldron]</color> Added {ingredient.type}. Contents: {_contents.Count}");
        }
    }

    // Crucial for physics-based
    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out Ingredient ingredient))
        {
            NetworkObject netObj = ingredient.GetComponent<NetworkObject>();
            
            // If this object was inside our tracking list and left the trigger zone
            if (_physicalIngredients.Contains(netObj))
            {
                _physicalIngredients.Remove(netObj);

                // Remove it from the synced recipe list
                // We loop backwards to safely remove the exact type that fell out
                for (int i = _contents.Count - 1; i >= 0; i--)
                {
                    // Assuming IngredientNetworkElement has a .Type property or casts to IngredientType
                    if (_contents[i].Type == ingredient.type) 
                    {
                        _contents.RemoveAt(i);
                        break; // Only remove one instance of this ingredient type!
                    }
                }

                Debug.Log($"<color=yellow>[Cauldron]</color> {ingredient.type} fell out! Contents: {_contents.Count}");
            }
        }
    } 

    //  BREW (called by Clients through PlayerInteraction)
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestBrewServerRpc()
    {
        // --- NEW DEBUG LOGGING ---
        List<string> ingredientNames = new List<string>();
        foreach (var ingredient in _contents)
        {
            // Assuming IngredientNetworkElement has a .Type property
            ingredientNames.Add(ingredient.Type.ToString()); 
        }
        string contentsString = ingredientNames.Count > 0 ? string.Join(", ", ingredientNames) : "Empty";
        
        // If nothing is in the cauldron, then nothing should happen. 
        if (_contents.Count == 0)
        {
            Debug.Log("<color=yellow>[Cauldron]</color> The cauldron must have at least 1 ingredient to brew!");
            return; // Stop here so it doesn't run validation or explode
        }

        Debug.Log($"<color=cyan>[Cauldron]</color> Brew requested! Contents ({_contents.Count}): [ {contentsString} ]");

        if (Validate() != null) 
        { 
            Explode(); 
            return; 
        }

        PotionRecipe recipe = BuildRecipe();

        SpawnPotions(recipe);

        ConsumePhysicalIngredients();

        _contents.Clear(); // Fixed: Clear() is the correct method for NetworkLists!
    }

    private void ConsumePhysicalIngredients()
    {
        if (!IsServer) return;

        // We need to collect the NetworkObjectIds to tell the clients which items to hide
        ulong[] objectsToHide = new ulong[_physicalIngredients.Count];

        for (int i = 0; i < _physicalIngredients.Count; i++)
        {
            NetworkObject netObj = _physicalIngredients[i];
            if (netObj != null && netObj.IsSpawned)
            {
                objectsToHide[i] = netObj.NetworkObjectId;

                // 1. Teleport the object. 
                // (Assuming the Ingredient has a NetworkTransform, this position syncs automatically)
                if (ingredientGraveyard != null)
                {
                    netObj.transform.position = ingredientGraveyard.position;
                }

                // 2. Disable physics on the server so it stops interacting
                if (netObj.TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
                if (netObj.TryGetComponent(out Collider col))
                {
                    col.enabled = false;
                }
            }
        }
        
        // 3. Tell ALL clients (including the host) to hide the visual meshes
        HideIngredientsRpc(objectsToHide);
        
        // Empty our tracking list for the next brew
        _physicalIngredients.Clear(); 
    }


    [Rpc(SendTo.Everyone)]
    private void HideIngredientsRpc(ulong[] objectIds)
    {
        foreach (ulong id in objectIds)
        {
            // Try to find the local instance of this networked object
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject netObj))
            {
                // Disable all renderers (in case your ingredient has multiple parts)
                Renderer[] renderers = netObj.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    r.enabled = false;
                }

                // Disable colliders on the client side too
                Collider[] colliders = netObj.GetComponentsInChildren<Collider>();
                foreach (var c in colliders)
                {
                    c.enabled = false;
                }
                
                // Just in case, ensure the client-side Rigidbody is also frozen
                if (netObj.TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }
        }
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
