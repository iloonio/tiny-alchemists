using Unity.Netcode;
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
public class NetworkedPotion : NetworkBehaviour
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
        ExplodeEffectServerRpc(contact.point);
        Destroy(gameObject);
    }

    // handle the exploding stuff here. 
    [ServerRpc] private void ExplodeEffectServerRpc(Vector3 hitPoint)
    {
        if (_recipe == null)
        {
            Debug.LogWarning("[Potion] No recipe set!");
            return;
        }

        Debug.Log($"<color=magenta>[Potion]</color> {_recipe} broke at {hitPoint}");
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
