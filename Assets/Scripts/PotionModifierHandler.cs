using System.Collections.Generic;
using UnityEngine;


//  PotionModifierHandler.cs — Applies modifier effects to targets
//
// Bbehavior differs based on delivery type.
//  Handles all Special Interactions:
//    Fire+Sparkle   → Repeated explosions with knockback
//    Size+Magnetic  → Increased mass (automatic via heavier objects)
//    Bouncy+Magnetic → Repels instead of attracts
//
//  Called by:
//    - Potion.cs (no-base instant burst, one-shot)
//    - PotionDeliveryZone.cs (Cloud/Puddle, per-tick)
//    - PotionDeliveryCube.cs (Object, per-tick for magnetic)

public enum DeliveryContext
{
    InstantBurst,   // No base
    Cloud,          // Cloud zone
    Puddle,         // Puddle zone
    ObjectCube      // Physics cube (contact-based, mostly handled by cube itself)
}

public static class PotionModifierHandler
{
    // Apply all modifiers to hits. Context determines behavior differences.
    public static void ApplyModifiers(
        Collider[] hits,
        List<IngredientType> modifiers,
        Vector3 zoneCenter,
        DeliveryContext context = DeliveryContext.InstantBurst)
    {
        // Detect Special Interactions
        bool hasFireSparkle = modifiers.Contains(IngredientType.Fire) && modifiers.Contains(IngredientType.Sparkle);
        bool hasBouncyMagnetic = modifiers.Contains(IngredientType.Bouncy) && modifiers.Contains(IngredientType.Magnetic);

        foreach (var mod in modifiers)
        {
            switch (mod)
            {
                case IngredientType.Fire:
                    if (hasFireSparkle)
                        break; // Handled by special interaction below
                    ApplyFire(hits);
                    break;

                case IngredientType.Float:
                    ApplyFloat(hits, context, zoneCenter);
                    break;

                case IngredientType.Bouncy:
                    ApplyBouncy(hits, zoneCenter, context);
                    break;

                case IngredientType.Magnetic:
                    ApplyMagnetic(hits, hasBouncyMagnetic);
                    break;

                case IngredientType.Sparkle:
                    if (hasFireSparkle)
                        break; // Handled by special interaction below
                    ApplySparkle(hits);
                    break;

                // Size is handled at spawn time (scales zone/cube)
            }
        }

        // Special Interaction: Fire + Sparkle
        if (hasFireSparkle)
        {
            ApplyFireSparkleExplosion(hits, zoneCenter);
        }
    }


    //  INDIVIDUAL MODIFIERS
    
    // ── FIRE ──
    // Player fire effect: faster movement, random horizontal push, miasma immunity
    private static void ApplyFire(Collider[] hits)
    {
        foreach (var col in hits)
        {
            var sem = col.GetComponent<StatusEffectManager>();
            if (sem != null && !sem.IsOnFire)
            {
                sem.ApplyFire();
                continue;
            }

            if (col.TryGetComponent<FlammableObject>(out var flam)) flam.IgniteServer();
        }
    }

    // ── FLOAT ──
    //   No-base: "unaffected by gravity for ~5s" (one-shot duration)
    //   Cloud:   "affected players are unaffected by gravity" (continuous while in zone)
    //   Puddle:  "continuous force pushing away from puddle" (upward push)
    private static void ApplyFloat(Collider[] hits, DeliveryContext context, Vector3 zoneCenter)
    {
        foreach (var col in hits)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            StatusEffectManager sem = col.GetComponent<StatusEffectManager>();

            switch (context)
            {
                case DeliveryContext.InstantBurst:
                    // One-shot: apply 5s float status via StatusEffectManager
                    if (sem != null)
                        sem.ApplyFloat(5f);
                    else if (rb != null)
                        rb.useGravity = false; // non-player objects just lose gravity
                    break;

                case DeliveryContext.Cloud:
                    // Continuous: disable gravity while inside (re-applied each tick)
                    if (sem != null)
                        sem.ApplyFloat(1.5f); // short duration, re-applied each tick
                    else if (rb != null)
                        rb.AddForce(Vector3.up * (Physics.gravity.magnitude + 1f), ForceMode.Acceleration);
                    break;

                case DeliveryContext.Puddle:
                    // Upward push from surface, sufficient to counteract gravity
                    if (rb != null && !rb.isKinematic)
                        rb.AddForce(Vector3.up * (Physics.gravity.magnitude + 3f), ForceMode.Acceleration);
                    break;
            }
        }
    }

    // ── BOUNCY ──
    //   No-base: "increased knockback effect"
    //   Cloud:   "affected players are bouncy" (status effect)
    //   Puddle:  "surface is bouncy" (upward impulse on contact)
    //   Object:  Handled by PotionDeliveryCube (PhysicsMaterial)
    private static void ApplyBouncy(Collider[] hits, Vector3 center, DeliveryContext context)
    {
        foreach (var col in hits)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            StatusEffectManager sem = col.GetComponent<StatusEffectManager>();

            switch (context)
            {
                case DeliveryContext.InstantBurst:
                    // Increased knockback away from center
                    if (rb != null && !rb.isKinematic)
                    {
                        Vector3 dir = (col.transform.position - center).normalized;
                        if (dir.sqrMagnitude < 0.01f) dir = Vector3.up;
                        rb.AddForce((dir + Vector3.up * 0.5f) * 15f, ForceMode.Impulse);
                    }
                    break;

                case DeliveryContext.Cloud:
                    // Make players bouncy (status effect with PhysicsMaterial)
                    if (sem != null)
                        sem.ApplyBouncy(1.5f); // re-applied each tick
                    break;

                case DeliveryContext.Puddle:
                    // Surface bounces things upward
                    if (rb != null && !rb.isKinematic)
                    {
                        // Only bounce if moving downward toward puddle
                        float verticalSpeed = rb.linearVelocity.y;
                        if (verticalSpeed < 0)
                            rb.AddForce(Vector3.up * Mathf.Abs(verticalSpeed) * 2.5f, ForceMode.Impulse);
                    }
                    break;
            }
        }
    }

    // ── MAGNETIC ──
    // Special: Bouncy+Magnetic → repels instead
    private static void ApplyMagnetic(Collider[] hits, bool repels)
    {
        foreach (var col in hits)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb == null || rb.isKinematic) continue;

            // Register in global MagneticRegistry for mutual attraction
            MagneticRegistry.Instance.Register(rb, 1.5f, repels);
        }
    }

    // ── SPARKLE ──
    private static void ApplySparkle(Collider[] hits)
    {
        // TODO: Instantiate sparkle particle effects on targets
        // Placeholder: visual-only, no gameplay effect
    }


    //  SPECIAL INTERACTIONS

    // ── FIRE + SPARKLE: "repeated explosions with large flash and knockback" ──
    private static void ApplyFireSparkleExplosion(Collider[] hits, Vector3 center)
    {
        foreach (var col in hits)
        {
            // Knockback
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddExplosionForce(18f, center, 6f, 1.5f, ForceMode.Impulse);
            }

            // Also ignite
            var sem = col.GetComponent<StatusEffectManager>();
            if (sem != null && !sem.IsOnFire) sem.ApplyFire();

            var flam = col.GetComponent<FlammableObject>();
            if (flam != null) flam.IgniteServer();
        }

        Debug.Log("<color=yellow>[Special]</color> Fire+Sparkle EXPLOSION!");
    }
}
