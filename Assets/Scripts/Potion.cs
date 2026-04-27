using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

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
public class Potion : MonoBehaviour
{
    private PotionRecipe _recipe;

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

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
    }

    public void Initialize(PotionRecipe recipe)
    {
        _recipe = recipe;
        TintVial();
    }

    // Gravity is managed by PlayerInteraction on grab/throw.
    // No OnPickedUp() needed here.
    
    //  COLLISION → BREAK
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude < breakSpeed) return;

        ContactPoint contact = collision.GetContact(0);
        ExplodeEffect(contact.point, contact.normal, collision.gameObject);
        Destroy(gameObject);
    }

    //  EFFECT DISPATCH
    private void ExplodeEffect(Vector3 hitPoint, Vector3 hitNormal, GameObject hitObj)
    {
        if (_recipe == null)
        {
            Debug.LogWarning("[Potion] No recipe set!");
            return;
        }

        Debug.Log($"<color=magenta>[Potion]</color> {_recipe} broke at {hitPoint}");

        bool hasSize = _recipe.HasModifier(IngredientType.Size);
        float radius = hasSize ? sizedRadius : baseRadius;
        float cubeSize = hasSize ? sizedCubeSize : baseCubeSize;

        if (!_recipe.Base.HasValue)
        {
            HandleInstantBurst(hitPoint, radius);
        }
        else
        {
            switch (_recipe.Base.Value)
            {
                case IngredientType.Cloud:
                    SpawnZone(DeliveryShape.Cloud, hitPoint, hitNormal, radius);
                    break;
                case IngredientType.Object:
                    SpawnCube(hitPoint, cubeSize);
                    break;
                case IngredientType.Puddle:
                    SpawnZone(DeliveryShape.Puddle, hitPoint, hitNormal, radius);
                    break;
            }
        }

        // Direct-hit: apply modifier effects to whatever the bottle struck
        ApplyDirectHit(hitObj);
    }
    
    //  NO BASE: INSTANT BURST
    private void HandleInstantBurst(Vector3 center, float radius)
    {
        Debug.Log("<color=yellow>[Potion]</color> Instant burst!");
        Collider[] hits = Physics.OverlapSphere(center, radius);

        // 3 unit radius instantaneous explosion; small knockback" (even with no modifiers)
        foreach (var col in hits)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddExplosionForce(burstKnockback, center, radius, 0.5f, ForceMode.Impulse);
            }
        }

        // Then apply modifier effects on top
        if (_recipe.Modifiers.Count > 0)
        {
            PotionModifierHandler.ApplyModifiers(
                hits,
                _recipe.Modifiers,
                center,
                DeliveryContext.InstantBurst
            );
        }
    }

    //  DELIVERY SPAWNERS  
    private void SpawnZone(DeliveryShape shape, Vector3 position, Vector3 surfaceNormal, float radius)
    {
        PrimitiveType primitive = (shape == DeliveryShape.Cloud)
            ? PrimitiveType.Sphere
            : PrimitiveType.Cylinder;

        GameObject zone = GameObject.CreatePrimitive(primitive);
        zone.name = $"PotionZone_{shape}";
        zone.transform.position = position;

        Collider col = zone.GetComponent<Collider>();
        if (col != null) Destroy(col);

        PotionDeliveryZone delivery = zone.AddComponent<PotionDeliveryZone>();
        delivery.Configure(shape, radius, deliveryDuration, _recipe.Modifiers, surfaceNormal);
    }

    private void SpawnCube(Vector3 position, float size)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "PotionCube";
        cube.transform.position = position + Vector3.up * (size * 0.5f);

        PotionDeliveryCube delivery = cube.AddComponent<PotionDeliveryCube>();
        delivery.Configure(size, deliveryDuration, _recipe.Modifiers);

        // Spawning the cube on the network
        NetworkObject NetObjCube = cube.AddComponent<NetworkObject>();
        cube.AddComponent<NetworkTransform>();

        NetObjCube.Spawn();
    }

    private void ApplyDirectHit(GameObject hitObj)
    {
        if (_recipe.Modifiers.Count == 0) return;

        Collider hitCol = hitObj.GetComponent<Collider>();
        if (hitCol == null) return;

        PotionModifierHandler.ApplyModifiers(
            new Collider[] { hitCol },
            _recipe.Modifiers,
            hitObj.transform.position,
            DeliveryContext.InstantBurst
        );
    }

    //  VISUAL TINT
    private void TintVial()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

        Color baseColor = Color.gray;
        if (_recipe.Base.HasValue)
        {
            switch (_recipe.Base.Value)
            {
                case IngredientType.Cloud:  baseColor = new Color(0.7f, 0.7f, 1f); break;
                case IngredientType.Object: baseColor = new Color(0.6f, 0.4f, 0.2f); break;
                case IngredientType.Puddle: baseColor = new Color(0.3f, 0.1f, 0.8f); break;
            }
        }

        if (_recipe.HasModifier(IngredientType.Fire))
            baseColor = Color.Lerp(baseColor, Color.red, 0.5f);
        else if (_recipe.HasModifier(IngredientType.Float))
            baseColor = Color.Lerp(baseColor, Color.white, 0.4f);
        else if (_recipe.HasModifier(IngredientType.Bouncy))
            baseColor = Color.Lerp(baseColor, Color.green, 0.4f);
        else if (_recipe.HasModifier(IngredientType.Magnetic))
            baseColor = Color.Lerp(baseColor, Color.magenta, 0.4f);
        else if (_recipe.HasModifier(IngredientType.Sparkle))
            baseColor = Color.Lerp(baseColor, Color.yellow, 0.4f);

        rend.material.color = baseColor;
    }
}
