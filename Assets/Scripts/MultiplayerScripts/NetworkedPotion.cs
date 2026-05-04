using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.Timeline;


//  Potion.cs — Potion vial that breaks on impact
//
//   MATRIX:
//    No Base  → ~3 unit radius instantaneous explosion, small knockback
//    Cloud    → ~3 unit radius sphere, 120s
//    Object   → ~2 unit length physics cube, 120s
//    Puddle   → ~3 unit radius circular puddle on surface, 120s
//
//  MODIFIER:
//    Radius → ~5 units; Cube side → ~4 units

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Renderer))]
public class NetworkedPotion : NetworkBehaviour
{

    [Header("Network Prefabs for each Core type")]
    [SerializeField] private GameObject cloudZonePrefab;
    [SerializeField] private GameObject puddleZonePrefab;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private GameObject PoofVFXPrefab;

    [Header("Delivery: Radius (Cloud / Puddle / No-base)")]
    public float baseRadius = 3f;           // 3 units
    public float sizedRadius = 5f;          // Size 5 units

    [Header("Delivery: Cube (Object base)")]
    public float baseCubeSize = 2f;         // 2 unit length
    public float sizedCubeSize = 4f;        // Size 4 units

    [Header("Duration")]
    public float deliveryDuration = 120f;   // 120s

    [Header("No-Base Burst")]
    public float burstKnockback = 6f;       // small knockback

    [Header("Break Threshold")]
    public float breakSpeed = 3f;

    private Rigidbody _rb;

    private Renderer _renderer;

    public NetworkVariable<PotionRecipe> _recipe = new();

    

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
    }

    // Only server should be able to call this. 
    public void InitializeServer(PotionRecipe recipe)
    {
        Debug.Log($"<color=magenta>[NetworkedPotion]</color> Initializing potion with Recipe: {recipe}");
        _recipe.Value = recipe;
    }

    public override void OnNetworkSpawn()
    {
        TintVial();
    }

    // Gravity is managed by PlayerInteraction on grab/throw.
    // No OnPickedUp() needed here.
    
    //  COLLISION → BREAK
    /// <summary>
    /// OnCollisionEnter is called when the bottle collides with something else. 
    /// Aside from that, it is all handled Server-side. 
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return; 
        // Only Server should call this function, client's don't touch this.

        if (collision.relativeVelocity.magnitude < breakSpeed) return;

        ContactPoint contact = collision.GetContact(0);

        ExplodeEffectServer(contact.point, collision.gameObject);

        // Despawn the bottle across the network
        GetComponent<NetworkObject>().Despawn();
        
    }

    // handle the exploding stuff here. 
    private void ExplodeEffectServer(Vector3 hitPoint, GameObject hitObj)
    {
        if (_recipe == null)
        {
            Debug.LogWarning("[Potion] No recipe set!");
            return;
        }

        bool hasSize = _recipe.Value.HasModifier(IngredientType.Size);
        float radius = hasSize ? sizedRadius : baseRadius;
        float cubeSize = hasSize ? sizedCubeSize : baseCubeSize;

        if (!_recipe.Value.HasBase)
        {
            HandleInstantBurst(hitPoint, radius);
        }
        else
        {
            switch (_recipe.Value.Base)
            {
                case IngredientType.Cloud:
                    //
                    break;
                case IngredientType.Puddle:
                    break;
                case IngredientType.Object:
                    SpawnNetworkedObject(hitPoint, cubeSize);
                    break;
            }
        }
        ApplyDirectHit(hitObj);
        Debug.Log($"<color=magenta>[Potion]</color> {_recipe} broke at {hitPoint}");
    }

    /// <summary>
    /// Called by the Server in the instance where an 
    /// Instant Burst effect is caused by the breaking of a potion.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    private void HandleInstantBurst(Vector3 center, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius);
        foreach (var col in hits)
        {
            if (col.TryGetComponent(out Rigidbody rb) && !rb.isKinematic)
            {
                rb.AddExplosionForce(burstKnockback, center, radius, 0.5f, ForceMode.Impulse);
            }
        }

        if (_recipe.Value.ModifierCount > 0)
        {
            var Modifiers = RecipeAsList();

            PotionModifierHandler.ApplyModifiers(hits, Modifiers, center, DeliveryContext.InstantBurst);
        }

        PlayBurstFxClientRpc(center);
    }

    private List<IngredientType> RecipeAsList()
    {
        return new List<IngredientType>
            {
                _recipe.Value.Mod1,
                _recipe.Value.Mod2,
                _recipe.Value.Mod3
            }.GetRange(0, _recipe.Value.ModifierCount); // Get only active modifiers
    }

    /// <summary>
    /// Server spawns an Object with its specified recipe configurations.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="size"></param>
    private void SpawnNetworkedObject(Vector3 position, float size)
    {
        if (cubePrefab == null)
        {
            Debug.Log("<color=red>Missing Cubeprefab in NetworkingPotion.cs!! </color>");
            return;
        }
        GameObject cube = Instantiate(cubePrefab, position+Vector3.up * (size * 0.5f), Quaternion.identity);
        cube.transform.localScale = Vector3.one * size;

        var delivery = cube.GetComponent<PotionDeliveryCube>();
        var Modifiers = RecipeAsList();
        delivery.Configure(size, deliveryDuration, Modifiers);

        cube.GetComponent<NetworkObject>().Spawn();
    }

    /// <summary>
    /// Apply effects Directly to a hit target. 
    /// </summary>
    /// <param name="hitObj"></param>
    private void ApplyDirectHit(GameObject hitObj)
    {
        var Modifiers = RecipeAsList();
       if (Modifiers.Count == 0) return;
       if (hitObj.TryGetComponent(out Collider hitCol))
        {
            PotionModifierHandler.ApplyModifiers(
                new Collider[] {hitCol}, 
                Modifiers, 
                hitObj.transform.position, 
                DeliveryContext.InstantBurst
                );
        }
    }

    [ClientRpc] private void PlayBurstFxClientRpc(Vector3 center)
    {
        Instantiate(PoofVFXPrefab, center, Quaternion.identity);
    }


    //  VISUAL TINT
    private void TintVial()
    {
        Color baseColor = Color.gray;
        if (_recipe.Value.HasBase)
        {
            switch (_recipe.Value.Base)
            {
                case IngredientType.Cloud:  baseColor = new Color(0.7f, 0.7f, 1f); break;
                case IngredientType.Object: baseColor = new Color(0.6f, 0.4f, 0.2f); break;
                case IngredientType.Puddle: baseColor = new Color(0.3f, 0.1f, 0.8f); break;
            }
        }

        if (_recipe.Value.HasModifier(IngredientType.Fire))
            baseColor = Color.Lerp(baseColor, Color.red, 0.5f);
        else if (_recipe.Value.HasModifier(IngredientType.Float))
            baseColor = Color.Lerp(baseColor, Color.white, 0.4f);
        else if (_recipe.Value.HasModifier(IngredientType.Bouncy))
            baseColor = Color.Lerp(baseColor, Color.green, 0.4f);
        else if (_recipe.Value.HasModifier(IngredientType.Magnetic))
            baseColor = Color.Lerp(baseColor, Color.magenta, 0.4f);
        else if (_recipe.Value.HasModifier(IngredientType.Sparkle))
            baseColor = Color.Lerp(baseColor, Color.yellow, 0.4f);

        _renderer.material.color = baseColor;
    }
}
