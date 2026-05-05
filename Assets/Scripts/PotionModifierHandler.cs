using System.Collections.Generic;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════
//  PotionModifierHandler.cs — Applies modifier effects to targets
//
//  ARCHITECTURE:
//    This is called ONLY on the server (by NetworkedPotion,
//    PotionDeliveryCube, etc.). It modifies world state:
//      - Calls StatusEffectManager.ApplyFire() (which sets NetworkVariables)
//      - Applies physics forces (AddForce, knockback)
//      - Calls FlammableObject.IgniteServer()
//
//    Clients never call this directly — they react to NetworkVariable
//    changes set by the server through StatusEffectManager.
//
//  CURRENT SCOPE: Fire + Size (Size handled at spawn time)
//  Other modifiers are stubbed for future implementation.
// ═══════════════════════════════════════════════════════════════

public enum DeliveryContext
{
    InstantBurst,
    Cloud,
    Puddle,
    ObjectCube
}

public static class PotionModifierHandler
{
    /// <summary>
    /// Apply all modifiers to hit colliders. SERVER ONLY.
    /// </summary>
    public static void ApplyModifiers(
        Collider[] hits,
        List<IngredientType> modifiers,
        Vector3 zoneCenter,
        DeliveryContext context = DeliveryContext.InstantBurst)
    {
        bool hasFireSparkle = modifiers.Contains(IngredientType.Fire)
                           && modifiers.Contains(IngredientType.Sparkle);

        foreach (var mod in modifiers)
        {
            switch (mod)
            {
                case IngredientType.Fire:
                    if (!hasFireSparkle) ApplyFire(hits);
                    break;

                case IngredientType.Float:
                    // TODO: ApplyFloat (future scope)
                    break;

                case IngredientType.Bouncy:
                    // TODO: ApplyBouncy (future scope)
                    break;

                case IngredientType.Magnetic:
                    // TODO: ApplyMagnetic (future scope)
                    break;

                case IngredientType.Sparkle:
                    // TODO: ApplySparkle (future scope)
                    break;

                // Size is handled at spawn time (scales zone/cube), not per-tick
            }
        }

        if (hasFireSparkle)
            ApplyFireSparkleExplosion(hits, zoneCenter);
    }

    // ── FIRE ──
    private static void ApplyFire(Collider[] hits)
    {
        foreach (var col in hits)
        {
            // Players / entities with StatusEffectManager
            var sem = col.GetComponent<StatusEffectManager>();
            if (sem != null && !sem.IsOnFireNet.Value)
            {
                sem.ApplyFire(); // Server-only; sets NetworkVariable
                continue;
            }

            // Flammable environment objects
            if (col.TryGetComponent<FlammableObject>(out var flam))
                flam.IgniteServer();
        }
    }

    // ── FIRE + SPARKLE SPECIAL ──
    private static void ApplyFireSparkleExplosion(Collider[] hits, Vector3 center)
    {
        foreach (var col in hits)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
                rb.AddExplosionForce(18f, center, 6f, 1.5f, ForceMode.Impulse);

            var sem = col.GetComponent<StatusEffectManager>();
            if (sem != null && !sem.IsOnFireNet.Value) sem.ApplyFire();

            if (col.TryGetComponent<FlammableObject>(out var flam))
                flam.IgniteServer();
        }

        Debug.Log("<color=yellow>[Special]</color> Fire+Sparkle EXPLOSION!");
    }
}
